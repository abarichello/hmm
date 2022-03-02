using System;
using System.Collections;
using System.Collections.Generic;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Utils;
using Hoplon.Input;
using Hoplon.Input.UiNavigation;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.EndMatch.Battlepass
{
	public class EndMatchBattlepassMissionSlot : MonoBehaviour
	{
		public bool IsCompleted()
		{
			return this._missionSlotData.IsCompleted;
		}

		public int GetXpAmount()
		{
			return this._missionSlotData.XpAmount;
		}

		public IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		public UIButton GamepadFeedbackSelectionButton
		{
			get
			{
				return this._gamepadFeedbackSelectionButton;
			}
		}

		public void Setup(EndMatchBattlepassMissionSlot.MissionSlotData slotData)
		{
			base.transform.Rotate(0f, 90f, 0f);
			base.GetComponent<NGUIWidgetAlpha>().Alpha = 0f;
			this._checkMarkWidgetAlpha.gameObject.SetActive(slotData.IsCompleted);
			this._xpAmountLabel.text = slotData.XpAmount.ToString("0");
			this._nameLabel.text = slotData.NameText;
			this._borderTexture.mainTexture = ((!slotData.IsPremium) ? ((!slotData.IsCompleted) ? this._freeBorderTexture : this._freeCompletedBorderTexture) : ((!slotData.IsCompleted) ? this._premiumBorderTexture : this._premiumCompletedBorderTexture));
			UIWidget boxXpTexture = this._boxXpTexture;
			Texture mainTexture = (!slotData.IsPremium) ? this._freeBoxXpTexture : this._premiumBoxXpTexture;
			this._boxXpAuraTexture.mainTexture = mainTexture;
			boxXpTexture.mainTexture = mainTexture;
			this.CreateNewObjectivesCells(slotData);
			this.FillObjectiveInformation(slotData);
			this._rewardWidgetAlpha.Alpha = ((!slotData.IsCompleted) ? this._incompleteRewardAlpha : this._completeRewardAlpha);
			this._ticketTexture.mainTexture = ((!slotData.IsPremium) ? this._freeTicketTexture : this._premiumTicketTexture);
			this._ticketTooltipTrigger.TooltipText = Language.Get((!slotData.IsPremium) ? "BATTLEPASS_REWARD_CATEGORY_FREE" : "BATTLEPASS_REWARD_CATEGORY_PREMIUM", TranslationContext.Battlepass);
			this._missionSlotData = slotData;
			this._objectiveGrid.Reposition();
			IInputActiveDeviceChangeNotifier inputActiveDeviceChangeNotifier = this._diContainer.Resolve<IInputActiveDeviceChangeNotifier>();
			this._activeDeviceDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(inputActiveDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), delegate(InputDevice device)
			{
				bool enabled = device == 3;
				this._gamepadFeedbackSelectionButton.enabled = enabled;
			}));
		}

		private void CreateNewObjectivesCells(EndMatchBattlepassMissionSlot.MissionSlotData slotData)
		{
			if (slotData.Objectives.Length < 2)
			{
				return;
			}
			for (int i = 1; i < slotData.Objectives.Length; i++)
			{
				EndMatchBattlepassObjectiveMissionSlot item = Object.Instantiate<EndMatchBattlepassObjectiveMissionSlot>(this._objectivesList[i - 1], this._objectivesList[i - 1].ObjectiveAnchorTransform);
				this._objectivesList.Add(item);
			}
		}

		private void FillObjectiveInformation(EndMatchBattlepassMissionSlot.MissionSlotData slotData)
		{
			this.OrdenateMissionObjectives(slotData.Objectives);
			for (int i = 0; i < slotData.Objectives.Length; i++)
			{
				EndMatchBattlepassObjectiveMissionSlot endMatchBattlepassObjectiveMissionSlot = this._objectivesList[i];
				int num = Math.Min(slotData.Objectives[i].CurrentProgressAmount, slotData.Objectives[i].ProgressMaxAmount);
				endMatchBattlepassObjectiveMissionSlot.ProgressLabel.text = string.Format("{0}/{1}", num, slotData.Objectives[i].ProgressMaxAmount);
				endMatchBattlepassObjectiveMissionSlot.ProgressSliderTexture.fillAmount = 0f;
				endMatchBattlepassObjectiveMissionSlot.DescriptionLabel.text = slotData.Objectives[i].DescriptionText;
				endMatchBattlepassObjectiveMissionSlot.ProgressSliderTexture.mainTexture = ((!slotData.IsPremium) ? this._freeProgressBarTexture : this._premiumProgressBarTexture);
				if (i < slotData.Objectives.Length - 1)
				{
					endMatchBattlepassObjectiveMissionSlot.SeparatorObject.SetActive(true);
				}
			}
		}

		private void OrdenateMissionObjectives(EndMatchBattlepassMissionSlot.ObjectiveSlotData[] data)
		{
			Array.Sort<EndMatchBattlepassMissionSlot.ObjectiveSlotData>(data, new Comparison<EndMatchBattlepassMissionSlot.ObjectiveSlotData>(this.CompareGreatestProgress));
		}

		private int CompareGreatestProgress(EndMatchBattlepassMissionSlot.ObjectiveSlotData objectiveSlotData, EndMatchBattlepassMissionSlot.ObjectiveSlotData slotData)
		{
			float value = (float)objectiveSlotData.CurrentProgressAmount / (float)objectiveSlotData.ProgressMaxAmount;
			return ((float)slotData.CurrentProgressAmount / (float)slotData.ProgressMaxAmount).CompareTo(value);
		}

		public void PlayCardInAnimation()
		{
			this._missionCompletedAnimation.Play("mission_complete_card_in");
		}

		public IEnumerator ShowProgressBarAnimationCoroutine(float progressDurationTimeInSec)
		{
			for (int i = 0; i < this._objectivesList.Count; i++)
			{
				EndMatchBattlepassObjectiveMissionSlot objective = this._objectivesList[i];
				if (this._missionSlotData.Objectives[i].CurrentProgressAmount != 0)
				{
					for (float timer = 0f; timer < progressDurationTimeInSec; timer += Time.deltaTime)
					{
						float normalizedTime = timer / progressDurationTimeInSec;
						float progressValue = Mathf.Lerp(0f, (float)this._missionSlotData.Objectives[i].CurrentProgressAmount, normalizedTime);
						objective.ProgressSliderTexture.fillAmount = progressValue / (float)this._missionSlotData.Objectives[i].ProgressMaxAmount;
						yield return null;
					}
					objective.ProgressSliderTexture.fillAmount = (float)this._missionSlotData.Objectives[i].CurrentProgressAmount / (float)this._missionSlotData.Objectives[i].ProgressMaxAmount;
				}
			}
			yield break;
		}

		public void PlayMissionCompletedAnimation()
		{
			this._missionCompletedAnimation.Play("mission_complete");
		}

		public void PlayRewardAnimation()
		{
			this._rewardAnimation.Play("mission_complete_reward_feedback");
		}

		public void Dispose()
		{
			base.StopAllCoroutines();
			this.DisposeActiveDevice();
		}

		private void OnDestroy()
		{
			this.DisposeActiveDevice();
		}

		private void DisposeActiveDevice()
		{
			if (this._activeDeviceDisposable != null)
			{
				this._activeDeviceDisposable.Dispose();
				this._activeDeviceDisposable = null;
			}
		}

		[Header("[GUI Components]")]
		[SerializeField]
		private NGUIWidgetAlpha _rewardWidgetAlpha;

		[SerializeField]
		private Animation _missionCompletedAnimation;

		[SerializeField]
		private Animation _rewardAnimation;

		[SerializeField]
		private UILabel _xpAmountLabel;

		[SerializeField]
		private UILabel _nameLabel;

		[SerializeField]
		private NGUIWidgetAlpha _checkMarkWidgetAlpha;

		[SerializeField]
		private UITexture _borderTexture;

		[SerializeField]
		private UITexture _boxXpTexture;

		[SerializeField]
		private UITexture _boxXpAuraTexture;

		[SerializeField]
		private UITexture _ticketTexture;

		[SerializeField]
		private HMMTooltipTrigger _ticketTooltipTrigger;

		[SerializeField]
		private List<EndMatchBattlepassObjectiveMissionSlot> _objectivesList;

		[SerializeField]
		private UIGrid _objectiveGrid;

		[SerializeField]
		private UIButton _gamepadFeedbackSelectionButton;

		[Header("[Assets]")]
		[SerializeField]
		private Texture _freeTicketTexture;

		[SerializeField]
		private Texture _premiumTicketTexture;

		[SerializeField]
		private Texture _freeProgressBarTexture;

		[SerializeField]
		private Texture _premiumProgressBarTexture;

		[SerializeField]
		private Texture _freeBoxXpTexture;

		[SerializeField]
		private Texture _premiumBoxXpTexture;

		[SerializeField]
		private Texture _freeBorderTexture;

		[SerializeField]
		private Texture _freeCompletedBorderTexture;

		[SerializeField]
		private Texture _premiumBorderTexture;

		[SerializeField]
		private Texture _premiumCompletedBorderTexture;

		[SerializeField]
		[Range(0f, 1f)]
		private float _completeRewardAlpha = 1f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _incompleteRewardAlpha = 0.2f;

		[Header("UiNavigation")]
		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		private EndMatchBattlepassMissionSlot.MissionSlotData _missionSlotData;

		[Inject]
		private DiContainer _diContainer;

		private IDisposable _activeDeviceDisposable;

		[Serializable]
		public struct MissionSlotData
		{
			public int MissionIndex;

			public bool IsPremium;

			public bool IsCompleted;

			public int XpAmount;

			public string NameText;

			public EndMatchBattlepassMissionSlot.ObjectiveSlotData[] Objectives;
		}

		[Serializable]
		public struct ObjectiveSlotData
		{
			public int CurrentProgressAmount;

			public int ProgressMaxAmount;

			public string DescriptionText;
		}
	}
}
