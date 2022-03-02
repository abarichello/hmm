using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class WinMatchUpdater : IMissionProgressUpdater
	{
		public WinMatchUpdater(IPlayerStats playerStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			if (this._playerStats.MatchWon)
			{
				progressValue.CurrentValue += 1f * this._mission.TargetVictoryModifier;
			}
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;

		private const int ONE_MATCH = 1;
	}
}
