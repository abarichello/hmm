using System;
using System.Collections.Generic;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.BotAI.Infra;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Match.Infra;

namespace HeavyMetalMachines.BotAI.Business
{
	public class BotDifficultyCalculatorByOwnTeam : IBotDifficultyCalculator
	{
		public BotDifficultyCalculatorByOwnTeam(IBotDifficultyGameArenaInfo botDifficultyGameArenaInfo, IMatchPlayersProvider matchPlayersProvider)
		{
			this._botDifficultyGameArenaInfo = botDifficultyGameArenaInfo;
			this._matchPlayersProvider = matchPlayersProvider;
		}

		public BotAIGoal.BotDifficulty Calculate(TeamKind team)
		{
			int mmr = 0;
			IMatchPlayers getMatchPlayers = this._matchPlayersProvider.GetMatchPlayers;
			if (team != TeamKind.Red)
			{
				if (team == TeamKind.Blue)
				{
					bool flag = getMatchPlayers.IsTeamBotOnly(getMatchPlayers.GetPlayersAndBotsByTeam(TeamKind.Blue));
					if (flag)
					{
						mmr = this._matchPlayersProvider.GetMatchPlayers.RedMMR;
						return this.CalculateDifficultyForTeamWithBotOnly(mmr);
					}
					mmr = this._matchPlayersProvider.GetMatchPlayers.BlueMMR;
				}
			}
			else
			{
				bool flag = getMatchPlayers.IsTeamBotOnly(getMatchPlayers.GetPlayersAndBotsByTeam(TeamKind.Red));
				if (flag)
				{
					mmr = this._matchPlayersProvider.GetMatchPlayers.BlueMMR;
					return this.CalculateDifficultyForTeamWithBotOnly(mmr);
				}
				mmr = this._matchPlayersProvider.GetMatchPlayers.RedMMR;
			}
			return this.GetBotDifficulty(mmr);
		}

		private BotAIGoal.BotDifficulty CalculateDifficultyForTeamWithBotOnly(int mmr)
		{
			List<BotDifficultyData> configListForBotDifficultyByOwnTeam = this._botDifficultyGameArenaInfo.GetConfigListForBotDifficultyByOwnTeam();
			for (int i = configListForBotDifficultyByOwnTeam.Count - 1; i >= 0; i--)
			{
				BotDifficultyData botDifficultyData = configListForBotDifficultyByOwnTeam[i];
				if (botDifficultyData.MinimumMMRForTier <= mmr)
				{
					return botDifficultyData.BotOnlyTeam;
				}
			}
			throw new ArgumentException(string.Format("Config for MMR {0} not found in BotDifficultycalculatedByOwnTeam", mmr));
		}

		private BotAIGoal.BotDifficulty GetBotDifficulty(int mmr)
		{
			List<BotDifficultyData> configListForBotDifficultyByOwnTeam = this._botDifficultyGameArenaInfo.GetConfigListForBotDifficultyByOwnTeam();
			for (int i = configListForBotDifficultyByOwnTeam.Count - 1; i >= 0; i--)
			{
				BotDifficultyData botDifficultyData = configListForBotDifficultyByOwnTeam[i];
				if (botDifficultyData.MinimumMMRForTier <= mmr)
				{
					return botDifficultyData.HumanTeam;
				}
			}
			throw new ArgumentException(string.Format("Config for MMR {0} not found in BotDifficultycalculatedByOwnTeam", mmr));
		}

		private readonly IBotDifficultyGameArenaInfo _botDifficultyGameArenaInfo;

		private readonly IMatchPlayersProvider _matchPlayersProvider;
	}
}
