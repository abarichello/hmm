using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class MedalCountOnVictoriesUpdater : IMissionProgressUpdater
	{
		public MedalCountOnVictoriesUpdater(IPlayerStats playerStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			if (this._playerStats.MatchWon)
			{
				float num = (float)this._playerStats.NumberOfMedals * this._mission.TargetVictoryModifier;
				progressValue.CurrentValue += num;
			}
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;
	}
}
