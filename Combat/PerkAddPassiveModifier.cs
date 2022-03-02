using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkAddPassiveModifier : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._modifiers = ((!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers);
			BasePerk.PerkTarget target = this.Target;
			if (target != BasePerk.PerkTarget.Owner)
			{
				if (target == BasePerk.PerkTarget.Target)
				{
					CombatObject combat = CombatRef.GetCombat(this.Effect.Target);
					this._combat = ((!combat) ? null : combat.Controller);
				}
			}
			else
			{
				this._combat = this.Effect.Gadget.Combat.Controller;
			}
			if (this._combat)
			{
				this._combat.AddPassiveModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId);
			}
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this._combat)
			{
				this._combat.RemovePassiveModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkAddPassiveModifier));

		public BasePerk.PerkTarget Target;

		public bool UseExtraModifiers;

		private CombatController _combat;

		private ModifierData[] _modifiers;
	}
}
