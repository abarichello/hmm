using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BlackLotusChargedBeam : GadgetBehaviour
	{
		public BlackLotusChargedBeamInfo MyInfo
		{
			get
			{
				return base.Info as BlackLotusChargedBeamInfo;
			}
		}

		protected float _trapLifeTime
		{
			get
			{
				return this.MyInfo.TrapLifeTime - 0.001f * (float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._dropStartMillis);
			}
		}

		protected int _chargeMax
		{
			get
			{
				return this.MyInfo.ProjectileEffect.Length - 1;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (GameHubBehaviour.Hub)
			{
				this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			this._upgChargeTime = new Upgradeable(this.MyInfo.ChargeTimeUpgrade, this.MyInfo.ChargeTime, this.MyInfo.UpgradesValues);
			this.UpdateChargeJokerRatio();
			this._upgProjectileMinDamage = new Upgradeable(this.MyInfo.ProjectileMinDamageUpgrade, this.MyInfo.ProjectileMinDamage, this.MyInfo.UpgradesValues);
			this._upgProjectileMaxDamage = new Upgradeable(this.MyInfo.ProjectileMaxDamageUpgrade, this.MyInfo.ProjectileMaxDamage, this.MyInfo.UpgradesValues);
			this._upgTrapDamage = ModifierData.CreateData(this.MyInfo.TrapDamage, this.MyInfo);
			this._upgTrapCount = new Upgradeable(this.MyInfo.TrapCountUpgrade, (float)this.MyInfo.TrapCount, this.MyInfo.UpgradesValues);
			this._upgProjectileRange = new Upgradeable(this.MyInfo.ProjectileRangeUpgrade, this.MyInfo.ProjectileRange, this.MyInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._upgChargeTime.SetLevel(upgradeName, level);
			this.UpdateChargeJokerRatio();
			this._upgProjectileMinDamage.SetLevel(upgradeName, level);
			this._upgProjectileMaxDamage.SetLevel(upgradeName, level);
			this._upgTrapDamage.SetLevel(upgradeName, level);
			this._upgTrapCount.SetLevel(upgradeName, level);
			this._upgProjectileRange.SetLevel(upgradeName, level);
		}

		private void UpdateChargeJokerRatio()
		{
			this._chargeJokerRatio = (float)this._chargeMax / this._upgChargeTime;
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.TryDropTrap();
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (this._chargeStartMillis != 0L)
			{
				this._chargeCount = (float)(this.CurrentTime - this._chargeStartMillis) * 0.001f * this._chargeJokerRatio;
				this.Combat.GadgetStates.SetJokerBarState(this._chargeCount, this._upgChargeTime.Get() * this._chargeJokerRatio);
			}
			else
			{
				this.Combat.GadgetStates.SetJokerBarState(0f, this._upgChargeTime * this._chargeJokerRatio);
				this._chargeCount = 0f;
			}
			base.GadgetUpdate();
			if (this.Combat.IsBot && Mathf.Min((float)this._chargeMax, this._chargeCount) == (float)this._chargeMax)
			{
				base.Pressed = true;
			}
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!this._isCharging)
			{
				if (!base.Pressed)
				{
					this.StartCharging();
				}
				else
				{
					this.CurrentCooldownTime = this.CurrentTime;
					if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
					{
						return;
					}
					this.StartCooldown();
					this._chargeCount = Mathf.Min((float)this._chargeMax, this._chargeCount);
					this.FireProjectile();
					if (this._upgTrapCount > 0f && this._chargeCount == (float)this._chargeMax)
					{
						this.StartDropping();
					}
				}
			}
			else if (base.Pressed)
			{
				if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
				{
					return;
				}
				this.StopCharging();
				this.StartCooldown();
				this._chargeCount = Mathf.Min((float)this._chargeMax, this._chargeCount);
				this.FireProjectile();
				if (this._upgTrapCount > 0f && this._chargeCount == (float)this._chargeMax)
				{
					this.StartDropping();
				}
			}
			else
			{
				this.CurrentCooldownTime = this.CurrentTime;
			}
		}

		private void StartCooldown()
		{
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
		}

		protected void StartCharging()
		{
			this._isCharging = true;
			this._chargeStartMillis = this.CurrentTime;
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ChargeEffect);
			effectEvent.Origin = this.Combat.Transform.position;
			effectEvent.TargetId = this.Combat.Id.ObjId;
			this._chargeEffectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		protected void StopCharging()
		{
			EffectRemoveEvent content = new EffectRemoveEvent
			{
				TargetEventId = this._chargeEffectId,
				TargetId = -1
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
			this._chargeEffectId = -1;
			this._isCharging = false;
			this._chargeStartMillis = 0L;
		}

		protected void FireProjectile()
		{
			int num = (int)this._chargeCount;
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ProjectileEffect[num]);
			effectEvent.MoveSpeed = this.MyInfo.ProjectileMoveSpeed;
			effectEvent.Range = this._upgProjectileRange;
			effectEvent.Origin = this.Combat.Transform.position;
			effectEvent.Target = base.Target;
			effectEvent.LifeTime = effectEvent.Range / effectEvent.MoveSpeed;
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.Modifiers = ModifierData.CreateConvoluted(ModifierData.CreateData(this.MyInfo.ProjectileModifiers, this.MyInfo), Mathf.Lerp(this._upgProjectileMinDamage, this._upgProjectileMaxDamage, this._chargeCount / (float)this._chargeMax));
			base.OnGadgetUsed(GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent));
		}

		protected void StartDropping()
		{
			this._trapCountMax = (int)this._upgTrapCount.Get();
			this._trapCountValue = 0;
			this._dropPosition = this.Combat.transform.position;
			this._dropDirection = base.CalcDirection(this._dropPosition, base.Target);
			this._dropUpdater = new TimedUpdater((int)(this.MyInfo.DropTick * 1000f), false, false);
			this._dropDirection.y = 0f;
			this._dropDirection.Normalize();
			this._dropStartMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this._isDropping = true;
			BlackLotusChargedBeamInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = base.GetEffectEvent(myInfo.AudioEffect);
			effectEvent.Origin = this._dropPosition + this._dropDirection * this._upgProjectileRange / 2f;
			effectEvent.LifeTime = this._trapLifeTime;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this.TryDropTrap();
		}

		protected void TryDropTrap()
		{
			if (!this._isDropping)
			{
				return;
			}
			if (this._dropUpdater.ShouldHalt())
			{
				return;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.TrapEffect);
			effectEvent.Modifiers = this._upgTrapDamage;
			effectEvent.Target = (effectEvent.Origin = this._dropPosition);
			effectEvent.Direction = this._dropDirection;
			effectEvent.Range = this.MyInfo.TrapRange;
			effectEvent.LifeTime = this._trapLifeTime;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this._dropPosition += this.MyInfo.TrapDropDistance * effectEvent.Direction;
			this._trapCountValue++;
			if (this._trapCountValue >= this._trapCountMax)
			{
				this._isDropping = false;
			}
		}

		public override float GetDps()
		{
			return base.GetDpsFromModifierData(ModifierData.CreateConvoluted(ModifierData.CreateData(this.MyInfo.ProjectileModifiers, this.MyInfo), Mathf.Lerp(this._upgProjectileMinDamage, this._upgProjectileMaxDamage, this._chargeCount / (float)this._chargeMax)));
		}

		public override float GetRange()
		{
			return this._upgProjectileRange;
		}

		public override float GetRangeSqr()
		{
			return this._upgProjectileRange * this._upgProjectileRange;
		}

		protected Upgradeable _upgChargeTime;

		protected Upgradeable _upgProjectileMinDamage;

		protected Upgradeable _upgProjectileMaxDamage;

		protected Upgradeable _upgProjectileRange;

		protected ModifierData[] _upgTrapDamage;

		protected Upgradeable _upgTrapCount;

		private bool _isCharging;

		private long _chargeStartMillis;

		private float _chargeJokerRatio;

		private float _chargeCount;

		private int _chargeEffectId;

		private bool _isDropping;

		private Vector3 _dropPosition;

		private Vector3 _dropDirection;

		private TimedUpdater _dropUpdater;

		private long _dropStartMillis;

		private int _trapCountValue;

		private int _trapCountMax;
	}
}
