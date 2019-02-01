using System;
using Commons.Swordfish.Battlepass;
using Commons.Swordfish.Progression;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.UnityUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassMissionsListMissionsGroupPresenter : MonoBehaviour, IEnhancedScrollerDelegate, IListMissionsGroupPresenterSelector
	{
		public void Setup(IBattlepassBuyUiActions buyUiActions, bool hasPremium, MissionConfig missionConfig, BattlepassProgress progress)
		{
			this._buyUiActions = buyUiActions;
			this._showActiveMissions = true;
			this._noCompletedMissionsGameObject.SetActive(false);
			if (!this._tryToShowUnlockPremiumAnimation && !hasPremium)
			{
				this._tryToShowUnlockPremiumAnimation = true;
			}
			this._activeMissionsData = new SmallList<UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData>();
			this._completedMissionsData = new SmallList<UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData>();
			if (SceneManager.GetActiveScene() != base.gameObject.scene)
			{
				int num = 4;
				if (!hasPremium)
				{
					num--;
				}
				if (progress.MissionProgresses != null)
				{
					int num2 = 0;
					while (num2 < progress.MissionProgresses.Count && num2 < num)
					{
						MissionProgress missionProgress = progress.MissionProgresses[num2];
						Mission mission = missionConfig.Missions[missionProgress.MissionIndex];
						UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData item = new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
						{
							IsEmpty = false,
							IsCompleted = false,
							IsLocked = false,
							IsPremium = missionProgress.IsPremium(missionConfig),
							DescriptionText = string.Format(Language.Get(mission.DescriptionDraft, TranslationSheets.BattlepassMissions), mission.Target),
							ProgressCurrentValue = Mathf.FloorToInt(missionProgress.CurrentValue),
							ProgressTotalValue = mission.Target,
							TitleText = Language.Get(mission.NameDraft, TranslationSheets.BattlepassMissions),
							XpAmount = mission.XpReward
						};
						this._activeMissionsData.Add(item);
						num2++;
					}
				}
				if (this._activeMissionsData.Count < num)
				{
					for (int i = this._activeMissionsData.Count; i < num; i++)
					{
						this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
						{
							IsEmpty = true
						});
					}
				}
				if (!hasPremium)
				{
					this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
					{
						IsPremium = true,
						IsLocked = true
					});
				}
				if (progress.MissionsCompleted != null)
				{
					for (int j = 0; j < progress.MissionsCompleted.Length; j++)
					{
						if (progress.MissionsCompleted[j])
						{
							Mission mission2 = missionConfig.Missions[j];
							UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData item2 = new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
							{
								IsEmpty = false,
								IsPremium = (mission2.IsPremium > 0),
								IsCompleted = true,
								DescriptionText = string.Format(Language.Get(mission2.DescriptionDraft, TranslationSheets.BattlepassMissions), mission2.Target),
								IsLocked = false,
								ProgressCurrentValue = mission2.Target,
								ProgressTotalValue = mission2.Target,
								TitleText = Language.Get(mission2.NameDraft, TranslationSheets.BattlepassMissions),
								XpAmount = mission2.XpReward
							};
							this._completedMissionsData.Add(item2);
						}
					}
				}
			}
			else
			{
				this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
				{
					IsCompleted = false,
					TitleText = "Title",
					DescriptionText = "Description",
					ProgressCurrentValue = 100,
					ProgressTotalValue = 256
				});
				this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
				{
					IsCompleted = false,
					IsPremium = true,
					TitleText = "Title",
					DescriptionText = "Description",
					ProgressCurrentValue = 856,
					ProgressTotalValue = 2500
				});
				this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
				{
					IsEmpty = true
				});
				if (hasPremium)
				{
					this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
					{
						IsEmpty = true
					});
				}
				else
				{
					this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
					{
						IsPremium = true,
						IsLocked = true
					});
				}
				for (int k = 0; k < 99; k++)
				{
					this._completedMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
					{
						IsEmpty = false,
						IsPremium = (k % 4 == 0),
						IsCompleted = true,
						IsLocked = false,
						TitleText = "Title " + k,
						DescriptionText = "Description " + k,
						ProgressCurrentValue = k,
						ProgressTotalValue = k,
						XpAmount = 100 + k
					});
				}
			}
			this._missionSlotCellViewHeight = this._missionCellViewPrefab.GetComponent<RectTransform>().sizeDelta.y;
			this._scroller.Delegate = this;
			this.UpdateListInfoGroup();
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return (!this._showActiveMissions) ? this._completedMissionsData.Count : this._activeMissionsData.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return this._missionSlotCellViewHeight;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UnityUiBattlepassMissionsListMissionCellView unityUiBattlepassMissionsListMissionCellView = scroller.GetCellView(this._missionCellViewPrefab) as UnityUiBattlepassMissionsListMissionCellView;
			if (!this._showActiveMissions)
			{
				unityUiBattlepassMissionsListMissionCellView.Setup(this._completedMissionsData[dataIndex], new UnityUiBattlepassMissionsListMissionCellView.OnUnlockPremiumClickDelegate(this.OnUnlockPremiumClick));
			}
			else
			{
				UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData missionSlotData = this._activeMissionsData[dataIndex];
				unityUiBattlepassMissionsListMissionCellView.Setup(missionSlotData, new UnityUiBattlepassMissionsListMissionCellView.OnUnlockPremiumClickDelegate(this.OnUnlockPremiumClick));
				if (missionSlotData.TriggerUnlockAnimation)
				{
					missionSlotData.TriggerUnlockAnimation = false;
					this._activeMissionsData[dataIndex] = missionSlotData;
				}
			}
			return unityUiBattlepassMissionsListMissionCellView;
		}

		private void OnUnlockPremiumClick()
		{
			this._rewardsToggleInfo.SetToggleValue(true);
			this._buyUiActions.OnUnlockPremiumButtonClick(true);
		}

		public void ToggleMission(bool selectActiveMissions)
		{
			this._showActiveMissions = selectActiveMissions;
			this._noCompletedMissionsGameObject.SetActive(!selectActiveMissions && this._completedMissionsData.Count == 0);
			this._scroller.ReloadData(0f);
			this.UpdateListInfoGroup();
		}

		private void UpdateListInfoGroup()
		{
			int num = 0;
			int num2 = 0;
			if (this._showActiveMissions)
			{
				for (int i = 0; i < this._activeMissionsData.Count; i++)
				{
					UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData missionSlotData = this._activeMissionsData[i];
					if (!missionSlotData.IsEmpty && !missionSlotData.IsLocked)
					{
						num2++;
					}
					if (!missionSlotData.IsLocked)
					{
						num++;
					}
				}
			}
			else
			{
				num = this._completedMissionsData.Count;
			}
			this._listInfoGroupPresenter.Setup(this._showActiveMissions, num2, num);
		}

		public void TryPremiumUnlock()
		{
			if (this._tryToShowUnlockPremiumAnimation)
			{
				this._tryToShowUnlockPremiumAnimation = false;
				UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData value = this._activeMissionsData[this._activeMissionsData.Count - 1];
				value.TriggerUnlockAnimation = true;
				this._activeMissionsData[this._activeMissionsData.Count - 1] = value;
				this._scroller.ReloadData(0f);
				this.UpdateListInfoGroup();
			}
		}

		private const int NumMaxActiveMissionSlots = 4;

		private SmallList<UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData> _activeMissionsData;

		private SmallList<UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData> _completedMissionsData;

		[SerializeField]
		private EnhancedScroller _scroller;

		[SerializeField]
		private EnhancedScrollerCellView _missionCellViewPrefab;

		[SerializeField]
		private UnityUiBattlepassMissionsListInfoGroupPresenter _listInfoGroupPresenter;

		[SerializeField]
		private UnityUiToggleInfo _rewardsToggleInfo;

		[SerializeField]
		private GameObject _noCompletedMissionsGameObject;

		private IBattlepassBuyUiActions _buyUiActions;

		private bool _showActiveMissions;

		private bool _tryToShowUnlockPremiumAnimation;

		private float _missionSlotCellViewHeight;

		public struct MissionSlotData
		{
			public bool IsEmpty;

			public bool IsPremium;

			public bool IsLocked;

			public bool IsCompleted;

			public bool TriggerUnlockAnimation;

			public string TitleText;

			public string DescriptionText;

			public int XpAmount;

			public int ProgressCurrentValue;

			public int ProgressTotalValue;
		}
	}
}
