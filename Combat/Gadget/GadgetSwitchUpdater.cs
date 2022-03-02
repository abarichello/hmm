using System;
using HeavyMetalMachines.Event;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class GadgetSwitchUpdater : IGadgetUpdater
	{
		public GadgetSwitchUpdater(HMMHub hub, GadgetBehaviour gadgetBehaviour, Action beforeBaseGadgetFixedUpdate, Action baseGadgetFixedUpdate, Func<int> fireWarmupCallback, Func<int> fireCallback, Func<Action<EffectEvent>, int> fireExtraCallback, Action<int> onGadgetUsed)
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
			if (this._hub.Net.IsClient())
			{
				return;
			}
			if (!this._gadget.Activated || this._hub.Global.LockAllPlayers || this._dead)
			{
				this.TryDestroyCurrentEffect(this._currentEffectId);
				this._currentSwitchValue = -1;
				return;
			}
			this._gadget.CurrentTime = (long)this._hub.GameTime.GetPlaybackTime();
			if ((this._gadget.Pressed && this._keyEverReleased) || this._currentSwitchValue == -1)
			{
				this.TryToFireWarmupOrGadget();
			}
			this._keyEverReleased = !this._gadget.Pressed;
			if (this._beforeBaseGadgetFixedUpdate != null)
			{
				this._beforeBaseGadgetFixedUpdate();
			}
			this._baseGadgetFixedUpdate();
		}

		private void TryToFireWarmupOrGadget()
		{
			if (this._gadget.CurrentCooldownTime > this._gadget.CurrentTime)
			{
				return;
			}
			if (!this._gadget.Combat.Controller.ConsumeEP((float)this._gadget.ActivationCost))
			{
				return;
			}
			bool flag = this._currentSwitchValue > -1;
			if (flag && this._gadget.WarmupTime > 0f)
			{
				this.FireWarmup();
			}
			else
			{
				this.FireGadget();
			}
			if (flag)
			{
				this._gadget.CurrentCooldownTime = this._gadget.CurrentTime + (long)((this._gadget.Cooldown + this._gadget.WarmupTime) * 1000f);
			}
		}

		private void FireWarmup()
		{
			this._fireWarmupCallback();
		}

		private void FireGadget()
		{
			if (this._currentSwitchValue == 0)
			{
				this._currentSwitchValue = 1;
				this._currentEffectId = this._fireExtraCallback(new Action<EffectEvent>(this.SetExtraEffectTransform));
			}
			else
			{
				this._currentSwitchValue = 0;
				this._currentEffectId = this._fireCallback();
			}
			this._gadget.CurrentGadgetValue = this._currentSwitchValue;
			this._onGadgetUsed(this._currentEffectId);
		}

		private void SetExtraEffectTransform(EffectEvent data)
		{
			if (!this._gadget.Info.UseEffectDeathPosition)
			{
				return;
			}
			data.Origin = this._origin;
			data.Direction = this._direction;
		}

		private void TryDestroyCurrentEffect(int effectId)
		{
			if (this._currentEffectId == -1 || effectId != this._currentEffectId)
			{
				return;
			}
			this._currentEffectId = -1;
			this._gadget.DestroyExistingFiredEffects();
		}

		public void DestroyEffect(DestroyEffectMessage evt)
		{
			bool flag = evt.RemoveData.TargetEventId == this._gadget.LastWarmupId && evt.RemoveData.DestroyReason == BaseFX.EDestroyReason.Lifetime;
			bool flag2 = evt.RemoveData.TargetEventId == this._currentEffectId;
			if (!flag && !flag2)
			{
				return;
			}
			if (this._gadget.Info.UseEffectDeathPosition && flag)
			{
				BaseFX baseFx = this._hub.Events.Effects.GetBaseFx(this._currentEffectId);
				CombatObject combat = CombatRef.GetCombat(baseFx);
				this._origin = combat.transform.position;
				this._direction = combat.transform.forward;
			}
			this.TryDestroyCurrentEffect(this._currentEffectId);
			this.FireGadget();
		}

		public void ObjectUnspawned(UnspawnEvent evt)
		{
			this._dead = true;
			this.TryDestroyCurrentEffect(this._currentEffectId);
		}

		public void ObjectSpawned(SpawnEvent evt)
		{
			this._dead = false;
		}

		protected readonly HMMHub _hub;

		protected readonly GadgetBehaviour _gadget;

		protected readonly Action _beforeBaseGadgetFixedUpdate;

		protected readonly Action _baseGadgetFixedUpdate;

		protected readonly Func<int> _fireWarmupCallback;

		protected readonly Func<int> _fireCallback;

		protected readonly Func<Action<EffectEvent>, int> _fireExtraCallback;

		protected readonly Action<int> _onGadgetUsed;

		private bool _keyEverReleased;

		private int _currentSwitchValue = -1;

		private int _currentEffectId = -1;

		private bool _dead;

		private Vector3 _direction;

		private Vector3 _origin;
	}
}
