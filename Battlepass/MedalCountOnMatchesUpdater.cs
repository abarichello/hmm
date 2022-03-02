using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class MedalCountOnMatchesUpdater : IMissionProgressUpdater
	{
		public MedalCountOnMatchesUpdater(IPlayerStats playerStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			float num = (!this._playerStats.MatchWon) ? ((float)this._playerStats.NumberOfMedals) : ((float)this._playerStats.NumberOfMedals * this._mission.TargetVictoryModifier);
			progressValue.CurrentValue += num;
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;
	}
}
