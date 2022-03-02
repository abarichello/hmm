using System;
using HeavyMetalMachines.Combat;

namespace HeavyMetalMachines.Arena.Infra
{
	public interface IArenaModifierStorage
	{
		ModifierData[] GetByArenaCondition(ArenaModifierCondition condition);

		void Set(ArenaModifierCondition condition, ModifierData[] datas);
	}
}
