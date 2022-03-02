using System;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Combat.Infra
{
	public class ModifierService : IModifierService
	{
		public void AddPassiveModifiersFromEnvironment(ICombatController combat, ModifierData[] modifiers)
		{
			combat.AddPassiveModifiers(modifiers, null, -1);
		}

		public void RemovePassiveModifiersFromEnvironment(ICombatController combat, ModifierData[] modifiers)
		{
			combat.RemovePassiveModifiers(modifiers, null, -1);
		}
	}
}
