using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	public class MissionObjectiveToProgressUpdaterMapper
	{
		public MissionObjectiveToProgressUpdaterMapper(IPlayerStats playerStats, IMatchStats matchHistory, IMatchPlayers matchPlayers, Mission mission, ICollectionScriptableObject allItemTypes)
		{
			this._mapper = new Dictionary<MissionObjective, IMissionProgressUpdater>
			{
				{
					12,
					new DamageOnVictoriesUpdater(playerStats, mission)
				},
				{
					13,
					new HealOnVictoriesUpdater(playerStats, mission)
				},
				{
					16,
					new MedalCountOnMatchesUpdater(playerStats, mission)
				},
				{
					11,
					new MedalCountOnVictoriesUpdater(playerStats, mission)
				},
				{
					9,
					new DamagePerMinuteOnSingleVictoryUpdater(playerStats, matchHistory, mission)
				},
				{
					10,
					new HealPerMinuteOnSingleVictoryUpdater(playerStats, matchHistory, mission)
				},
				{
					8,
					new MedalCountOnSingleVictoryUpdater(playerStats, mission)
				},
				{
					14,
					new PlayMatchUpdater(playerStats, mission)
				},
				{
					2,
					new TeamDamageUpdater(playerStats, matchHistory, mission)
				},
				{
					1,
					new TeamHealUpdater(playerStats, matchHistory, mission)
				},
				{
					3,
					new TeamKillUpdater(playerStats, matchHistory, mission)
				},
				{
					4,
					new TeamBombCarrierKillUpdater(playerStats, matchHistory, mission)
				},
				{
					6,
					new TeamCarrierKillUpdater(playerStats, matchHistory, mission)
				},
				{
					5,
					new TeamTacklerKillUpdater(playerStats, matchHistory, mission)
				},
				{
					7,
					new TeamSupportKillUpdater(playerStats, matchHistory, mission)
				},
				{
					0,
					new TeamDeliveryUpdater(playerStats, matchHistory, mission)
				},
				{
					15,
					new WinMatchUpdater(playerStats, mission)
				},
				{
					17,
					new PlayWithCharacterUpdater(playerStats, mission, allItemTypes)
				},
				{
					18,
					new PlayWithCharacterInMatchUpdater(playerStats, matchPlayers, mission, allItemTypes)
				},
				{
					19,
					new UseCustomizationUpdater(playerStats, mission, allItemTypes)
				},
				{
					20,
					new UseCustomizationInMatchUpdater(playerStats, matchPlayers, mission, allItemTypes)
				},
				{
					21,
					new PlayWithDriverRoleUpdater(playerStats, mission)
				},
				{
					22,
					new PlayWithDriverRoleInMatchUpdater(playerStats, matchPlayers, mission)
				},
				{
					23,
					new DamageOnSingleVictoryUpdater(playerStats, mission)
				},
				{
					24,
					new DeliveryOnSingleVictoryUpdater(playerStats, mission)
				},
				{
					25,
					new DeliveyOnMultipleMatchesUpdater(playerStats, mission)
				}
			};
		}

		public IMissionProgressUpdater MapperResolver(MissionObjective missionObjective)
		{
			if (this._mapper.ContainsKey(missionObjective))
			{
				return this._mapper[missionObjective];
			}
			return null;
		}

		private readonly Dictionary<MissionObjective, IMissionProgressUpdater> _mapper;
	}
}
