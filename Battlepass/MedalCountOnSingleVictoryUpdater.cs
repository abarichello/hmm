using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class MedalCountOnSingleVictoryUpdater : IMissionProgressUpdater
	{
		public MedalCountOnSingleVictoryUpdater(IPlayerStats playerStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			if (this._playerStats.MatchWon)
			{
				float currentValue = (float)this._playerStats.NumberOfMedals * this._mission.TargetVictoryModifier;
				progressValue.CurrentValue = currentValue;
			}
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;
	}
}
