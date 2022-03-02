using System;
using HeavyMetalMachines.Arena;

namespace HeavyMetalMachines.BotAI.Infra
{
	public interface IBotDifficultyCalculatorProvider
	{
		IBotDifficultyCalculator GetBotDifficultyCalculator(IBotDifficultyGameArenaInfo botDifficultyGameArenaInfo);
	}
}
