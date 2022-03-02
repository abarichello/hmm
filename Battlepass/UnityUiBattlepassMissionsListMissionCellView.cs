using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassMissionsListMissionCellView : EnhancedScrollerCellView
	{
		public Text GetObjectiveTextLabel
		{
			get
			{
				return this._objectivesDescriptionList[0];
			}
		}

		public Text GetMissionTitleLabel
		{
			get
			{
				return this._titleText;
			}
		}

		public Text GetObjectiveProgressTextLabel
		{
			get
			{
				return this._objectiveProgressBarList[0].ProgressText;
			}
		}

		public void Setup(UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData data, UnityUiBattlepassMissionsListMissionCellView.OnUnlockPremiumClickDelegate onUnlockPremiumClick)
		{
			this._onUnlockPremiumClick = onUnlockPremiumClick;
			this.InternalSetup(data);
		}

		private void InternalSetup(UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData data)
		{
			this._showUnlockAnimationOnEnable = false;
			this._emptyTextGameObject.SetActive(data.IsEmpty);
			this._activeMissionGameObject.SetActive(!data.IsEmpty && !data.IsLocked);
			this._premiumLockedGameObject.SetActive(data.IsPremium && data.IsLocked);
			this._completedIconGameObject.SetActive(data.IsCompleted);
			if (data.IsEmpty)
			{
				this._bgImage.sprite = this._assets.BgEmptyTexture;
				this.DisableUnusedObjectivesCells(data);
			}
			else
			{
				this._titleText.text = data.TitleText;
				this._xpText.text = data.XpAmount.ToString("0");
				this.CreateNewsObjectivesCells(data);
				string currentTextColor = HudUtils.RGBToHex(this._progressCurrentValueTextColor);
				string totalTextColor = HudUtils.RGBToHex((!data.IsPremium) ? this._progressTotalValueFreeTextColor : this._progressTotalValuePremiumTextColor);
				this.FillObjectivesCells(data, totalTextColor, currentTextColor);
				this._ticketRawImage.texture = ((!data.IsPremium) ? this._assets.TicketFreeTexture : this._assets.TicketPremiumTexture);
				if (data.IsPremium)
				{
					this._bgImage.sprite = ((!data.IsLocked) ? this._assets.BgPremiumTexture : this._assets.BgPremiumLockedTexture);
				}
				else
				{
					this._bgImage.sprite = this._assets.BgFreeTexture;
				}
			}
			this._unlockPremiumButton.interactable = true;
			if (data.TriggerUnlockAnimation)
			{
				this._unlockPremiumButton.interactable = false;
				this._premiumLockedGameObject.SetActive(true);
				if (!base.gameObject.activeInHierarchy)
				{
					this._showUnlockAnimationOnEnable = true;
				}
				else
				{
					this._unlockPremiumAnimation.Play();
				}
			}
		}

		private void FillObjectivesCells(UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData data, string totalTextColor, string currentTextColor)
		{
			UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[] array = this.OrdenateMissionObjectives(data);
			for (int i = 0; i < array.Length; i++)
			{
				Text text = this._objectivesDescriptionList[i];
				MissionObjectiveProgressBar missionObjectiveProgressBar = this._objectiveProgressBarList[i];
				text.gameObject.SetActive(true);
				missionObjectiveProgressBar.gameObject.SetActive(true);
				int num = Math.Min(array[i].ProgressCurrentValue, array[i].ProgressTotalValue);
				missionObjectiveProgressBar.ProgressText.text = string.Format("<color=#{0}>{1}</color><color=#{2}> / {3}</color>", new object[]
				{
					(!data.IsCompleted) ? currentTextColor : totalTextColor,
					num,
					totalTextColor,
					array[i].ProgressTotalValue
				});
				missionObjectiveProgressBar.ProgressImage.fillAmount = ((array[i].ProgressTotalValue != 0) ? ((float)array[i].ProgressCurrentValue / (float)array[i].ProgressTotalValue) : 1f);
				missionObjectiveProgressBar.ProgressImage.sprite = ((!data.IsPremium) ? this._assets.FreeProgressBarSprite : this._assets.PremiumProgressBarSprite);
				missionObjectiveProgressBar.ProgressBgImage.color = ((!data.IsPremium) ? this._progressBarBgFreeColor : this._progressBarBgPremiumColor);
				text.text = array[i].DescriptionText;
				this.EnableSeparatorObjectiveObject(i);
			}
			this.DisableUnusedObjectivesCells(data);
		}

		private UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[] OrdenateMissionObjectives(UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData data)
		{
			UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[] array = (UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData[])data.ObjectiveList.Clone();
			Array.Sort<UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData>(array, new Comparison<UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData>(this.CompareGreatestProgress));
			return array;
		}

		private int CompareGreatestProgress(UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData objectiveSlotData, UnityUiBattlepassMissionsListMissionsGroupPresenter.ObjectiveSlotData slotData)
		{
			float value = (objectiveSlotData.ProgressTotalValue != 0) ? ((float)objectiveSlotData.ProgressCurrentValue / (float)objectiveSlotData.ProgressTotalValue) : 1f;
			return ((slotData.ProgressTotalValue != 0) ? ((float)slotData.ProgressCurrentValue / (float)slotData.ProgressTotalValue) : 1f).CompareTo(value);
		}

		private void EnableSeparatorObjectiveObject(int currentIndex)
		{
			if (currentIndex != 0)
			{
				this._objectiveSeparatorList[currentIndex].SetActive(true);
				return;
			}
			this._objectiveSeparatorList[currentIndex].SetActive(false);
		}

		private void DisableUnusedObjectivesCells(UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData data)
		{
			if (data.ObjectiveList.Length >= this._objectivesDescriptionList.Count)
			{
				return;
			}
			for (int i = data.ObjectiveList.Length; i < this._objectivesDescriptionList.Count; i++)
			{
				Text text = this._objectivesDescriptionList[i];
				MissionObjectiveProgressBar missionObjectiveProgressBar = this._objectiveProgressBarList[i];
				text.gameObject.SetActive(false);
				missionObjectiveProgressBar.gameObject.SetActive(false);
			}
			for (int j = data.ObjectiveList.Length; j < this._objectiveSeparatorList.Count; j++)
			{
				this._objectiveSeparatorList[j].SetActive(false);
			}
		}

		private void CreateNewsObjectivesCells(UnityUiBattlepassMissionsListMissionsGroupPresenter.MissionSlotData data)
		{
			if (data.ObjectiveList.Length <= 1 || this._objectivesDescriptionList.Count >= data.ObjectiveList.Length)
			{
				return;
			}
			for (int i = 1; i < data.ObjectiveList.Length; i++)
			{
				GameObject item = Object.Instantiate<GameObject>(this._objectiveSeparatorList[0], this._objectiveGridTransform);
				this._objectiveSeparatorList.Add(item);
				Text item2 = Object.Instantiate<Text>(this._objectivesDescriptionList[0], this._objectiveGridTransform);
				this._objectivesDescriptionList.Add(item2);
				MissionObjectiveProgressBar item3 = Object.Instantiate<MissionObjectiveProgressBar>(this._objectiveProgressBarList[0], this._objectiveGridTransform);
				this._objectiveProgressBarList.Add(item3);
			}
		}

		protected void OnEnable()
		{
			if (this._showUnlockAnimationOnEnable)
			{
				this._showUnlockAnimationOnEnable = false;
				this._unlockPremiumAnimation.Play();
			}
		}

		public void SetBorderCellSize(float height)
		{
			Vector2 sizeDelta;
			sizeDelta..ctor(this._bgImage.rectTransform.sizeDelta.x, height);
			this._bgImage.rectTransform.sizeDelta = sizeDelta;
		}

		[UnityUiComponentCall]
		public void OnUnlockPremiumClick()
		{
			this._onUnlockPremiumClick();
		}

		[SerializeField]
		private Color _progressBarBgFreeColor;

		[SerializeField]
		private Color _progressBarBgPremiumColor;

		[SerializeField]
		private Color _progressCurrentValueTextColor;

		[SerializeField]
		private Color _progressTotalValueFreeTextColor;

		[SerializeField]
		private Color _progressTotalValuePremiumTextColor;

		[SerializeField]
		private UnityUiBattlepassMissionsListMissionCellView.MissionCellViewAssets _assets;

		[SerializeField]
		private Image _bgImage;

		[SerializeField]
		private GameObject _emptyTextGameObject;

		[SerializeField]
		private GameObject _activeMissionGameObject;

		[SerializeField]
		private GameObject _premiumLockedGameObject;

		[SerializeField]
		private Text _xpText;

		[SerializeField]
		private Text _titleText;

		[SerializeField]
		private GameObject _completedIconGameObject;

		[SerializeField]
		private RawImage _ticketRawImage;

		[SerializeField]
		private Animation _unlockPremiumAnimation;

		[SerializeField]
		private Button _unlockPremiumButton;

		[SerializeField]
		private RectTransform _objectiveGridTransform;

		[SerializeField]
		private List<Text> _objectivesDescriptionList;

		[SerializeField]
		private List<GameObject> _objectiveSeparatorList;

		[SerializeField]
		private List<MissionObjectiveProgressBar> _objectiveProgressBarList;

		private UnityUiBattlepassMissionsListMissionCellView.OnUnlockPremiumClickDelegate _onUnlockPremiumClick;

		private bool _showUnlockAnimationOnEnable;

		public delegate void OnUnlockPremiumClickDelegate();

		[Serializable]
		private struct MissionCellViewAssets
		{
			public Sprite BgEmptyTexture;

			public Sprite BgFreeTexture;

			public Sprite BgPremiumLockedTexture;

			public Sprite BgPremiumTexture;

			public Texture TicketFreeTexture;

			public Texture TicketPremiumTexture;

			public Sprite FreeProgressBarSprite;

			public Sprite PremiumProgressBarSprite;
		}
	}
}
