using System;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Combat.Infra
{
	public interface IModifierService
	{
		void AddPassiveModifiersFromEnvironment(ICombatController combat, ModifierData[] modifiers);

		void RemovePassiveModifiersFromEnvironment(ICombatController combat, ModifierData[] modifiers);
	}
}
