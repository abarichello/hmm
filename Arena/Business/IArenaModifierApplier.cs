using System;
using System.Collections.Generic;
using HeavyMetalMachines.Arena.Infra;

namespace HeavyMetalMachines.Arena.Business
{
	public interface IArenaModifierApplier
	{
		void Init(int playerId, List<int> alliesIds, List<int> enemiesIds);

		void AddConditionIfValid(ArenaModifierCondition condition);

		void Dispose();
	}
}
