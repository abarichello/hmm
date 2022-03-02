using System;
using System.Collections.Generic;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Battlepass
{
	public class PlayWithDriverRoleInMatchUpdater : IMissionProgressUpdater
	{
		public PlayWithDriverRoleInMatchUpdater(IPlayerStats playerStats, IMatchPlayers matchPlayers, Mission mission)
		{
			this._playerStats = playerStats;
			this._matchPlayers = matchPlayers;
			this._mission = mission;
		}

		public void Update(MissionProgressValue progressValue, Objectives missionObjectives)
		{
			float usageOfDriverRoleInMatch = this.GetUsageOfDriverRoleInMatch(missionObjectives);
			float num;
			if (this._playerStats.MatchWon)
			{
				num = usageOfDriverRoleInMatch * this._mission.TargetVictoryModifier;
			}
			else
			{
				num = usageOfDriverRoleInMatch;
			}
			progressValue.CurrentValue += num;
		}

		private float GetUsageOfDriverRoleInMatch(Objectives missionObjectives)
		{
			float result = 0f;
			List<PlayerData> playersAndBotsByTeam = this._matchPlayers.GetPlayersAndBotsByTeam(TeamKind.Zero);
			for (int i = 0; i < playersAndBotsByTeam.Count; i++)
			{
				PlayerData playerData = playersAndBotsByTeam[i];
				CharacterItemTypeComponent component = playerData.CharacterItemType.GetComponent<CharacterItemTypeComponent>();
				if (component.Role == missionObjectives.DriverRoleKind)
				{
					result = 1f;
					break;
				}
			}
			return result;
		}

		private readonly IPlayerStats _playerStats;

		private readonly IMatchPlayers _matchPlayers;

		private readonly Mission _mission;
	}
}
