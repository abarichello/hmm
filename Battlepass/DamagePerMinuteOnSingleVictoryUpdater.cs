using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class DamagePerMinuteOnSingleVictoryUpdater : IMissionProgressUpdater
	{
		public DamagePerMinuteOnSingleVictoryUpdater(IPlayerStats playerStats, IMatchStats matchStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._matchHistory = matchStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			if (this._playerStats.MatchWon)
			{
				float currentValue = this._playerStats.GetDamagePerMinuteDealt(this._matchHistory) * this._mission.TargetVictoryModifier;
				progressValue.CurrentValue = currentValue;
			}
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;

		private readonly IMatchStats _matchHistory;
	}
}
