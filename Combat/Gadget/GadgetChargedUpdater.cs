using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetChargedUpdater : IGadgetUpdater
	{
		public GadgetChargedUpdater(HMMHub hub, GadgetBehaviour gadgetBehaviour, Action beforeBaseGadgetFixedUpdate, Action baseGadgetFixedUpdate, Func<int> fireWarmupCallback, Func<int> fireCallback, Func<int> fireExtraCallback, Action<int> onGadgetUsed)
		{
			this._beforeBaseGadgetFixedUpdate = beforeBaseGadgetFixedUpdate;
			this._baseGadgetFixedUpdate = baseGadgetFixedUpdate;
			this._fireCallback = fireCallback;
			this._fireWarmupCallback = fireWarmupCallback;
			this._fireExtraCallback = fireExtraCallback;
			this._onGadgetUsed = onGadgetUsed;
			this._gadget = gadgetBehaviour;
			this._hub = hub;
			this._heatRate = 1f / this._gadget.WarmupTime;
		}

		protected bool HasWarmupEffect
		{
			get
			{
				return this._currentWarmupEffectId != -1;
			}
		}

		public virtual void RunGadgetUpdate()
		{
			if (!this._hub || this._hub.Net.IsClient() || !this._gadget.Activated)
			{
				if (this.HasWarmupEffect)
				{
					if (this._beforeBaseGadgetFixedUpdate != null)
					{
						this._beforeBaseGadgetFixedUpdate();
					}
					this.StopCharge(BaseFX.EDestroyReason.Gadget);
					this._baseGadgetFixedUpdate();
				}
				return;
			}
			this._gadget.CurrentTime = (long)this._hub.GameTime.GetPlaybackTime();
			if (this._gadget.CurrentCooldownTime > this._gadget.CurrentTime)
			{
				if (this._beforeBaseGadgetFixedUpdate != null)
				{
					this._beforeBaseGadgetFixedUpdate();
				}
				this._baseGadgetFixedUpdate();
				return;
			}
			if (this._beforeBaseGadgetFixedUpdate != null)
			{
				this._beforeBaseGadgetFixedUpdate();
			}
			bool pressed = this._gadget.Pressed;
			if (pressed && !this.HasWarmupEffect)
			{
				this._gadget.CurrentHeat = 0f;
				this._currentWarmupEffectId = this._fireWarmupCallback();
				this._timeSet = false;
			}
			if (this.HasWarmupEffect)
			{
				if (!pressed)
				{
					this.StopCharge(BaseFX.EDestroyReason.Default);
					this._gadget.CurrentCooldownTime = (long)(this._gadget.Cooldown * 1000f) + this._gadget.CurrentTime;
					if (this._gadget.CurrentHeat < 1f)
					{
						this.CurrentEffectId = this._fireCallback();
						if (this._onGadgetUsed != null)
						{
							this._onGadgetUsed(this.CurrentEffectId);
						}
					}
					else if (this._onGadgetUsed != null)
					{
						this.CurrentEffectId = this._fireExtraCallback();
						this._onGadgetUsed(this.CurrentEffectId);
					}
				}
				else if (this._gadget.CurrentHeat >= 1f)
				{
					if (!this._timeSet)
					{
						this._nextTimeWillexplode = this._hub.GameTime.GetPlaybackTime() + this._gadget.Info.ChargedTimeMillis;
						this._timeSet = true;
					}
					if (this._hub.GameTime.GetPlaybackTime() > this._nextTimeWillexplode)
					{
						this.StopCharge(BaseFX.EDestroyReason.Lifetime);
						this._gadget.CurrentCooldownTime = (long)(this._gadget.Cooldown * 1000f) + this._gadget.CurrentTime;
						this._gadget.CurrentHeat = 0f;
					}
				}
				else
				{
					this._gadget.CurrentHeat = Mathf.Clamp01(this._gadget.CurrentHeat + this._heatRate * Time.deltaTime);
				}
			}
			this._baseGadgetFixedUpdate();
		}

		public void DestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId == this.CurrentEffectId)
			{
				this.CurrentEffectId = -1;
				this._gadget.CurrentHeat = 0f;
			}
		}

		public void ObjectUnspawned(UnspawnEvent evt)
		{
			this.ResetState();
		}

		public void ObjectSpawned(SpawnEvent evt)
		{
			this.ResetState();
		}

		private void ResetState()
		{
			if (!this._hub || this._hub.Net.IsClient())
			{
				return;
			}
			if (this._beforeBaseGadgetFixedUpdate != null)
			{
				this._beforeBaseGadgetFixedUpdate();
			}
			this.StopCharge(BaseFX.EDestroyReason.Gadget);
			this._baseGadgetFixedUpdate();
			this._gadget.CurrentHeat = 0f;
		}

		private void StopCharge(BaseFX.EDestroyReason reason)
		{
			BaseFX baseFx = this._hub.Events.Effects.GetBaseFx(this._currentWarmupEffectId);
			if (baseFx)
			{
				baseFx.TriggerDestroy(-1, baseFx.transform.position, false, null, Vector3.zero, reason, false);
			}
			this._currentWarmupEffectId = -1;
		}

		protected readonly HMMHub _hub;

		protected readonly GadgetBehaviour _gadget;

		protected readonly Action _beforeBaseGadgetFixedUpdate;

		protected readonly Action _baseGadgetFixedUpdate;

		private readonly Func<int> _fireWarmupCallback;

		protected readonly Func<int> _fireExtraCallback;

		protected readonly Func<int> _fireCallback;

		protected readonly Action<int> _onGadgetUsed;

		private int _currentWarmupEffectId = -1;

		public int CurrentEffectId = -1;

		private float _heatRate;

		private bool _timeSet;

		private int _nextTimeWillexplode;
	}
}
