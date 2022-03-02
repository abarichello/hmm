using System;
using HeavyMetalMachines.Combat.Infra;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Combat.Business
{
	public class AddPassiveModifiersFromEnvironment : IChangePassiveModifiersFromEnvironment
	{
		public AddPassiveModifiersFromEnvironment(ModifierData[] modifiers, ModifierService modifierService, ICombatControllerStorage combatControllerStorage)
		{
			this._modifiers = modifiers;
			this._modifierService = modifierService;
			this._combatControllerStorage = combatControllerStorage;
		}

		public void ExecuteOnObjId(int objId)
		{
			ICombatController byObjId = this._combatControllerStorage.GetByObjId(objId);
			this._modifierService.AddPassiveModifiersFromEnvironment(byObjId, this._modifiers);
		}

		private ICombatControllerStorage _combatControllerStorage;

		private ModifierData[] _modifiers;

		private ModifierService _modifierService;
	}
}
