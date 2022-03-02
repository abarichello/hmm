using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class HealOnVictoriesUpdater : IMissionProgressUpdater
	{
		public HealOnVictoriesUpdater(IPlayerStats playerStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			if (this._playerStats.MatchWon)
			{
				float num = this._playerStats.HealingProvided * this._mission.TargetVictoryModifier;
				progressValue.CurrentValue += num;
			}
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;
	}
}
