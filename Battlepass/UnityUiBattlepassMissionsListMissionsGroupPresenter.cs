using System;
using System.Collections.Generic;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Localization.Business;
using HeavyMetalMachines.UnityUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassMissionsListMissionsGroupPresenter : MonoBehaviour, IEnhancedScrollerDelegate, IListMissionsGroupPresenterSelector
	{
		public void Setup(IBattlepassBuyUiActions buyUiActions, bool hasPremium, MissionConfig missionConfig, BattlepassProgress progress)
		{
			this._buyUiActions = buyUiActions;
			this._showActiveMissions = true;
			this._noCompletedMissionsGameObject.SetActive(false);
			this._missionIndexToMissionCellHeight = new Dictionary<int, float>();
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
					this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
					{
						IsPremium = true,
						IsLocked = true,
						ObjectiveList = new UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[0],
						MissionIndex = -1
					});
				}
				if (progress.MissionProgresses != null)
				{
					int num2 = 0;
					while (num2 < progress.MissionProgresses.Count && num2 < num)
					{
						MissionProgress missionProgress = progress.MissionProgresses[num2];
						int missionIndex = missionProgress.MissionIndex;
						Mission mission = missionConfig.Missions[missionIndex];
						UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData missionSlotData = new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
						{
							MissionIndex = missionIndex,
							IsEmpty = false,
							IsCompleted = false,
							IsLocked = false,
							IsPremium = missionProgress.IsPremium(missionConfig),
							TitleText = Language.Get(mission.NameDraft, TranslationContext.BattlepassMissions),
							XpAmount = mission.XpReward
						};
						missionSlotData.ObjectiveList = this.InitializeMissionObjectives(mission.Objectives, missionProgress.CurrentProgressValue, missionProgress.MissionIndex);
						this._activeMissionsData.Add(missionSlotData);
						if (!this._missionIndexToMissionCellHeight.ContainsKey(missionIndex))
						{
							this._missionIndexToMissionCellHeight[missionIndex] = this.GetMissionSlotComponentsHeight(missionSlotData);
						}
						num2++;
					}
				}
				if (this._activeMissionsData.Count < 4)
				{
					for (int i = this._activeMissionsData.Count; i < 4; i++)
					{
						this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
						{
							IsEmpty = true,
							ObjectiveList = new UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[0],
							MissionIndex = -1
						});
					}
				}
				if (progress.MissionsCompleted != null)
				{
					for (int j = progress.MissionsCompleted.Count - 1; j > -1; j--)
					{
						int missionIndex2 = progress.MissionsCompleted[j].MissionIndex;
						Mission mission2 = missionConfig.Missions[missionIndex2];
						UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData missionSlotData2 = new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
						{
							MissionIndex = missionIndex2,
							IsEmpty = false,
							IsPremium = (mission2.IsPremium > 0),
							IsCompleted = true,
							IsLocked = false,
							TitleText = Language.Get(mission2.NameDraft, TranslationContext.BattlepassMissions),
							XpAmount = mission2.XpReward
						};
						missionSlotData2.ObjectiveList = this.InitializeMissionObjectives(mission2.Objectives, progress.MissionsCompleted[j].CurrentProgressValue, missionIndex2);
						this._completedMissionsData.Add(missionSlotData2);
						if (!this._missionIndexToMissionCellHeight.ContainsKey(missionIndex2))
						{
							this._missionIndexToMissionCellHeight[missionIndex2] = this.GetMissionSlotComponentsHeight(missionSlotData2);
						}
					}
				}
			}
			else
			{
				this.InitializeTestData(hasPremium);
			}
			this._missionSlotCellViewHeight = this._missionCellViewPrefab.GetComponent<RectTransform>().sizeDelta.y;
			this._scroller.Delegate = this;
			this.UpdateListInfoGroup();
		}

		private UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[] InitializeMissionObjectives(Objectives[] missionObjectives, MissionProgressValue[] missionProgress, int missionIndex)
		{
			UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[] array = new UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[missionObjectives.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DescriptionText = this._missionTranslator.GetLocalizedDescription(missionIndex, i);
				array[i].ProgressCurrentValue = ((missionProgress == null) ? missionObjectives[i].Target : Mathf.FloorToInt(missionProgress[i].CurrentValue));
				array[i].ProgressTotalValue = missionObjectives[i].Target;
			}
			return array;
		}

		private void QAHackShowMissionIndexInDescription(ref string text, int index)
		{
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return (!this._showActiveMissions) ? this._completedMissionsData.Count : this._activeMissionsData.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return this.GetMissionCellSize(dataIndex);
		}

		private float GetMissionCellSize(int dataIndex)
		{
			int missionIndex;
			int num;
			if (this._showActiveMissions)
			{
				missionIndex = this._activeMissionsData[dataIndex].MissionIndex;
				num = this._activeMissionsData[dataIndex].ObjectiveList.Length;
			}
			else
			{
				missionIndex = this._completedMissionsData[dataIndex].MissionIndex;
				num = this._completedMissionsData[dataIndex].ObjectiveList.Length;
			}
			if (num == 1)
			{
				return this._missionSlotCellViewHeight + this._missionObjectiveCellHeight;
			}
			if (this._missionIndexToMissionCellHeight.ContainsKey(missionIndex))
			{
				float num2 = this._missionIndexToMissionCellHeight[missionIndex];
				return this._missionSlotCellViewHeight + num2 + this._missionObjectiveCellHeight;
			}
			return this._missionSlotCellViewHeight + (float)num * this._missionObjectiveCellHeight;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UnityUiBattlepassMissionsListMissionCellView unityUiBattlepassMissionsListMissionCellView = scroller.GetCellView(this._missionCellViewPrefab) as UnityUiBattlepassMissionsListMissionCellView;
			if (!this._showActiveMissions)
			{
				UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData data = this._completedMissionsData[dataIndex];
				unityUiBattlepassMissionsListMissionCellView.Setup(data, new UnityUiBattlepassMissionsListMissionCellView.OnUnlockPremiumClickDelegate(this.OnUnlockPremiumClick));
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
			unityUiBattlepassMissionsListMissionCellView.SetBorderCellSize(this.GetMissionCellSize(dataIndex));
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

		private float GetMissionSlotComponentsHeight(UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData missionData)
		{
			float num = this._missionCellViewPrefab.GetMissionTitleLabel.rectTransform.sizeDelta.y;
			for (int i = 0; i < missionData.ObjectiveList.Length; i++)
			{
				num += this._missionCellViewPrefab.GetObjectiveProgressTextLabel.rectTransform.sizeDelta.y;
			}
			for (int j = 0; j < missionData.ObjectiveList.Length; j++)
			{
				num += HoplonUiUtils.GetTextPreferedSize(missionData.ObjectiveList[j].DescriptionText, this._missionCellViewPrefab.GetObjectiveTextLabel).y;
			}
			return num;
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
				UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData value = this._activeMissionsData[0];
				value.TriggerUnlockAnimation = true;
				this._activeMissionsData[0] = value;
				this._scroller.ReloadData(0f);
				this.UpdateListInfoGroup();
			}
		}

		private void InitializeTestData(bool hasPremium)
		{
			this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
			{
				IsCompleted = false,
				TitleText = "Title",
				ObjectiveList = new UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[]
				{
					new UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData
					{
						ProgressCurrentValue = 100,
						ProgressTotalValue = 256
					}
				}
			});
			this._activeMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
			{
				IsCompleted = false,
				IsPremium = true,
				TitleText = "Title",
				ObjectiveList = new UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[]
				{
					new UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData
					{
						ProgressCurrentValue = 856,
						ProgressTotalValue = 2500
					}
				}
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
			for (int i = 0; i < 99; i++)
			{
				this._completedMissionsData.Add(new UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData
				{
					IsEmpty = false,
					IsPremium = (i % 4 == 0),
					IsCompleted = true,
					IsLocked = false,
					TitleText = "Title " + i,
					ObjectiveList = new UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[]
					{
						new UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData
						{
							ProgressCurrentValue = i,
							ProgressTotalValue = i
						}
					},
					XpAmount = 100 + i
				});
			}
		}

		private const int NumMaxActiveMissionSlots = 4;

		private SmallList<UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData> _activeMissionsData;

		private SmallList<UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData> _completedMissionsData;

		private Dictionary<int, float> _missionIndexToMissionCellHeight;

		[SerializeField]
		private EnhancedScroller _scroller;

		[SerializeField]
		private UnityUiBattlepassMissionsListMissionCellView _missionCellViewPrefab;

		[SerializeField]
		private UnityUiBattlepassMissionsListInfoGroupPresenter _listInfoGroupPresenter;

		[SerializeField]
		private UnityUiToggleInfo _rewardsToggleInfo;

		[SerializeField]
		private GameObject _noCompletedMissionsGameObject;

		[SerializeField]
		private float _missionObjectiveCellHeight = 55f;

		private IBattlepassBuyUiActions _buyUiActions;

		private bool _showActiveMissions;

		private bool _tryToShowUnlockPremiumAnimation;

		private float _missionSlotCellViewHeight;

		[Inject]
		private IMissionTranslator _missionTranslator;

		public struct MissionSlotData
		{
			public int MissionIndex;

			public bool IsEmpty;

			public bool IsPremium;

			public bool IsLocked;

			public bool IsCompleted;

			public bool TriggerUnlockAnimation;

			public string TitleText;

			public int XpAmount;

			public UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[] ObjectiveList;
		}

		public struct ObjectiveSlotData
		{
			public int ProgressCurrentValue;

			public int ProgressTotalValue;

			public string DescriptionText;
		}
	}
}
