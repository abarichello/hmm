using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class GranTorinoRocket : ReflectiveProjectile, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		private GranTorinoRocketInfo MyInfo
		{
			get
			{
				return base.Info as GranTorinoRocketInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgIsUltimateUpgraded = new Upgradeable(this.MyInfo.UltimateUpgrade, this.MyInfo.IsUltimateUpgraded, this.MyInfo.UpgradesValues);
			this._upgPickupLifetime = new Upgradeable(this.MyInfo.PickupLifetimeUpgrade, this.MyInfo.PickupLifetime, this.MyInfo.UpgradesValues);
			this._upgPickupMultiplier = new Upgradeable(this.MyInfo.PickupValueMultiplierUpgrade, this.MyInfo.PickupValueMultiplier, this.MyInfo.UpgradesValues);
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivered;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivered;
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			float num = this._upgPickupMultiplier;
			base.SetLevel(upgradeName, level);
			this._upgPickupLifetime.SetLevel(upgradeName, level);
			this._upgPickupMultiplier.SetLevel(upgradeName, level);
			this._upgIsUltimateUpgraded.SetLevel(upgradeName, level);
			if (num != this._upgPickupMultiplier)
			{
				this.pickupsGot = 0;
			}
		}

		protected override int FireGadget()
		{
			if (this._upgIsUltimateUpgraded.BoolGet())
			{
				this.MyInfo.ReflectEffect = this.MyInfo.UpgradedReflectEffect;
				return base.FireExtraGadget(this.MyInfo.UpgradedEffect, this._damage, this.ExtraModifier);
			}
			this.MyInfo.ReflectEffect = this.MyInfo.NormalReflectEffect;
			return base.FireExtraGadget(this.MyInfo.Effect, this._damage, this.ExtraModifier);
		}

		public override int ForceFire()
		{
			int num = base.ForceFire();
			this.forcedShots.Add(num);
			return num;
		}

		public override void DrainCheck(CombatObject causer, CombatObject taker, ModifierData mod, float amount, int eventid)
		{
			if (this.forcedShots.Contains(eventid))
			{
				return;
			}
			base.DrainCheck(causer, taker, mod, amount, eventid);
		}

		protected override void OnMyEffectDestroyed(DestroyEffect evt)
		{
			this.forcedShots.Remove(evt.RemoveData.TargetEventId);
		}

		protected override Vector3 DummyPosition()
		{
			return base.GetValidPosition(this.Combat.transform.position, base.DummyPosition());
		}

		protected override int FireExtraGadgetOnDeath(DestroyEffect evt)
		{
			if (evt.EffectData.EffectInfo == this.MyInfo.ExtraEffect)
			{
				return -1;
			}
			if (this.pickupInOrigin == evt.RemoveData.TargetEventId)
			{
				this.pickupInOrigin = -1;
			}
			if (evt.RemoveData.SrvWasScenery || evt.RemoveData.SrvOtherCollider == null)
			{
				return -1;
			}
			CombatObject combat = CombatRef.GetCombat(evt.RemoveData.SrvOtherCollider);
			if (combat == null || combat.IsBomb)
			{
				return -1;
			}
			this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, this._gadgetState.GadgetState, this._gadgetState.CoolDown, this._gadgetState.Value, this._gadgetState.Heat, this.pickupsGot, null);
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ExtraEffect);
			effectEvent.Origin = evt.RemoveData.Origin;
			effectEvent.LifeTime = this._upgPickupLifetime.Get();
			effectEvent.Modifiers = this.BuffModifiers();
			int result = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			if (this.pickupInOrigin != -1 && GameHubBehaviour.Hub.Effects.GetBaseFx(this.pickupInOrigin) != null && (GameHubBehaviour.Hub.Effects.GetBaseFx(this.pickupInOrigin).transform.position - effectEvent.Origin).sqrMagnitude < 2f)
			{
				float f = 0.0174532924f * UnityEngine.Random.Range(-(this.MyInfo.PickupMaxSpreadAngle / 2f), this.MyInfo.PickupMaxSpreadAngle / 2f);
				float num = Mathf.Cos(f);
				float num2 = Mathf.Sin(f);
				float x = evt.EffectData.Direction.x;
				float z = evt.EffectData.Direction.z;
				Vector3 a = new Vector3(x * num - z * num2, 0f, x * num2 + z * num);
				effectEvent.Origin -= a * UnityEngine.Random.Range(0f, this.MyInfo.PickupMaxSpread);
			}
			else
			{
				this.pickupInOrigin = result;
			}
			return result;
		}

		protected override int FireExtraGadget()
		{
			return -1;
		}

		protected override void GadgetUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			base.GadgetUpdate();
			this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, this._gadgetState.GadgetState, this._gadgetState.CoolDown, this._gadgetState.Value, this._gadgetState.Heat, this.pickupsGot, null);
		}

		public ModifierData[] BuffModifiers()
		{
			return this.BuffModifiers(Mathf.Min(this._gadgetState.Counter / this.MyInfo.PickupStepsThreshold, this.MyInfo.MaxPickupSteps));
		}

		private ModifierData[] BuffModifiers(int numSteps)
		{
			float value = (float)numSteps * this._upgPickupMultiplier;
			return ModifierData.AddAmount(this.ExtraModifier, value, (ModifierData data) => data.Amount > 0f);
		}

		public bool HasPickupUpgrade()
		{
			return this._upgPickupMultiplier > 0f;
		}

		public float GetCurrentPickup()
		{
			return this.BuffModifiers(Mathf.Min(this._gadgetState.Counter / this.MyInfo.PickupStepsThreshold, this.MyInfo.MaxPickupSteps))[0].Amount;
		}

		public float GetMaxPickup()
		{
			return this.BuffModifiers(this.MyInfo.MaxPickupSteps)[0].Amount;
		}

		public void OnBombDelivered(int causerId, TeamKind team, Vector3 deliveryPosition)
		{
			this.pickupsGot = 0;
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			if (evt.Combat == this.Combat && this.HasPickupUpgrade())
			{
				this.pickupsGot++;
			}
		}

		public override float Cooldown
		{
			get
			{
				return this.GetCooldownWithoutReduction();
			}
		}

		protected Upgradeable _upgIsUltimateUpgraded;

		protected Upgradeable _upgPickupLifetime;

		protected Upgradeable _upgPickupMultiplier;

		protected Upgradeable _upgPickupMaxStep;

		private int pickupInOrigin = -1;

		private int pickupsGot;

		private HashSet<int> forcedShots = new HashSet<int>();
	}
}
