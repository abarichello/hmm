using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class FrontalRocketCharges : GadgetBehaviour, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		private FrontalRocketChargesInfo MyInfo
		{
			get
			{
				return base.Info as FrontalRocketChargesInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			FrontalRocketChargesInfo myInfo = this.MyInfo;
			this._upgPrimaryModifers = ModifierData.CreateData(myInfo.PrimaryModifiers, myInfo);
			this._upgMoveSpeed = new Upgradeable(myInfo.MoveSpeedUpgrade, myInfo.MoveSpeed, myInfo.UpgradesValues);
			this._upgChargeCount = new Upgradeable(myInfo.ChargeCountUpgrade, (float)myInfo.ChargeCount, myInfo.UpgradesValues);
			this._upgChargeTime = new Upgradeable(myInfo.ChargeTimeUpgrade, myInfo.ChargeTime, myInfo.UpgradesValues);
			this.ChargeCount = Mathf.Min(this.ChargeCount, base.MaxChargeCount);
			this._upgSecondaryModifiers = ModifierData.CreateData(myInfo.SecondaryModifiers, myInfo);
			this._upgSecondaryHits = new Upgradeable(myInfo.StunHitsUpgradeable, (float)myInfo.StunHits, myInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgPrimaryModifers.SetLevel(upgradeName, level);
			this._upgMoveSpeed.SetLevel(upgradeName, level);
			this.ChargeCount = Mathf.Min(this.ChargeCount, base.MaxChargeCount);
			this._upgSecondaryModifiers.SetLevel(upgradeName, level);
			this._upgSecondaryHits.SetLevel(upgradeName, level);
		}

		public override void Activate()
		{
			base.Activate();
			this.ChargeTime = (long)(base.MaxChargeTime * 1000f) + (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		protected override void Awake()
		{
			base.Awake();
			this.GadgetChargedUpdater = new GadgetWithChargesUpdater(GameHubBehaviour.Hub, this, new Action(base.GadgetUpdate), new Func<int>(this.FirePrimaryEffect));
			if (GameHubBehaviour.Hub)
			{
				this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		protected override void GadgetUpdate()
		{
			this.GadgetChargedUpdater.RunGadgetUpdate();
		}

		protected virtual int FirePrimaryEffect()
		{
			FrontalRocketChargesInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.PrimaryEffect);
			effectEvent.Modifiers = this._upgPrimaryModifers;
			effectEvent.MoveSpeed = this._upgMoveSpeed;
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.DummyPosition();
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			if (myInfo.Direction == FrontalRocketChargesInfo.DirectionEnum.Target)
			{
				effectEvent.Target = base.Target;
				effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			}
			else if (myInfo.Direction == FrontalRocketChargesInfo.DirectionEnum.Forward)
			{
				effectEvent.Direction = this.Combat.transform.forward;
				effectEvent.Direction.y = 0f;
				effectEvent.Direction.Normalize();
				effectEvent.Target = effectEvent.Origin + effectEvent.Direction * effectEvent.Range;
			}
			int num = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			base.OnGadgetUsed(num);
			base.ExistingFiredEffectsAdd(num);
			return num;
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
		}

		public override float GetDps()
		{
			return base.GetDpsFromModifierDataWithCustomCooldown(this._upgPrimaryModifers, this._upgChargeTime) + base.GetDpsFromModifierDataWithCustomCooldown(this._upgSecondaryModifiers, this._upgChargeTime);
		}

		protected GadgetWithChargesUpdater GadgetChargedUpdater;

		protected ModifierData[] _upgPrimaryModifers;

		protected ModifierData[] _upgSecondaryModifiers;

		protected Upgradeable _upgMoveSpeed;

		protected Upgradeable _upgChargeCount;

		protected Upgradeable _upgChargeTime;

		protected Upgradeable _upgSecondaryHits;

		private class Hit
		{
			public Hit(CombatObject combatObject, long hitMillis)
			{
				this.CombatObject = combatObject;
				this.HitMillis = hitMillis;
			}

			public CombatObject CombatObject;

			public long HitMillis;
		}
	}
}
