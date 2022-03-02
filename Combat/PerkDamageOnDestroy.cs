using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnDestroy : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._modifiers = base.GetModifiers(this.Source);
			BasePerk.PerkTarget target = this.Target;
			if (target != BasePerk.PerkTarget.Owner)
			{
				if (target == BasePerk.PerkTarget.Target)
				{
					this._combat = CombatRef.GetCombat(this.Effect.Target);
				}
			}
			else
			{
				this._combat = this.Effect.Gadget.Combat;
			}
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			bool flag = this.Effect.CheckHit(this._combat);
			if (flag && this._combat.isActiveAndEnabled && this._combat && this._combat.Controller)
			{
				if (this.SetModDirection)
				{
					this._modifiers.SetDirection(this._combat.transform.right);
				}
				this._combat.Controller.AddModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId, evt.RemoveData.SrvWasBarrier);
			}
		}

		public BasePerk.PerkTarget Target;

		public bool SetModDirection;

		public BasePerk.DamageSource Source = BasePerk.DamageSource.EventModifiers;

		private CombatObject _combat;

		private ModifierData[] _modifiers;
	}
}
