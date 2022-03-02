using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Battlepass
{
	public class TeamSupportKillUpdater : IMissionProgressUpdater
	{
		public TeamSupportKillUpdater(IPlayerStats playerStats, IMatchStats matchStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._matchHistory = matchStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			float num = 0f;
			if (this._playerStats.Team == TeamKind.Red)
			{
				num = this.HandleRedTeam(this._playerStats, this._matchHistory, this._mission.TargetVictoryModifier);
			}
			else if (this._playerStats.Team == TeamKind.Blue)
			{
				num = this.HandleBlueTeam(this._playerStats, this._matchHistory, this._mission.TargetVictoryModifier);
			}
			progressValue.CurrentValue += num;
		}

		private float HandleRedTeam(IPlayerStats playerStats, IMatchStats matchHistory, float progressModifier)
		{
			if (playerStats.MatchWon)
			{
				return (float)matchHistory.GetTotalSupportKillsRed() * progressModifier;
			}
			return (float)matchHistory.GetTotalSupportKillsRed();
		}

		private float HandleBlueTeam(IPlayerStats playerStats, IMatchStats matchHistory, float progressModifier)
		{
			if (playerStats.MatchWon)
			{
				return (float)matchHistory.GetTotalSupportKillsBlue() * progressModifier;
			}
			return (float)matchHistory.GetTotalSupportKillsBlue();
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;

		private readonly IMatchStats _matchHistory;
	}
}
