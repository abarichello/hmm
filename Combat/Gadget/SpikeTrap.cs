using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class SpikeTrap : BasicCannon, TriggerEnterCallback.ITriggerEnterCallbackListener
	{
		public SpikeTrapInfo TrapInfo
		{
			get
			{
				return base.Info as SpikeTrapInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			SpikeTrapInfo trapInfo = this.TrapInfo;
			this._extraDamage = ModifierData.CreateData(this.TrapInfo.ExtraModifier, this.TrapInfo);
			this._dropTime = new Upgradeable(trapInfo.DropTimeUpgrade, trapInfo.DropTime, trapInfo.UpgradesValues);
			this.NumLargerProjectile = new Upgradeable(trapInfo.LargerProjectileUpgrade, (float)trapInfo.NumLargeEffect, trapInfo.UpgradesValues);
			this.DropValueIncrease = new Upgradeable(trapInfo.DropValueIncreaseUpgrade, trapInfo.DropValueIncrease, trapInfo.UpgradesValues);
			this.FireExtraOnEnter = new Upgradeable(trapInfo.FireExtraOnEnterUpgrade, trapInfo.FireExtraOnEnter, trapInfo.UpgradesValues);
			this._lastDropPosition = Vector3.one * float.MaxValue;
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivered;
		}

		protected override void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivered;
			base.OnDestroy();
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._extraDamage.SetLevel(upgradeName, level);
			this._dropTime.SetLevel(upgradeName, level);
			this.NumLargerProjectile.SetLevel(upgradeName, level);
			this.DropValueIncrease.SetLevel(upgradeName, level);
			if (this.DropValueIncrease == 0f)
			{
				this._numHittedAfterUpgrade = 0;
				this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, this._gadgetState.GadgetState, this._gadgetState.CoolDown, this._gadgetState.Value, this._gadgetState.Heat, this.CalcDropSteps(), null);
			}
		}

		protected override int FireGadget()
		{
			Vector3 vector = this.DummyPosition();
			float num = Vector3.SqrMagnitude(vector - this._lastDropPosition);
			float num2 = this.TrapInfo.DropDistance * this.TrapInfo.DropDistance;
			if (num < num2)
			{
				return -1;
			}
			this._lastDropPosition = vector;
			int result = -1;
			Vector3 a = this.Combat.transform.right * this.TrapInfo.SpaceBetweenTraps;
			Vector3 a2 = Vector3.zero;
			if (this.TrapInfo.DropNumber % 2 == 1)
			{
				result = this.DropTrap(vector);
				a2 = this.Combat.transform.right * this.TrapInfo.SpaceBetweenTraps / 2f;
			}
			for (int i = 1; i < this.TrapInfo.DropNumber; i += 2)
			{
				Vector3 b = a2 + (float)i * a;
				this.DropTrap(vector + b);
				result = this.DropTrap(vector - b);
			}
			return result;
		}

		private int DropTrap(Vector3 position)
		{
			if (this.TrapInfo.Kind != GadgetKind.InstantWithCharges || this.ChargeCount == base.MaxChargeCount)
			{
				this._numLargeProjectile = this.NumLargerProjectile.IntGet();
			}
			EffectEvent effectEvent = (this._numLargeProjectile <= 0) ? base.GetEffectEvent(this.TrapInfo.Effect) : base.GetEffectEvent(this.TrapInfo.EffectLarger);
			effectEvent.LifeTime = base.LifeTime;
			effectEvent.Origin = position;
			effectEvent.Target = Vector3.zero;
			effectEvent.Range = this.GetRange();
			effectEvent.Direction = this.Combat.transform.forward;
			int num = this.CalcDropSteps();
			effectEvent.Modifiers = ModifierData.AddAmount(this._damage, this.DropValueIncrease * (float)num, new Func<ModifierData, bool>(this.Filter));
			effectEvent.ExtraModifiers = ModifierData.AddAmount(this._extraDamage, this.DropValueIncrease * (float)num, new Func<ModifierData, bool>(this.Filter));
			effectEvent.CustomVar = (byte)base.Radius;
			effectEvent.SourceGadget = this;
			effectEvent.SourceCombat = this.Combat;
			effectEvent.SourceSlot = base.Slot;
			effectEvent.SourceId = this.Parent.ObjId;
			this._numLargeProjectile--;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private bool Filter(ModifierData modifierData)
		{
			return modifierData.Info.Effect == EffectKind.HPHeavyDamage || modifierData.Info.Effect == EffectKind.HPLightDamage;
		}

		private int CalcDropSteps()
		{
			return (this.TrapInfo.MaxDropSteps != 0) ? Mathf.Min(this._numHittedAfterUpgrade / this.TrapInfo.DropStepsThreshold, this.TrapInfo.MaxDropSteps) : 0;
		}

		protected int FireExtraGadget(CombatObject target)
		{
			EffectEvent effectEvent = base.GetEffectEvent(this.TrapInfo.ExtraEffect);
			effectEvent.Origin = target.transform.position;
			effectEvent.TargetId = target.Id.ObjId;
			effectEvent.LifeTime = this.ExtraLifeTime;
			base.SetTargetAndDirection(target.Movement.LastVelocity.normalized, effectEvent);
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public override void OnTriggerEnterCallback(TriggerEnterCallback evt)
		{
			if (!this.FireExtraOnEnter.BoolGet())
			{
				return;
			}
			if (evt.Gadget == this && evt.Combat != null && evt.Combat != this.Combat)
			{
				if (this.TrapInfo.ExtraEffect.Exists())
				{
					this.FireExtraGadget(evt.Combat);
				}
				if (this.DropValueIncrease != 0f)
				{
					this._numHittedAfterUpgrade++;
					this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, this._gadgetState.GadgetState, this._gadgetState.CoolDown, this._gadgetState.Value, this._gadgetState.Heat, this.CalcDropSteps(), null);
				}
			}
		}

		public void OnBombDelivered(int causerId, TeamKind team, Vector3 deliveryPosition)
		{
			this._numHittedAfterUpgrade = 0;
			this._numLargeProjectile = 0;
			this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, this._gadgetState.GadgetState, this._gadgetState.CoolDown, this._gadgetState.Value, this._gadgetState.Heat, this.CalcDropSteps(), null);
		}

		public int GetNumLargerProjectile(int chargeCount)
		{
			if (chargeCount == base.MaxChargeCount)
			{
				return this.NumLargerProjectile.IntGet();
			}
			return Mathf.Max(0, this.NumLargerProjectile.IntGet() - (base.MaxChargeCount - chargeCount));
		}

		public float GetSpikeExtraDamage()
		{
			return this.DropValueIncrease * (float)this._gadgetState.Counter;
		}

		public float GetSpikeTrapMaxSteps()
		{
			return (float)this.TrapInfo.MaxDropSteps;
		}

		public bool HasSpikeDamageUpgrade()
		{
			return this.DropValueIncrease.Level > 0;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SpikeTrap));

		private ModifierData[] _extraDamage;

		private Upgradeable _dropTime;

		protected Upgradeable NumLargerProjectile;

		protected Upgradeable DropValueIncrease;

		protected Upgradeable FireExtraOnEnter;

		private Vector3 _lastDropPosition;

		private int _numLargeProjectile;

		private int _numHittedAfterUpgrade;
	}
}
