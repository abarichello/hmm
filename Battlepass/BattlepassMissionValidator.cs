using System;
using System.Collections.Generic;
using Commons.Swordfish.Battlepass;
using Commons.Swordfish.Progression;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Battlepass
{
	public class BattlepassMissionValidator : GameHubObject
	{
		public void ValidateMissionCompletion(BattlepassProgress battlepassProgress, IPlayerStats playerStats, IMatchStats matchHistory)
		{
			List<MissionProgress> missionProgresses = battlepassProgress.MissionProgresses;
			List<int> missionsCompletedIndex = playerStats.MissionsCompletedIndex;
			for (int i = 0; i < missionProgresses.Count; i++)
			{
				MissionProgress missionProgress = missionProgresses[i];
				Mission[] missions = GameHubObject.Hub.SharedConfigs.Battlepass.Mission.Missions;
				if (this.CheckCompletion(missionProgress, playerStats, matchHistory, missions))
				{
					missionsCompletedIndex.Add(missionProgress.MissionIndex);
				}
			}
		}

		private bool CheckCompletion(MissionProgress missionProgress, IPlayerStats playerStats, IMatchStats matchHistory, Mission[] missionList)
		{
			Mission mission = missionList[missionProgress.MissionIndex];
			switch (mission.Objective)
			{
			case MissionObjective.TeamDelivery:
			{
				TeamKind team = playerStats.Team;
				if (team != TeamKind.Red)
				{
					if (team == TeamKind.Blue)
					{
						missionProgress.CurrentValue += (float)matchHistory.GetDeliveriesBlue();
					}
				}
				else
				{
					missionProgress.CurrentValue += (float)matchHistory.GetDeliveriesRed();
				}
				break;
			}
			case MissionObjective.TeamHeal:
			{
				TeamKind team2 = playerStats.Team;
				if (team2 != TeamKind.Red)
				{
					if (team2 == TeamKind.Blue)
					{
						missionProgress.CurrentValue += matchHistory.GetTotalBlueTeamHeal();
					}
				}
				else
				{
					missionProgress.CurrentValue += matchHistory.GetTotalRedTeamHeal();
				}
				break;
			}
			case MissionObjective.TeamDamage:
			{
				TeamKind team3 = playerStats.Team;
				if (team3 != TeamKind.Red)
				{
					if (team3 == TeamKind.Blue)
					{
						missionProgress.CurrentValue += matchHistory.GetTotalBlueTeamDamage();
					}
				}
				else
				{
					missionProgress.CurrentValue += matchHistory.GetTotalRedTeamDamage();
				}
				break;
			}
			case MissionObjective.TeamKill:
			{
				TeamKind team4 = playerStats.Team;
				if (team4 != TeamKind.Red)
				{
					if (team4 == TeamKind.Blue)
					{
						missionProgress.CurrentValue += (float)matchHistory.GetTotalKillsBlue();
					}
				}
				else
				{
					missionProgress.CurrentValue += (float)matchHistory.GetTotalKillsRed();
				}
				break;
			}
			case MissionObjective.TeamBombCarrierKill:
			{
				TeamKind team5 = playerStats.Team;
				if (team5 != TeamKind.Red)
				{
					if (team5 == TeamKind.Blue)
					{
						missionProgress.CurrentValue += (float)matchHistory.GetTotalBombCarrierKillsBlue();
					}
				}
				else
				{
					missionProgress.CurrentValue += (float)matchHistory.GetTotalBombCarrierKillsRed();
				}
				break;
			}
			case MissionObjective.TeamTacklerKill:
			{
				TeamKind team6 = playerStats.Team;
				if (team6 != TeamKind.Red)
				{
					if (team6 == TeamKind.Blue)
					{
						missionProgress.CurrentValue += (float)matchHistory.GetTotalTacklerKillsBlue();
					}
				}
				else
				{
					missionProgress.CurrentValue += (float)matchHistory.GetTotalTacklerKillsRed();
				}
				break;
			}
			case MissionObjective.TeamCarrierKill:
			{
				TeamKind team7 = playerStats.Team;
				if (team7 != TeamKind.Red)
				{
					if (team7 == TeamKind.Blue)
					{
						missionProgress.CurrentValue += (float)matchHistory.GetTotalCarrierKillsBlue();
					}
				}
				else
				{
					missionProgress.CurrentValue += (float)matchHistory.GetTotalCarrierKillsRed();
				}
				break;
			}
			case MissionObjective.TeamSupportKill:
			{
				TeamKind team8 = playerStats.Team;
				if (team8 != TeamKind.Red)
				{
					if (team8 == TeamKind.Blue)
					{
						missionProgress.CurrentValue += (float)matchHistory.GetTotalSupportKillsBlue();
					}
				}
				else
				{
					missionProgress.CurrentValue += (float)matchHistory.GetTotalSupportKillsRed();
				}
				break;
			}
			case MissionObjective.MedalCountOnSingleVictory:
				if (playerStats.MatchWon)
				{
					missionProgress.CurrentValue = (float)playerStats.NumberOfMedals;
				}
				break;
			case MissionObjective.DamagePerMinuteOnSingleVictory:
				if (playerStats.MatchWon)
				{
					missionProgress.CurrentValue = playerStats.GetDamagePerMinuteDealt(matchHistory);
				}
				break;
			case MissionObjective.HealPerMinuteOnSingleVictory:
				if (playerStats.MatchWon)
				{
					missionProgress.CurrentValue = playerStats.GetHealingPerMinuteProvided(matchHistory);
				}
				break;
			case MissionObjective.MedalCountOnVictories:
				if (playerStats.MatchWon)
				{
					missionProgress.CurrentValue += (float)playerStats.NumberOfMedals;
				}
				break;
			case MissionObjective.DamageOnVictories:
				if (playerStats.MatchWon)
				{
					missionProgress.CurrentValue += playerStats.DamageDealtToPlayers;
				}
				break;
			case MissionObjective.HealOnVictories:
				if (playerStats.MatchWon)
				{
					missionProgress.CurrentValue += playerStats.HealingProvided;
				}
				break;
			case MissionObjective.PlayMatch:
				missionProgress.CurrentValue += 1f;
				break;
			case MissionObjective.WinMatch:
				if (playerStats.MatchWon)
				{
					missionProgress.CurrentValue += 1f;
				}
				break;
			case MissionObjective.MedalCountOnMatches:
				missionProgress.CurrentValue += (float)playerStats.NumberOfMedals;
				break;
			default:
				BattlepassMissionValidator.Log.ErrorFormat("Not implemented MissionObjective: {0}", new object[]
				{
					mission.Objective
				});
				break;
			}
			return missionProgress.CurrentValue >= (float)mission.Target;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BattlepassMissionValidator));
	}
}
