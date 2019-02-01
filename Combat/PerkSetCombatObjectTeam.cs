using System;

namespace HeavyMetalMachines.Combat
{
	public class PerkSetCombatObjectTeam : BasePerk
	{
		public override void PerkInitialized()
		{
			CombatObject component = base.GetComponent<CombatObject>();
			CombatObject component2 = this.Effect.Owner.GetComponent<CombatObject>();
			component.CreepTeam = component2.Team;
		}
	}
}
