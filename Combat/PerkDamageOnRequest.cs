using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkDamageOnRequest : BasePerk
	{
		public override void PerkInitialized()
		{
			this._modifiers = ((!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers);
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

		public void CauseDamage()
		{
			if (this._combat && this._combat.Controller)
			{
				this._combat.Controller.AddModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId, false);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDamageOnRequest));

		public BasePerk.PerkTarget Target;

		public bool UseExtraModifiers;

		private CombatObject _combat;

		private ModifierData[] _modifiers;
	}
}
