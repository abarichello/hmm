using System;

namespace HeavyMetalMachines.Combat
{
	public class PerkChangeLayer : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			this._target = base.GetTargetCombat(this.Effect, this.Target);
			if (!this._target)
			{
				return;
			}
			this._target.Layer.ChangeLayer(this.TargetLayer, this);
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			if (!this._target)
			{
				return;
			}
			this._target.Layer.RevertLayer(this);
		}

		public BasePerk.PerkTarget Target;

		public LayerManager.Layer TargetLayer;

		private CombatObject _target;
	}
}
