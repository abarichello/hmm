using System;
using System.Collections.Generic;
using Commons.Swordfish.Battlepass;
using Commons.Swordfish.Progression;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.EndMatch.Battlepass
{
	public class EndMatchBattlepassView : GameHubBehaviour
	{
		protected void Awake()
		{
			this._useTestData = false;
		}

		public bool TryToShowWindow(EndMatchBattlepassView.OnWindowClosed onWindowClosedCallback, BattlepassProgressScriptableObject battlepassProgress)
		{
			this._onWindowClosedCallback = onWindowClosedCallback;
			if (!this.HasPendingMissions())
			{
				return false;
			}
			base.gameObject.SetActive(true);
			if (GameHubBehaviour.Hub == null)
			{
				this.FakeSetup();
			}
			else
			{
				EndMatchBattlepassViewHeader.HeaderData headerData = this.LoadHeaderData();
				List<EndMatchBattlepassMissionSlot.MissionSlotData> list = this.LoadMissions(GameHubBehaviour.Hub.MatchHistory.CurrentPlayerReward.MissionsCompleted, GameHubBehaviour.Hub.MatchHistory.CurrentPlayerReward.OldMissionProgresses, GameHubBehaviour.Hub.MatchHistory.CurrentPlayerReward.MissionProgresses, GameHubBehaviour.Hub.SharedConfigs.Battlepass.Mission);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsCompleted)
					{
						headerData.HasMissionCompleted = true;
						break;
					}
				}
				this._missionWindow.Setup(headerData, list, new EndMatchBattlepassMissionWindow.OnHideDelegate(this.OnMissionWindowHide), new EndMatchBattlepassViewHeader.OnLevelUpDetected(this.OnLevelUpDetected), this._levelUpWindow);
				this._levelUpWindow.Setup(headerData.OldLevel, battlepassProgress.Progress.HasPremium(), GameHubBehaviour.Hub.SharedConfigs.Battlepass.Levels, false);
			}
			this._missionWindow.Show();
			return true;
		}

		private void FakeSetup()
		{
			GameObject gameObject = new GameObject("FakeLoadingManager - Dont Save In Scene");
			gameObject.AddComponent<LoadingManager>();
			gameObject.transform.SetSiblingIndex(2);
			BitLogger.Initialize(CppFileAppender.GetMainLogger());
			EndMatchBattlepassViewHeader.HeaderData headerData = new EndMatchBattlepassViewHeader.HeaderData
			{
				IsPlayerLeaver = false,
				OldLevel = 1,
				OldLevelProgressXp = 900,
				MaxXpPerLevel = new List<int>
				{
					0,
					1000,
					2000,
					6000
				},
				MatchBonusXp = 1000,
				PerformanceBonusXp = 100,
				FoundersBonusXp = 300,
				EventBonusXp = 100,
				BoosterBonusXp = 500
			};
			bool flag = false;
			List<EndMatchBattlepassMissionSlot.MissionSlotData> list = new List<EndMatchBattlepassMissionSlot.MissionSlotData>(4);
			for (int i = 0; i < 4; i++)
			{
				EndMatchBattlepassMissionSlot.MissionSlotData missionSlotData = new EndMatchBattlepassMissionSlot.MissionSlotData
				{
					IsPremium = (i % 2 == 0),
					IsCompleted = (i >= 2),
					XpAmount = 1750,
					CurrentProgressAmount = 5 * i,
					ProgressMaxAmount = ((i < 2) ? (6 * i + 1) : (5 * i)),
					NameText = "mission_" + i,
					DescriptionText = "description_" + i
				};
				EndMatchBattlepassMissionSlot.MissionSlotData item = missionSlotData;
				flag |= item.IsCompleted;
				list.Add(item);
			}
			headerData.HasMissionCompleted = flag;
			this._headerDataTest.HasMissionCompleted = false;
			for (int j = 0; j < this._missionSlotDatasTest.Count; j++)
			{
				EndMatchBattlepassMissionSlot.MissionSlotData missionSlotData = this._missionSlotDatasTest[j];
				if (missionSlotData.IsCompleted)
				{
					this._headerDataTest.HasMissionCompleted = true;
					break;
				}
			}
			this._missionWindow.Setup((!this._useTestData) ? headerData : this._headerDataTest, (!this._useTestData) ? list : this._missionSlotDatasTest, new EndMatchBattlepassMissionWindow.OnHideDelegate(this.OnMissionWindowHide), new EndMatchBattlepassViewHeader.OnLevelUpDetected(this.OnLevelUpDetected), this._levelUpWindow);
			this._levelUpWindow.Setup((!this._useTestData) ? headerData.OldLevel : this._headerDataTest.OldLevel, false, new ProgressionLevel[1], true);
		}

		private void OnLevelUpDetected(int level)
		{
			this._levelUpWindow.DoLevelUp(level);
		}

		private void OnMissionWindowHide()
		{
			if (this._onWindowClosedCallback != null)
			{
				this._onWindowClosedCallback();
				this._onWindowClosedCallback = null;
			}
		}

		private bool HasPendingMissions()
		{
			return true;
		}

		protected void OnDestroy()
		{
			this._missionWindow.Dispose();
		}

		private EndMatchBattlepassViewHeader.HeaderData LoadHeaderData()
		{
			RewardsBag currentPlayerReward = GameHubBehaviour.Hub.MatchHistory.CurrentPlayerReward;
			int oldBattlepassXP = currentPlayerReward.OldBattlepassXP;
			BattlepassConfig battlepass = GameHubBehaviour.Hub.SharedConfigs.Battlepass;
			int levelForXp = battlepass.GetLevelForXp(oldBattlepassXP);
			ProgressionLevel[] levels = battlepass.Levels;
			List<int> list = new List<int>(levels.Length);
			int num = 0;
			for (int i = 0; i < levels.Length; i++)
			{
				int xp = levels[i].XP;
				list.Add(xp - num);
				num = xp;
			}
			return new EndMatchBattlepassViewHeader.HeaderData
			{
				IsPlayerLeaver = GameHubBehaviour.Hub.Players.CurrentPlayerData.IsLeaver,
				OldLevel = levelForXp,
				OldLevelProgressXp = currentPlayerReward.OldBattlepassXP - levels[levelForXp].XP,
				MaxXpPerLevel = list,
				MatchBonusXp = currentPlayerReward.XpByMatchtime + currentPlayerReward.XpByWin,
				PerformanceBonusXp = currentPlayerReward.XpByPerformance,
				FoundersBonusXp = currentPlayerReward.XpByFounderBonus,
				EventBonusXp = currentPlayerReward.XpByEvent,
				BoosterBonusXp = currentPlayerReward.XpByBoost
			};
		}

		private List<EndMatchBattlepassMissionSlot.MissionSlotData> LoadMissions(int[] missionsCompleted, MissionProgress[] oldMissionProgresses, MissionProgress[] missionProgresses, MissionConfig bpMissionConfig)
		{
			List<EndMatchBattlepassMissionSlot.MissionSlotData> list = new List<EndMatchBattlepassMissionSlot.MissionSlotData>(4);
			int num = 0;
			while (num < oldMissionProgresses.Length && list.Count < 4)
			{
				MissionProgress missionProgress = oldMissionProgresses[num];
				bool flag = false;
				for (int i = 0; i < missionsCompleted.Length; i++)
				{
					if (missionProgress.MissionIndex == missionsCompleted[i])
					{
						list.Add(this.CreateMissionSlotData(true, missionProgress, bpMissionConfig));
						flag = true;
						break;
					}
				}
				if (!flag && missionProgresses != null)
				{
					foreach (MissionProgress missionProgress2 in missionProgresses)
					{
						if (missionProgress2.MissionIndex == missionProgress.MissionIndex)
						{
							if (missionProgress2.CurrentValue != missionProgress.CurrentValue)
							{
								list.Add(this.CreateMissionSlotData(false, missionProgress2, bpMissionConfig));
							}
							break;
						}
					}
				}
				num++;
			}
			return list;
		}

		private EndMatchBattlepassMissionSlot.MissionSlotData CreateMissionSlotData(bool isCompleted, MissionProgress missionProgress, MissionConfig bpMissionConfig)
		{
			Mission mission = bpMissionConfig.Missions[missionProgress.MissionIndex];
			return new EndMatchBattlepassMissionSlot.MissionSlotData
			{
				MissionIndex = missionProgress.MissionIndex,
				IsPremium = missionProgress.IsPremium(bpMissionConfig),
				IsCompleted = isCompleted,
				XpAmount = mission.XpReward,
				CurrentProgressAmount = ((!isCompleted) ? Mathf.FloorToInt(missionProgress.CurrentValue) : mission.Target),
				ProgressMaxAmount = mission.Target,
				NameText = Language.Get(mission.NameDraft, TranslationSheets.BattlepassMissions),
				DescriptionText = string.Format(Language.Get(mission.DescriptionDraft, TranslationSheets.BattlepassMissions), mission.Target)
			};
		}

		private static readonly BitLogger Log = new BitLogger(typeof(EndMatchBattlepassView));

		[SerializeField]
		private EndMatchBattlepassMissionWindow _missionWindow;

		[SerializeField]
		private EndMatchBattlepassLevelUpWindow _levelUpWindow;

		[Header("[Test Only]")]
		[SerializeField]
		private bool _useTestData;

		[SerializeField]
		private EndMatchBattlepassViewHeader.HeaderData _headerDataTest;

		[SerializeField]
		private List<EndMatchBattlepassMissionSlot.MissionSlotData> _missionSlotDatasTest;

		private EndMatchBattlepassView.OnWindowClosed _onWindowClosedCallback;

		public delegate void OnWindowClosed();
	}
}
