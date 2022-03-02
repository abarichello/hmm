using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;

namespace HeavyMetalMachines.Battlepass
{
	public class BattlepassMissionValidator
	{
		public void ValidateMissionCompletion(Mission[] missionList, BattlepassProgress battlepassProgress, IPlayerStats playerStats, IMatchStats matchHistory, IMatchPlayers mathPlayers, ICollectionScriptableObject inventoryCollection)
		{
			List<MissionProgress> missionProgresses = battlepassProgress.MissionProgresses;
			List<MissionCompleted> missionsCompletedIndex = playerStats.MissionsCompletedIndex;
			for (int i = 0; i < missionProgresses.Count; i++)
			{
				MissionProgress missionProgress = missionProgresses[i];
				int objectiveCompletedIndex = 0;
				if (this.CheckCompletion(missionProgress, playerStats, matchHistory, mathPlayers, missionList, inventoryCollection, out objectiveCompletedIndex))
				{
					MissionCompleted item = new MissionCompleted
					{
						MissionIndex = missionProgress.MissionIndex,
						ObjectiveCompletedIndex = objectiveCompletedIndex,
						CurrentProgressValue = missionProgress.CurrentProgressValue
					};
					missionsCompletedIndex.Add(item);
				}
			}
		}

		private bool CheckCompletion(MissionProgress missionProgress, IPlayerStats playerStats, IMatchStats matchHistory, IMatchPlayers mathPlayers, Mission[] missionList, ICollectionScriptableObject inventoryCollection, out int objectiveIndex)
		{
			Mission mission = missionList[missionProgress.MissionIndex];
			MissionObjectiveToProgressUpdaterMapper missionObjectiveToProgressUpdaterMapper = new MissionObjectiveToProgressUpdaterMapper(playerStats, matchHistory, mathPlayers, mission, inventoryCollection);
			objectiveIndex = 0;
			bool result = false;
			for (int i = 0; i < mission.Objectives.Length; i++)
			{
				BattlepassMissionValidator.Log.DebugFormat("Validating mission progress. Index: {0}", new object[]
				{
					missionProgress.MissionIndex
				});
				Objectives missionObjectives = mission.Objectives[i];
				IMissionProgressUpdater missionProgressUpdater = missionObjectiveToProgressUpdaterMapper.MapperResolver(missionObjectives.Objective);
				if (missionProgressUpdater == null)
				{
					BattlepassMissionValidator.Log.ErrorFormat("Not implemented MissionObjective: {0}", new object[]
					{
						missionObjectives.Objective
					});
				}
				else
				{
					missionProgressUpdater.Update(missionProgress.CurrentProgressValue[i], missionObjectives);
					if (missionProgress.CurrentProgressValue[i].CurrentValue >= (float)missionObjectives.Target)
					{
						objectiveIndex = i;
						result = true;
					}
				}
			}
			return result;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BattlepassMissionValidator));
	}
}
