using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;

namespace HeavyMetalMachines.BotAI
{
	public interface IBotAIDirectives
	{
		List<CombatObject> GetEnemies();

		bool IsPathFixed();
	}
}
