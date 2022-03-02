using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetToggledUpdater : IGadgetUpdater
	{
		public GadgetToggledUpdater(HMMHub hub, GadgetBehaviour gadgetBehaviour, Action beforeBaseGadgetFixedUpdate, Action baseGadgetFixedUpdate, Func<int> fireCallback, Action<int> onGadgetUsed)
		{
			this._beforeBaseGadgetFixedUpdate = beforeBaseGadgetFixedUpdate;
			this._baseGadgetFixedUpdate = baseGadgetFixedUpdate;
			this._fireCallback = fireCallback;
			this._onGadgetUsed = onGadgetUsed;
			this._gadget = gadgetBehaviour;
			this._hub = hub;
		}

		public virtual void RunGadgetUpdate()
		{
			if (!this._hub || this._hub.Net.IsClient() || !this._gadget.Activated)
			{
				return;
			}
			this._gadget.CurrentTime = (long)this._hub.GameTime.GetPlaybackTime();
			if (this._gadget.Pressed && !this._gadget.Toggled && this.released)
			{
				this.StartGadget();
			}
			else if (this._gadget.Pressed && this._gadget.Toggled && this.released)
			{
				this.StopGadget();
			}
			this.released = !this._gadget.Pressed;
			if (this._beforeBaseGadgetFixedUpdate != null)
			{
				this._beforeBaseGadgetFixedUpdate();
			}
			this._baseGadgetFixedUpdate();
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
			this.released = false;
			this._gadget.Toggled = true;
			this.CurrentEffectId = this._fireCallback();
			this._onGadgetUsed(this.CurrentEffectId);
			if (this.CurrentEffectId == -1)
			{
				this.StopGadget();
			}
		}

		protected virtual void StopGadget()
		{
			this._gadget.CurrentCooldownTime = (long)(this._gadget.Cooldown * 1000f) + this._gadget.CurrentTime;
			this._gadget.Toggled = false;
			this._gadget.DestroyExistingFiredEffects();
			this.CurrentEffectId = -1;
		}

		public void DestroyEffect(DestroyEffectMessage evt)
		{
			if (evt.RemoveData.TargetEventId != this.CurrentEffectId)
			{
				return;
			}
			this.StopGadget();
		}

		public virtual void ObjectUnspawned(UnspawnEvent evt)
		{
			if (!this._gadget.Toggled)
			{
				return;
			}
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

		protected readonly Action<int> _onGadgetUsed;

		public int CurrentEffectId = -1;

		protected bool released = true;
	}
}
