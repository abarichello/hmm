using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkInstantDoubleDamage : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			CombatObject targetCombat = base.GetTargetCombat(this.Effect, this.ModifiersTarget);
			CombatObject targetCombat2 = base.GetTargetCombat(this.Effect, this.ExtraModifiersTarget);
			bool flag = true;
			bool flag2 = true;
			if (this.Mode == PerkInstantDoubleDamage.ModeEnum.ExtraModifiersOnlyIfModifiersAppliy && !this.CheckModifiersApply(this.Effect.Data.Modifiers, targetCombat))
			{
				flag2 = false;
			}
			if (flag)
			{
				targetCombat.Controller.AddModifiers(this.Effect.Data.Modifiers, this.Effect.Gadget.Combat, this.Effect.EventId, false);
			}
			if (flag2)
			{
				targetCombat2.Controller.AddModifiers(this.Effect.Data.ExtraModifiers, this.Effect.Gadget.Combat, this.Effect.EventId, false);
			}
		}

		private bool CheckModifiersApply(ModifierData[] mods, CombatObject combatObject)
		{
			for (int i = 0; i < mods.Length; i++)
			{
				if (!combatObject.Controller.CheckShouldApplyMod(mods[i], this.Effect.Gadget.Combat, this.Effect.EventId, false))
				{
					return false;
				}
			}
			return true;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkInstantDoubleDamage));

		public BasePerk.PerkTarget ModifiersTarget = BasePerk.PerkTarget.Target;

		public BasePerk.PerkTarget ExtraModifiersTarget = BasePerk.PerkTarget.Target;

		public PerkInstantDoubleDamage.ModeEnum Mode = PerkInstantDoubleDamage.ModeEnum.Normal;

		public enum ModeEnum
		{
			Normal = 1,
			ExtraModifiersOnlyIfModifiersAppliy
		}
	}
}
