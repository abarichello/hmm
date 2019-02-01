using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetBaseUpdater : IGadgetUpdater
	{
		public GadgetBaseUpdater(HMMHub hub, GadgetBehaviour gadgetBehaviour, Action beforeBaseGadgetFixedUpdate, Action baseGadgetFixedUpdate, Func<int> fireWarmupCallback, Func<int> fireCallback, Func<int> fireExtraCallback, Action<int> onGadgetUsed)
		{
			this._beforeBaseGadgetFixedUpdate = beforeBaseGadgetFixedUpdate;
			this._baseGadgetFixedUpdate = baseGadgetFixedUpdate;
			this._fireWarmupCallback = fireWarmupCallback;
			this._fireCallback = fireCallback;
			this._fireExtraCallback = fireExtraCallback;
			this._onGadgetUsed = onGadgetUsed;
			this._gadget = gadgetBehaviour;
			this._hub = hub;
		}

		public void RunGadgetUpdate()
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
			if (this._gadget.TestSecondClick())
			{
				if (!this._gadget.Pressed)
				{
					this._buttonHasBeenReleased = true;
				}
				if (this._buttonHasBeenReleased && this._gadget.Pressed)
				{
					this._gadget.ExecuteSecondClick();
					this._buttonHasBeenReleased = false;
				}
			}
			if (this._gadget.CurrentCooldownTime > this._gadget.CurrentTime)
			{
				return;
			}
			if (!this._gadget.Pressed)
			{
				this._gadget.CurrentCooldownTime = this._gadget.CurrentTime;
				return;
			}
			if (!this._gadget.Combat.Controller.ConsumeEP((float)this._gadget.ActivationCost))
			{
				return;
			}
			long num = this._gadget.CurrentTime - this._gadget.CurrentCooldownTime;
			this._gadget.CurrentCooldownTime = (long)(this._gadget.Cooldown * 1000f) + (long)(this._gadget.Info.WarmupSeconds * 1000f) + this._gadget.CurrentTime - num;
			if (this._gadget.Info.WarmupSeconds != 0f)
			{
				if (this.FireWarmup() == -1)
				{
					this._gadget.CurrentCooldownTime -= (long)(this._gadget.Info.WarmupSeconds * 1000f);
				}
			}
			else
			{
				this.FireGadget();
			}
			this._buttonHasBeenReleased = false;
		}

		private int FireWarmup()
		{
			return this._fireWarmupCallback();
		}

		private void FireGadget()
		{
			int num = this._fireCallback();
			if (num != -1 && this._onGadgetUsed != null)
			{
				this._onGadgetUsed(num);
			}
		}

		public void DestroyEffect(DestroyEffect evt)
		{
			if (evt.RemoveData.TargetEventId == this._gadget.LastWarmupId && evt.RemoveData.DestroyReason == BaseFX.EDestroyReason.Lifetime)
			{
				this._gadget.Origin = evt.RemoveData.Origin;
				this.FireGadget();
			}
		}

		public void ObjectUnspawned(UnspawnEvent evt)
		{
		}

		public void ObjectSpawned(SpawnEvent evt)
		{
		}

		private readonly HMMHub _hub;

		private readonly GadgetBehaviour _gadget;

		private readonly Action _beforeBaseGadgetFixedUpdate;

		private readonly Action _baseGadgetFixedUpdate;

		private readonly Func<int> _fireWarmupCallback;

		private readonly Func<int> _fireCallback;

		private readonly Func<int> _fireExtraCallback;

		private readonly Action<int> _onGadgetUsed;

		private bool _buttonHasBeenReleased;
	}
}
