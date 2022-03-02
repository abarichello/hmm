using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.GameStates.MainMenu.Progression;
using HeavyMetalMachines.Utils;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.EndMatch.Battlepass
{
	public class EndMatchBattlepassMissionWindow : MonoBehaviour
	{
		private UiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event EndMatchBattlepassMissionWindow.OnHideDelegate _onHideEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event EndMatchBattlepassViewHeader.OnLevelUpDetected _onLevelUpDetected;

		public void Dispose()
		{
			this._header.Dispose();
			List<Transform> childList = this._missionsGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].GetComponent<EndMatchBattlepassMissionSlot>().Dispose();
			}
			base.StopAllCoroutines();
			if (this._navigationSelectionDisposable != null)
			{
				this._navigationSelectionDisposable.Dispose();
				this._navigationSelectionDisposable = null;
			}
			this._missionsGrid.onCustomSort = null;
			this._onHideEvent = null;
			this._onLevelUpDetected = null;
		}

		public void Setup(EndMatchBattlepassViewHeader.HeaderData headerData, List<EndMatchBattlepassMissionSlot.MissionSlotData> missionSlotDatas, EndMatchBattlepassMissionWindow.OnHideDelegate onHideEvent, EndMatchBattlepassViewHeader.OnLevelUpDetected onLevelUpDetected, IAnimationSync animationSync)
		{
			this._onHideEvent = onHideEvent;
			this._onLevelUpDetected = onLevelUpDetected;
			this._isPlayerLeaver = headerData.IsPlayerLeaver;
			this._header.Setup(headerData, animationSync);
			this._afkInfoGameObject.SetActive(this._isPlayerLeaver);
			this.SetupMissionSlots(missionSlotDatas);
			this._missionsTitleGameObject.SetActive(!this._isPlayerLeaver && missionSlotDatas.Count > 0);
			this._noActiveMissionsGameObject.SetActive(!this._isPlayerLeaver && missionSlotDatas.Count == 0);
			this._okButtonAnimation.gameObject.GetComponent<NGUIWidgetAlpha>().Alpha = 0f;
			this._okButtonCollider.enabled = false;
			this._levelUpTriggered = false;
			this._navigationSelectionDisposable = ObservableExtensions.Subscribe<int>(Observable.Do<int>(this._uiNavigationAxisSelector.ObserveNavigationSelectionId(), new Action<int>(this.EnableSubGroupOnSelectionTransfor)));
		}

		private void EnableSubGroupOnSelectionTransfor(int selectedTransformId)
		{
			List<Transform> childList = this._missionsGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				EndMatchBattlepassMissionSlot component = childList[i].GetComponent<EndMatchBattlepassMissionSlot>();
				int instanceID = component.GamepadFeedbackSelectionButton.transform.GetInstanceID();
				if (instanceID == selectedTransformId)
				{
					component.UiNavigationSubGroupHolder.SubGroupFocusGet();
				}
				else
				{
					component.UiNavigationSubGroupHolder.SubGroupFocusRelease();
				}
			}
		}

		private void SetupMissionSlots(List<EndMatchBattlepassMissionSlot.MissionSlotData> missionSlotDatas)
		{
			this._missionsGrid.hideInactive = false;
			List<Transform> childList = this._missionsGrid.GetChildList();
			if (this._isPlayerLeaver)
			{
				for (int i = 0; i < childList.Count; i++)
				{
					childList[i].gameObject.SetActive(false);
				}
				return;
			}
			missionSlotDatas.Sort(new Comparison<EndMatchBattlepassMissionSlot.MissionSlotData>(this.MissionSlotDatasSort));
			for (int j = 0; j < childList.Count; j++)
			{
				if (j < missionSlotDatas.Count)
				{
					childList[j].GetComponent<EndMatchBattlepassMissionSlot>().Setup(missionSlotDatas[j]);
					childList[j].gameObject.SetActive(true);
				}
				else
				{
					childList[j].gameObject.SetActive(false);
				}
			}
			this._missionsGrid.hideInactive = true;
			this._missionsGrid.Reposition();
		}

		private int MissionSlotDatasSort(EndMatchBattlepassMissionSlot.MissionSlotData slotData1, EndMatchBattlepassMissionSlot.MissionSlotData slotData2)
		{
			int num = string.Compare(slotData1.NameText, slotData2.NameText, StringComparison.Ordinal);
			if (num == 0 && slotData1.IsCompleted == slotData2.IsCompleted && slotData1.IsPremium == slotData2.IsPremium)
			{
				return 0;
			}
			if (slotData1.IsCompleted && slotData1.IsPremium)
			{
				return -1;
			}
			if (slotData2.IsCompleted && slotData2.IsPremium)
			{
				return 1;
			}
			if (slotData1.IsCompleted)
			{
				return -1;
			}
			if (slotData2.IsCompleted)
			{
				return 1;
			}
			if (slotData1.IsPremium)
			{
				return -1;
			}
			if (slotData2.IsPremium)
			{
				return 1;
			}
			return num;
		}

		public void Show()
		{
			this.UiNavigationGroupHolder.AddGroup();
			this._windowAnimation.Play("mission_complete_in");
			base.StartCoroutine(this.AnimateSlots());
		}

		private IEnumerator AnimateSlots()
		{
			if (this._isPlayerLeaver)
			{
				this.AnimateShowOkButtonAndEnableCollider();
				yield break;
			}
			yield return new WaitForSeconds(this._delayBeforeFlipSlotTimeInSec);
			List<Transform> missionsGridChildList = this._missionsGrid.GetChildList();
			for (int i = 0; i < missionsGridChildList.Count; i++)
			{
				EndMatchBattlepassMissionSlot missionSlot = missionsGridChildList[i].GetComponent<EndMatchBattlepassMissionSlot>();
				missionSlot.PlayCardInAnimation();
				yield return new WaitForSeconds(this._missionsSlotFlipDelayTimeInSec);
				yield return base.StartCoroutine(missionSlot.ShowProgressBarAnimationCoroutine(this._missionsSlotProgresssTimeInSec));
				if (missionSlot.IsCompleted())
				{
					missionSlot.PlayMissionCompletedAnimation();
					yield return new WaitForSeconds(this._missionsSlotCheckDelayTimeInSec);
				}
				yield return new WaitForSeconds(this._missionsDelayBetweenSlotsTimeInSec);
			}
			yield return new WaitForSeconds(this._delayBeforeBonusTimeInSec);
			for (int j = 0; j < missionsGridChildList.Count; j++)
			{
				EndMatchBattlepassMissionSlot missionSlot2 = missionsGridChildList[j].GetComponent<EndMatchBattlepassMissionSlot>();
				if (missionSlot2.IsCompleted())
				{
					missionSlot2.PlayRewardAnimation();
					yield return new WaitForSeconds(this._missionSlotXpDelayBeforeHeaderTimeInSec);
					yield return base.StartCoroutine(this._header.PlayMissionXpAnimationCoroutine(missionSlot2.GetXpAmount(), this._headerBonusTimeInSec, this._headerDelayAfterBonusTimeInSec));
				}
			}
			yield return base.StartCoroutine(this._header.PlayBonusXpRewardAnimationCoroutine(this._headerBonusTimeInSec, this._headerDelayAfterBonusTimeInSec));
			yield return new WaitForSeconds(this._delayBeforeTransferAnimationTimeInSec);
			float transferAnimationTimeInSec = Mathf.Max(this._transferAnimationMinTimeInSec, (float)this._header.GetTotalXpGain() / (float)this._transferAnimationXpPerSecond);
			yield return base.StartCoroutine(this._header.PlayRewardTransferAnimationCoroutine(transferAnimationTimeInSec, new EndMatchBattlepassViewHeader.OnLevelUpDetected(this.OnLevelUpDetected)));
			this.AnimateShowOkButtonAndEnableCollider();
			this._uiNavigationAxisSelector.RebuildAndSelect();
			yield break;
		}

		private void OnLevelUpDetected(int level)
		{
			if (this._onLevelUpDetected != null)
			{
				this._onLevelUpDetected(level);
			}
			if (!this._levelUpTriggered)
			{
				this._windowAnimation.Play("mission_complete_transition");
			}
			this._levelUpTriggered = true;
		}

		public void Hide()
		{
			this.UiNavigationGroupHolder.RemoveGroup();
			this._windowAnimation.Play("mission_complete_out");
			this._okButtonCollider.enabled = false;
			if (this._onHideEvent != null)
			{
				this._onHideEvent();
			}
		}

		public void OnOkButtonClick()
		{
			this.Hide();
		}

		private void AnimateShowOkButtonAndEnableCollider()
		{
			this._okButtonAnimation.Play("mission_complete_button_in");
			this._okButtonCollider.enabled = true;
		}

		private void AnimateHideOkButton()
		{
			this._okButtonAnimation.Play("mission_complete_button_out");
		}

		[SerializeField]
		private Animation _windowAnimation;

		[SerializeField]
		private EndMatchBattlepassViewHeader _header;

		[SerializeField]
		private GameObject _missionsTitleGameObject;

		[SerializeField]
		private UIGrid _missionsGrid;

		[SerializeField]
		private BoxCollider _okButtonCollider;

		[SerializeField]
		private Animation _okButtonAnimation;

		[SerializeField]
		private GameObject _noActiveMissionsGameObject;

		[SerializeField]
		private GameObject _afkInfoGameObject;

		[Header("[Configs]")]
		[SerializeField]
		private float _delayBeforeFlipSlotTimeInSec = 1f;

		[SerializeField]
		private float _missionsSlotFlipDelayTimeInSec = 0.1f;

		[SerializeField]
		private float _missionsSlotProgresssTimeInSec = 0.3f;

		[SerializeField]
		private float _missionsSlotCheckDelayTimeInSec;

		[SerializeField]
		private float _missionsDelayBetweenSlotsTimeInSec = 0.5f;

		[SerializeField]
		private float _delayBeforeBonusTimeInSec = 1f;

		[SerializeField]
		private float _missionSlotXpDelayBeforeHeaderTimeInSec = 0.1f;

		[SerializeField]
		private float _headerBonusTimeInSec = 1f;

		[SerializeField]
		private float _headerDelayAfterBonusTimeInSec = 1f;

		[SerializeField]
		private float _delayBeforeTransferAnimationTimeInSec = 0.5f;

		[SerializeField]
		private float _transferAnimationMinTimeInSec = 1f;

		[SerializeField]
		private int _transferAnimationXpPerSecond = 1000;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		private bool _isPlayerLeaver;

		private bool _levelUpTriggered;

		private IDisposable _navigationSelectionDisposable;

		public delegate void OnHideDelegate();
	}
}
