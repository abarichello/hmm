using System;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.BotAI
{
	public interface IGetBotDifficulty
	{
		BotAIGoal.BotDifficulty Get(TeamKind team);
	}
}
