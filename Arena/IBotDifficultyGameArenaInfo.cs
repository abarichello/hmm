using System;
using System.Collections.Generic;
using Assets.ClientApiObjects.Specializations;

namespace HeavyMetalMachines.Arena
{
	public interface IBotDifficultyGameArenaInfo
	{
		BotDifficultyCalculatorKind CalculatorKind { get; }

		List<BotDifficultyData> GetConfigListForBotDifficultyByOtherTeam();

		List<BotDifficultyData> GetConfigListForBotDifficultyByOwnTeam();
	}
}
