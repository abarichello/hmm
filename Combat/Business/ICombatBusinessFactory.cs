using System;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Combat.Business
{
	public interface ICombatBusinessFactory
	{
		IChangePassiveModifiersFromEnvironment CreateAddPassiveModifiersFromEnvironment(ICombatController combatController, ModifierData[] modifiers);

		IChangePassiveModifiersFromEnvironment CreateRemovePassiveModifiersFromEnvironment(ICombatController combatController, ModifierData[] modifiers);
	}
}
