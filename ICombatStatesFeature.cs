using System;
using HeavyMetalMachines.Combat;

namespace HeavyMetalMachines
{
	public interface ICombatStatesFeature
	{
		ICombatDataSerialData GetCombatData(int id);

		ICombatAttributesSerialData GetCombatAttributes(int id);

		IGadgetDataSerialData GetGadgetData(int id);

		ISpawnControllerSerialData GetSpawnControllerData(int id);

		BombGridController.IGridGamePlayerSerialData GetGridGamePlayerData(int id);
	}
}
