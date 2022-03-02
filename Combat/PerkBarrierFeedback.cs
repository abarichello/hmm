using System;
using HeavyMetalMachines.VFX;

namespace HeavyMetalMachines.Combat
{
	public class PerkBarrierFeedback : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			this._target = base.GetTargetCombat(this.Effect, this.Target);
			if (!this._target)
			{
				return;
			}
			this._target.ListenToBarrierHit += this.BarrierHitListener;
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (!this._target)
			{
				return;
			}
			this._target.ListenToBarrierHit -= this.BarrierHitListener;
		}

		private void BarrierHitListener(CombatObject causer, ModifierInstance mod, int eventId)
		{
			this._target.Feedback.Add(this.Feedback, eventId, causer.Id.ObjId, mod.StartTime, (int)((float)mod.StartTime + this.Feedback.LifeTime * 1000f), 0, this.Effect.Gadget.Slot);
		}

		public ModifierFeedbackInfo Feedback;

		public BasePerk.PerkTarget Target;

		private CombatObject _target;
	}
}
