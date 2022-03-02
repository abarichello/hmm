using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class DeliveyOnMultipleMatchesUpdater : IMissionProgressUpdater
	{
		public DeliveyOnMultipleMatchesUpdater(IPlayerStats playerStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			float num;
			if (this._playerStats.MatchWon)
			{
				num = (float)this._playerStats.BombsDelivered * this._mission.TargetVictoryModifier;
			}
			else
			{
				num = (float)this._playerStats.BombsDelivered;
			}
			progressValue.CurrentValue += num;
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;
	}
}
