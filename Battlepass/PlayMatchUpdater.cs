using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class PlayMatchUpdater : IMissionProgressUpdater
	{
		public PlayMatchUpdater(IPlayerStats playerStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			float num = (!this._playerStats.MatchWon) ? 1f : (1f * this._mission.TargetVictoryModifier);
			progressValue.CurrentValue += num;
		}

		private const int ONE_MATCH = 1;

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;
	}
}
