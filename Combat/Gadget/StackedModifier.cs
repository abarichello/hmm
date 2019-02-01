using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.VFX;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class StackedModifier : GameHubBehaviour
	{
		public StackedModifier(Func<FXInfo, EffectEvent> getEffectEvent, FXInfo effectInfo, FXInfo explosionEffectInfo, CombatObject combatObject, ModifierFeedbackInfo[] modifierFeedbacks, int stackIntervalMillis)
		{
			this._combatObject = combatObject;
			this._modifierFeedbacks = modifierFeedbacks;
			this._getEffectEvent = getEffectEvent;
			this._effectInfo = effectInfo;
			this._explosionEffectInfo = explosionEffectInfo;
			this._stackIntervalMillis = stackIntervalMillis;
		}

		public void RunGadgetFixedUpdate()
		{
			if (this._stackTimedUpdater.ShouldHalt() || this._stackedTargets.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < this._stackedTargets.Count; i++)
			{
				if (this._stackedTargets[i].EndTimeMillis < (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime())
				{
					this.DestroyStack(this._stackedTargets[i--]);
				}
			}
		}

		public void SetLevel(ModifierData[] modifiers, ModifierData[] explosionModifiers, ModifierData[] extraModifiers, float[] explosionMultipliers, float range, float lifeTime, int maxStack)
		{
			this._modifiers = modifiers;
			this._extraModifiers = extraModifiers;
			this._explosionModifiers = explosionModifiers;
			this._range = range;
			this._lifeTime = lifeTime;
			this._maxStack = maxStack;
			this._explosionMultipliers = explosionMultipliers;
		}

		public void OnTriggerEnterCallback(TriggerEnterCallback evt, GadgetBehaviour gadgetOwner)
		{
			if (evt.Gadget != gadgetOwner)
			{
				return;
			}
			if (evt.Combat == null || !evt.Combat.IsAlive())
			{
				return;
			}
			StackedModifier.StackedTarget stackedTarget = this.GetStackedTarget(evt.Combat);
			if (stackedTarget != null)
			{
				this.AddChargeToStack(stackedTarget);
			}
			else
			{
				this.CreateStack(evt.Combat);
			}
		}

		private void CreateStack(CombatObject taker)
		{
			long num = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			long num2 = num + (long)(this._lifeTime * 1000f);
			int feedbackId = (this._modifierFeedbacks == null) ? -1 : taker.Feedback.Add(this._modifierFeedbacks[0], -1, this._combatObject.Id.ObjId, (int)num, (int)num2, 1, GadgetSlot.None);
			StackedModifier.StackedTarget stackedTarget = new StackedModifier.StackedTarget(num2, taker, feedbackId, this._stackIntervalMillis);
			stackedTarget.CombatObject.ListenToObjectUnspawn += this.OnTargetDeath;
			this._stackedTargets.Add(stackedTarget);
			this.StartEffect(stackedTarget);
		}

		private void AddChargeToStack(StackedModifier.StackedTarget target)
		{
			long num = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			target.EndTimeMillis = num + (long)(this._lifeTime * 1000f);
			if (target.NextStackUpdater.ShouldHalt())
			{
				return;
			}
			if (target.Charges < this._maxStack)
			{
				target.Charges++;
				StackedModifier.DestroyEffect(target);
				this.StartEffect(target);
			}
			if (target.FeedbackId != -1)
			{
				target.CombatObject.Feedback.Remove(target.FeedbackId);
				target.FeedbackId = target.CombatObject.Feedback.Add(this._modifierFeedbacks[target.Charges - 1], -1, this._combatObject.Id.ObjId, (int)num, (int)target.EndTimeMillis, target.Charges, GadgetSlot.None);
			}
		}

		private void StartEffect(StackedModifier.StackedTarget target)
		{
			EffectEvent effectEvent = this._getEffectEvent(this._effectInfo);
			effectEvent.Origin = target.CombatObject.Transform.position;
			effectEvent.Target = target.CombatObject.Transform.position;
			effectEvent.TargetId = target.CombatObject.Id.ObjId;
			effectEvent.Modifiers = ModifierData.CreateConvoluted(this._modifiers, this._explosionMultipliers[target.Charges - 1]);
			int effectId = GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			target.EffectId = effectId;
		}

		private static void DestroyEffect(StackedModifier.StackedTarget target)
		{
			if (target.EffectId == -1)
			{
				return;
			}
			BaseFX baseFx = GameHubBehaviour.Hub.Events.Effects.GetBaseFx(target.EffectId);
			if (baseFx)
			{
				baseFx.TriggerDefaultDestroy(-1);
			}
			target.EffectId = -1;
		}

		private void DestroyStack(StackedModifier.StackedTarget target)
		{
			if (!string.IsNullOrEmpty(this._explosionEffectInfo.Effect))
			{
				EffectEvent effectEvent = this._getEffectEvent(this._explosionEffectInfo);
				effectEvent.Origin = target.CombatObject.Transform.position;
				effectEvent.TargetId = target.CombatObject.Id.ObjId;
				effectEvent.Range = this._range;
				effectEvent.Modifiers = ModifierData.CreateConvoluted(this._explosionModifiers, this._explosionMultipliers[target.Charges - 1]);
				if (target.Charges == this._maxStack)
				{
					effectEvent.ExtraModifiers = this._extraModifiers;
					effectEvent.CustomVar = 1;
				}
				else
				{
					effectEvent.CustomVar = 0;
				}
				GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			}
			StackedModifier.DestroyEffect(target);
			if (target.FeedbackId != -1)
			{
				target.CombatObject.Feedback.Remove(target.FeedbackId);
			}
			target.CombatObject.ListenToObjectUnspawn -= this.OnTargetDeath;
			this._stackedTargets.Remove(target);
		}

		private void OnTargetDeath(CombatObject combatObject, UnspawnEvent msg)
		{
			StackedModifier.StackedTarget stackedTarget = this.GetStackedTarget(combatObject);
			if (stackedTarget == null)
			{
				return;
			}
			this.DestroyStack(stackedTarget);
		}

		private StackedModifier.StackedTarget GetStackedTarget(CombatObject combatObject)
		{
			for (int i = 0; i < this._stackedTargets.Count; i++)
			{
				if (this._stackedTargets[i].CombatObject.Id.ObjId == combatObject.Id.ObjId)
				{
					return this._stackedTargets[i];
				}
			}
			return null;
		}

		private TimedUpdater _stackTimedUpdater = new TimedUpdater
		{
			PeriodMillis = 100
		};

		private readonly List<StackedModifier.StackedTarget> _stackedTargets = new List<StackedModifier.StackedTarget>();

		private int _maxStack;

		private float _range;

		private float _lifeTime;

		private float[] _explosionMultipliers;

		private readonly int _stackIntervalMillis;

		private ModifierData[] _modifiers;

		private ModifierData[] _extraModifiers;

		private ModifierData[] _explosionModifiers;

		private readonly FXInfo _effectInfo;

		private readonly FXInfo _explosionEffectInfo;

		private readonly CombatObject _combatObject;

		private readonly ModifierFeedbackInfo[] _modifierFeedbacks;

		private readonly Func<FXInfo, EffectEvent> _getEffectEvent;

		[Serializable]
		private class StackedTarget
		{
			public StackedTarget(long endTime, CombatObject combatObject, int feedbackId, int stackIntervalMillis)
			{
				this.EndTimeMillis = endTime;
				this.CombatObject = combatObject;
				this.FeedbackId = feedbackId;
				this.Charges = 1;
				this.NextStackUpdater = new TimedUpdater(stackIntervalMillis, true, false);
			}

			public long EndTimeMillis;

			public int Charges;

			public int FeedbackId;

			public int EffectId;

			public TimedUpdater NextStackUpdater;

			public readonly CombatObject CombatObject;
		}
	}
}
