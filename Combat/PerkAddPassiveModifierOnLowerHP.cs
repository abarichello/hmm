using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkAddPassiveModifierOnLowerHP : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._modifiers = ((!this.UseExtraModifiers) ? this.Effect.Data.Modifiers : this.Effect.Data.ExtraModifiers);
			this.MinHpPercent = (int)this.Effect.Data.CustomVar;
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
		}

		private void FixedUpdate()
		{
			if (!this._combat || this._modifiers == null)
			{
				return;
			}
			if (this._combat.Combat.Data.HP < (float)(this.MinHpPercent * this._combat.Combat.Data.HPMax / 100))
			{
				if (this._applied)
				{
					return;
				}
				this._combat.AddPassiveModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId);
				this._applied = true;
			}
			else
			{
				if (!this._applied)
				{
					return;
				}
				this._combat.RemovePassiveModifiers(this._modifiers, this.Effect.Gadget.Combat, this.Effect.EventId);
				this._applied = false;
			}
		}

		public void OnDestroyEffect(DestroyEffect evt)
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

		public static readonly BitLogger Log = new BitLogger(typeof(PerkAddPassiveModifierOnLowerHP));

		public BasePerk.PerkTarget Target;

		public bool UseExtraModifiers;

		private CombatController _combat;

		private ModifierData[] _modifiers;

		private bool _applied;

		public int MinHpPercent;
	}
}
