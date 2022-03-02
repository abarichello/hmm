using System;
using System.Collections.Generic;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.BotAI.Infra;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Match.Infra;

namespace HeavyMetalMachines.BotAI.Business
{
	public class BotDifficultyCalculatorByOtherTeam : IBotDifficultyCalculator
	{
		public BotDifficultyCalculatorByOtherTeam(IBotDifficultyGameArenaInfo botDifficultyGameArenaInfo, IMatchPlayersProvider matchPlayersProvider)
		{
			this._botDifficultyGameArenaInfo = botDifficultyGameArenaInfo;
			this._matchPlayersProvider = matchPlayersProvider;
		}

		public BotAIGoal.BotDifficulty Calculate(TeamKind team)
		{
			int mmr = 0;
			if (team != TeamKind.Red)
			{
				if (team == TeamKind.Blue)
				{
					mmr = this._matchPlayersProvider.GetMatchPlayers.RedMMR;
				}
			}
			else
			{
				mmr = this._matchPlayersProvider.GetMatchPlayers.BlueMMR;
			}
			return this.GetBotDifficulty(mmr);
		}

		private BotAIGoal.BotDifficulty GetBotDifficulty(int mmr)
		{
			List<BotDifficultyData> configListForBotDifficultyByOtherTeam = this._botDifficultyGameArenaInfo.GetConfigListForBotDifficultyByOtherTeam();
			for (int i = configListForBotDifficultyByOtherTeam.Count - 1; i >= 0; i--)
			{
				BotDifficultyData botDifficultyData = configListForBotDifficultyByOtherTeam[i];
				if (botDifficultyData.MinimumMMRForTier <= mmr)
				{
					return botDifficultyData.HumanTeam;
				}
			}
			throw new ArgumentException(string.Format("Config for MMR {0} not found in BotDifficultycalculatedByOtherTeam", mmr));
		}

		private readonly IBotDifficultyGameArenaInfo _botDifficultyGameArenaInfo;

		private readonly IMatchPlayersProvider _matchPlayersProvider;
	}
}
