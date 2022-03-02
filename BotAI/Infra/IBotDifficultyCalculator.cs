using System;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.BotAI.Infra
{
	public interface IBotDifficultyCalculator
	{
		BotAIGoal.BotDifficulty Calculate(TeamKind team);
	}
}
