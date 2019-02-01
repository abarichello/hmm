using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class LinkAoE : GadgetBehaviour
	{
		public LinkAoEInfo AoEInfo
		{
			get
			{
				return base.Info as LinkAoEInfo;
			}
		}

		protected float _currentLifeTime
		{
			get
			{
				return base.LifeTime - 0.001f * (float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._currentFirstDropTimeMillis);
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			LinkAoEInfo aoEInfo = this.AoEInfo;
			this._dropTick = aoEInfo.DropTick;
			this._trapRadius = aoEInfo.TrapRange;
			this._dropDistance = aoEInfo.TrapDropDistance;
			this._trapDamage = ModifierData.CreateData(aoEInfo.TrapDamage, aoEInfo);
			this._upgTrapCount = new Upgradeable(aoEInfo.TrapCountUpgrade, (float)aoEInfo.TrapCount, aoEInfo.UpgradesValues);
			this._upgTrapDamageMax = new Upgradeable(aoEInfo.TrapMaxDamageUpgrade, aoEInfo.TrapMaxDamage, aoEInfo.UpgradesValues);
			this._upgTrapDamageMin = new Upgradeable(aoEInfo.TrapMinDamageUpgrade, aoEInfo.TrapMinDamage, aoEInfo.UpgradesValues);
			this._upgTrapDamageChargeTime = new Upgradeable(aoEInfo.TrapChargeTimeUpgrade, aoEInfo.TrapChargeTime, aoEInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._trapDamage.SetLevel(upgradeName, level);
			this._upgTrapCount.SetLevel(upgradeName, level);
			this._upgTrapDamageMax.SetLevel(upgradeName, level);
			this._upgTrapDamageMin.SetLevel(upgradeName, level);
			this._upgTrapDamageChargeTime.SetLevel(upgradeName, level);
		}

		protected override void Awake()
		{
			base.Awake();
			if (GameHubBehaviour.Hub)
			{
				this.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		public override void Activate()
		{
			base.Activate();
			this._lastActivationTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.TryDropTrap();
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.Combat.GadgetStates.SetJokerBarState((float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._lastActivationTime) * 0.001f, (float)this._upgTrapDamageChargeTime.IntGet());
			base.GadgetUpdate();
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return;
			}
			if (!base.Pressed)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			this.StartDropping();
			this.TryDropTrap();
		}

		protected void StartDropping()
		{
			float num = this._upgTrapDamageChargeTime * 1000f;
			int num2 = (int)this._upgTrapDamageMax.Get();
			int num3 = (int)this._upgTrapDamageMin.Get();
			float num4 = (float)((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._lastActivationTime);
			this._lastActivationTime = this.CurrentCooldownTime;
			this._currentTrapDamage = (int)Mathf.Lerp((float)num3, (float)num2, num4 / num);
			this._currentTrapCountMax = (int)this._upgTrapCount.Get();
			this._currentTrapCountValue = 0;
			this._currentDropPosition = this.Combat.transform.position;
			this._currentDropDirection = base.CalcDirection(this._currentDropPosition, base.Target);
			this._currentDropUpdater = new TimedUpdater
			{
				PeriodMillis = (int)(this._dropTick * 1000f)
			};
			this._currentDropDirection.y = 0f;
			this._currentDropDirection.Normalize();
			this._currentFirstDropTimeMillis = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this._currentIsDropping = true;
			LinkAoEInfo aoEInfo = this.AoEInfo;
			EffectEvent effectEvent = base.GetEffectEvent(aoEInfo.DropperEffect);
			effectEvent.Direction = this._currentDropDirection;
			effectEvent.Range = (float)this._currentTrapCountMax * this._trapRadius;
			effectEvent.MoveSpeed = effectEvent.Range / (this._dropTick * (float)this._currentTrapCountMax);
			effectEvent.Origin = this._currentDropPosition;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			EffectEvent effectEvent2 = base.GetEffectEvent(aoEInfo.AudioEffect);
			effectEvent2.Origin = this._currentDropPosition + this._currentDropDirection * effectEvent.Range / 2f;
			effectEvent2.LifeTime = this._currentLifeTime;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent2);
		}

		private void TryDropTrap()
		{
			if (!this._currentIsDropping)
			{
				return;
			}
			if (this._currentDropUpdater.ShouldHalt())
			{
				return;
			}
			LinkAoEInfo aoEInfo = this.AoEInfo;
			EffectEvent effectEvent = base.GetEffectEvent(aoEInfo.TrapEffect);
			effectEvent.Modifiers = ModifierData.CreateConvoluted(this._trapDamage, (float)this._currentTrapDamage);
			effectEvent.Target = (effectEvent.Origin = this._currentDropPosition);
			effectEvent.Direction = this._currentDropDirection;
			effectEvent.Range = this._trapRadius;
			effectEvent.LifeTime = this._currentLifeTime;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this._currentDropPosition += aoEInfo.TrapDropDistance * this._currentDropDirection;
			this._currentTrapCountValue++;
			if (this._currentTrapCountValue >= this._currentTrapCountMax)
			{
				this._currentIsDropping = false;
			}
		}

		protected ModifierData[] _trapDamage;

		protected float _dropTick;

		protected float _dropDistance;

		protected float _trapRadius;

		protected Upgradeable _upgTrapCount;

		protected Upgradeable _upgTrapDamageMax;

		protected Upgradeable _upgTrapDamageMin;

		protected Upgradeable _upgTrapDamageChargeTime;

		private int _currentTrapCountValue;

		private int _currentTrapCountMax;

		private int _currentTrapDamage;

		private Vector3 _currentDropPosition;

		private Vector3 _currentDropDirection;

		private TimedUpdater _currentDropUpdater;

		private long _currentFirstDropTimeMillis;

		private bool _currentIsDropping;

		private long _lastActivationTime;
	}
}
