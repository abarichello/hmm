using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.VFX;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BuffCannon : GadgetBehaviour
	{
		public BuffCannonInfo CannonInfo
		{
			get
			{
				return base.Info as BuffCannonInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			BuffCannonInfo cannonInfo = this.CannonInfo;
			this._damage = ModifierData.CreateData(cannonInfo.Damage, cannonInfo);
			this._extraDamage = ModifierData.CreateData(cannonInfo.ExtraDamage, cannonInfo);
			this._yellowDamage = ModifierData.CreateData(cannonInfo.YellowDamage, cannonInfo);
			this._collisionDamage = ModifierData.CreateData(cannonInfo.CollisionDamage, cannonInfo);
			this._greenDamage = ModifierData.CreateData(cannonInfo.GreenDamage, cannonInfo);
			base.Pressed = false;
			this.CurrentCooldownTime = 0L;
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._damage.SetLevel(upgradeName, level);
			this._extraDamage.SetLevel(upgradeName, level);
			this._yellowDamage.SetLevel(upgradeName, level);
			this._collisionDamage.SetLevel(upgradeName, level);
			this._greenDamage.SetLevel(upgradeName, level);
		}

		protected override void GadgetUpdate()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this.CurrentTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			base.GadgetUpdate();
			this.CheckFireGadget();
			if (this._gadgetState == null)
			{
				return;
			}
			this.Combat.GadgetStates.SetGadgetState(this._gadgetState, base.Slot, (this._charges.Count <= 0) ? GadgetState.NotActive : GadgetState.Ready, 0L, this._charges.Count, 0f, 0, null);
		}

		public void RemoveCharge(int buffId)
		{
			this._charges.RemoveAll((int x) => x == buffId);
			if (this._charges.Count == 0 && this._ammoFeedback != 0)
			{
				this.Combat.Feedback.Remove(this._ammoFeedback);
				this._ammoFeedback = 0;
			}
		}

		public void AddCharge(int buffId)
		{
			this._charges.Add(buffId);
			this._charges.Sort();
			if (this._charges.Count == 1)
			{
				BuffCannonInfo cannonInfo = this.CannonInfo;
				if (cannonInfo.AmmoFeedback != null)
				{
					this._ammoFeedback = this.Combat.Feedback.Add(cannonInfo.AmmoFeedback, -1, this.Combat.Id.ObjId, GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), -1, 0, base.Slot);
				}
			}
		}

		protected virtual int CheckFireGadget()
		{
			if (this.CurrentCooldownTime > this.CurrentTime)
			{
				return -1;
			}
			if (this._charges.Count == 0)
			{
				return -1;
			}
			if (this.CurrentCooldownTime == 0L || !base.Pressed)
			{
				this.CurrentCooldownTime = this.CurrentTime;
				return -1;
			}
			if (!this.Combat.Controller.ConsumeEP((float)base.ActivationCost))
			{
				return -1;
			}
			long num = this.CurrentTime - this.CurrentCooldownTime;
			this.CurrentCooldownTime = (long)(this.Cooldown * 1000f) + this.CurrentTime - num;
			int num2 = this.FireGadget();
			base.OnGadgetUsed(num2);
			return num2;
		}

		protected override int FireGadget()
		{
			return this.FireCannon();
		}

		private int FireCannon()
		{
			BuffCannonInfo cannonInfo = this.CannonInfo;
			EffectEvent effectEvent = base.GetEffectEvent(cannonInfo.Effect);
			effectEvent.MoveSpeed = cannonInfo.MoveSpeed;
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = this.DummyPosition();
			effectEvent.Target = base.Target;
			effectEvent.LifeTime = ((base.LifeTime <= 0f) ? (effectEvent.Range / effectEvent.MoveSpeed) : base.LifeTime);
			effectEvent.Direction = base.CalcDirection(effectEvent.Origin, effectEvent.Target);
			effectEvent.Modifiers = this._damage;
			effectEvent.ExtraModifiers = this._extraDamage;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		private void StartEffect(FXInfo effect, ModifierData[] damage, ModifierFeedbackInfo feedback, ref int evtId, ref int feedbackId)
		{
			if (effect != null && !string.IsNullOrEmpty(effect.Effect))
			{
				EffectEvent effectEvent = base.GetEffectEvent(effect);
				effectEvent.Origin = this.DummyPosition();
				effectEvent.Modifiers = damage;
				evtId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			}
			if (feedback != null)
			{
				feedbackId = this.Combat.Feedback.Add(feedback, -1, this.Combat.Id.ObjId, GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), -1, 0, base.Slot);
			}
		}

		private void StopEffect(ref int evtId, ref int feedbackId)
		{
			if (evtId > 0)
			{
				EffectRemoveEvent content = new EffectRemoveEvent
				{
					TargetEventId = evtId,
					Origin = this.DummyPosition(),
					SourceId = this.Combat.Id.ObjId
				};
				GameHubBehaviour.Hub.Events.TriggerEvent(content);
				evtId = 0;
			}
			if (feedbackId > 0)
			{
				this.Combat.Feedback.Remove(feedbackId);
				feedbackId = 0;
			}
		}

		public override void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (evt.RemoveData.TargetEventId != this._greenEffect)
			{
				return;
			}
			this._greenEffect = 0;
			if (this._greenBuffIds.Count == 0)
			{
				return;
			}
			BuffCannonInfo cannonInfo = this.CannonInfo;
			this.StartEffect(cannonInfo.GreenEffect, this._greenDamage, null, ref this._greenEffect, ref this._greenFeedback);
		}

		private ModifierData[] _extraDamage;

		private ModifierData[] _damage;

		private List<int> _charges = new List<int>(2);

		private int _ammoFeedback;

		private ModifierData[] _yellowDamage;

		private int _yellows;

		private int _yellowEffect;

		private int _yellowFeedback;

		private ModifierData[] _collisionDamage;

		private int _collisions;

		private int _collisionEffect;

		private int _collisionFeedback;

		private ModifierData[] _greenDamage;

		private int _greenEffect;

		private int _greenFeedback;

		private List<int> _greenBuffIds = new List<int>();
	}
}
