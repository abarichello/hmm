using System;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class PlayWithDriverRoleUpdater : IMissionProgressUpdater
	{
		public PlayWithDriverRoleUpdater(IPlayerStats playerStats, Mission mission)
		{
			this._playerStats = playerStats;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			int num = (this._playerStats.CharacterRole != missionObjectives.DriverRoleKind) ? 0 : 1;
			float num2;
			if (this._playerStats.MatchWon)
			{
				num2 = (float)num * this._mission.TargetVictoryModifier;
			}
			else
			{
				num2 = (float)num;
			}
			progressValue.CurrentValue += num2;
		}

		private readonly IPlayerStats _playerStats;

		private readonly Mission _mission;
	}
}
