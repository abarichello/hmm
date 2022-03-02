﻿using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Battlepass
{
	public class TeamDamageUpdater : IMissionProgressUpdater
	{
		public TeamDamageUpdater(IPlayerStats playerStats, IMatchStats matchStats, Mission mission)
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
				return matchHistory.GetTotalRedTeamDamage() * progressModifier;
			}
			return matchHistory.GetTotalRedTeamDamage();
		}

		private float HandleBlueTeam(IPlayerStats playerStats, IMatchStats matchHistory, float progressModifier)
		{
			if (playerStats.MatchWon)
			{
				return matchHistory.GetTotalBlueTeamDamage() * progressModifier;
			}
			return matchHistory.GetTotalBlueTeamDamage();
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;

		private readonly IMatchStats _matchHistory;
	}
}
