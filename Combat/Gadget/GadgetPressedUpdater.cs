using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetPressedUpdater : IGadgetUpdater
	{
		public GadgetPressedUpdater(HMMHub hub, GadgetBehaviour gadgetBehaviour, Action beforeBaseGadgetFixedUpdate, Action baseGadgetFixedUpdate, Func<int> warmupCallback, Func<int> fireCallback, Func<int> fireExtraCallback, Action<int> onGadgetUsed)
		{
			this._beforeBaseGadgetFixedUpdate = beforeBaseGadgetFixedUpdate;
			this._baseGadgetFixedUpdate = baseGadgetFixedUpdate;
			this._fireCallback = fireCallback;
			this._fireExtraCallback = fireExtraCallback;
			this._fireWarmupCallback = warmupCallback;
			this._onGadgetUsed = onGadgetUsed;
			this._gadget = gadgetBehaviour;
			this._hub = hub;
		}

		protected bool HasEffect
		{
			get
			{
				return this.CurrentEffectId != -1;
			}
		}

		protected bool WarmupFired
		{
			get
			{
				return this.WarmupEffectId != -1;
			}
		}

		private bool TestSecondClick
		{
			get
			{
				return this._gadget.Info.DestroyOnSecondClick || this._gadget.Info.FireExtraOnSecondClick;
			}
		}

		public virtual void RunGadgetUpdate()
		{
			if (!this._hub || this._hub.Net.IsClient() || !this._gadget.Activated)
			{
				return;
			}
			this._gadget.CurrentTime = (long)this._hub.GameTime.GetPlaybackTime();
			if (this._beforeBaseGadgetFixedUpdate != null)
			{
				this._beforeBaseGadgetFixedUpdate();
			}
			this._baseGadgetFixedUpdate();
			if (this.WarmupFired || this.HasEffect)
			{
				this._usageTime += Time.deltaTime;
			}
			if (this._gadget.Pressed && !this.PressedBlocked)
			{
				if (!this.HasEffect)
				{
					if (this._gadget.Info.WarmupSeconds != 0f && !this.WarmupFired)
					{
						this.WarmupEffectId = this._fireWarmupCallback();
					}
					else if (!this.WarmupFired)
					{
						this.StartGadget();
					}
				}
				else
				{
					if (this.TestSecondClick)
					{
						if (!this._gadget.PressedThisFrame && this.CurrentExtraEffectId == -1)
						{
							this._buttonHasBeenReleased = true;
						}
						if (this._buttonHasBeenReleased && this._gadget.PressedThisFrame)
						{
							this.ExecuteSecondClick();
							this._buttonHasBeenReleased = false;
						}
					}
					this._onGadgetUsed(this.CurrentEffectId);
					if (!this._gadget.Combat.Controller.ConsumeEP((float)this._gadget.ActivatedCost * Time.deltaTime))
					{
						this.StopGadget();
					}
				}
			}
			else if (!this._gadget.Info.DoNotDestroyOnRelease && (this.HasEffect || this.WarmupFired) && this._usageTime >= this._gadget.Info.MinimumUsageTime)
			{
				this.StopGadget();
			}
		}

		private void ExecuteSecondClick()
		{
			if (this._gadget.ExistingFiredEffectsCount() > 0)
			{
				if (this._gadget.Info.DestroyOnSecondClick)
				{
					this._gadget.DestroyExistingFiredEffects();
				}
				if (this._gadget.Info.FireExtraOnSecondClick)
				{
					this.CurrentExtraEffectId = this._fireExtraCallback();
				}
			}
		}

		protected virtual void StartGadget()
		{
			if (this._gadget.CurrentCooldownTime > this._gadget.CurrentTime)
			{
				return;
			}
			if (!this._gadget.Combat.Controller.ConsumeEP((float)this._gadget.ActivationCost))
			{
				return;
			}
			this.CurrentEffectId = this._fireCallback();
			this._gadget.Toggled = true;
			this._onGadgetUsed(this.CurrentEffectId);
			this._buttonHasBeenReleased = false;
		}

		protected virtual void StopGadget()
		{
			this._usageTime = 0f;
			this._gadget.CurrentCooldownTime = (long)(this._gadget.Cooldown * 1000f) + this._gadget.CurrentTime;
			this._gadget.Toggled = false;
			this._gadget.DestroyExistingFiredEffects();
			BaseFX baseFx = this._hub.Effects.GetBaseFx(this.WarmupEffectId);
			if (baseFx != null)
			{
				baseFx.TriggerDefaultDestroy(-1);
			}
			this.CurrentEffectId = -1;
			this.CurrentExtraEffectId = -1;
			this.WarmupEffectId = -1;
		}

		public void DestroyEffect(DestroyEffectMessage evt)
		{
			if (evt.RemoveData.TargetEventId == this._gadget.LastWarmupId && evt.RemoveData.DestroyReason == BaseFX.EDestroyReason.Lifetime)
			{
				this._gadget.Origin = evt.RemoveData.Origin;
				this.StartGadget();
			}
			else if (evt.RemoveData.TargetEventId == this._gadget.LastWarmupId && evt.RemoveData.DestroyReason != BaseFX.EDestroyReason.Lifetime)
			{
				this.StopGadget();
			}
			else if (evt.RemoveData.TargetEventId == this.CurrentExtraEffectId)
			{
				this.StopGadget();
			}
			if (evt.RemoveData.TargetEventId != this.CurrentEffectId || this.CurrentExtraEffectId != -1)
			{
				return;
			}
			this.StopGadget();
		}

		public virtual void ObjectUnspawned(UnspawnEvent evt)
		{
			this.StopGadget();
		}

		public void ObjectSpawned(SpawnEvent evt)
		{
		}

		protected readonly HMMHub _hub;

		protected readonly GadgetBehaviour _gadget;

		protected readonly Action _beforeBaseGadgetFixedUpdate;

		protected readonly Action _baseGadgetFixedUpdate;

		protected readonly Func<int> _fireCallback;

		protected readonly Func<int> _fireWarmupCallback;

		protected readonly Func<int> _fireExtraCallback;

		protected readonly Action<int> _onGadgetUsed;

		protected float _usageTime;

		public bool PressedBlocked;

		public int CurrentEffectId = -1;

		public int CurrentExtraEffectId = -1;

		public int WarmupEffectId = -1;

		private bool _buttonHasBeenReleased;
	}
}
