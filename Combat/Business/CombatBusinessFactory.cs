using System;
using HeavyMetalMachines.Combat.Infra;
using HeavyMetalMachines.Infra.Context;
using Zenject;

namespace HeavyMetalMachines.Combat.Business
{
	public class CombatBusinessFactory : ICombatBusinessFactory
	{
		public IChangePassiveModifiersFromEnvironment CreateAddPassiveModifiersFromEnvironment(ICombatController combatController, ModifierData[] modifiers)
		{
			return new AddPassiveModifiersFromEnvironment(modifiers, this._modifierService, this._combatControllerStorage);
		}

		public IChangePassiveModifiersFromEnvironment CreateRemovePassiveModifiersFromEnvironment(ICombatController combatController, ModifierData[] modifiers)
		{
			return new RemovePassiveModifiersFromEnvironment(modifiers, this._modifierService, this._combatControllerStorage);
		}

		[Inject]
		private ModifierService _modifierService;

		[Inject]
		private ICombatControllerStorage _combatControllerStorage;
	}
}
