using System;
using EnhancedUI.EnhancedScroller;
using HeavyMetalMachines.Frontend;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassMissionsListMissionCellView : EnhancedScrollerCellView
	{
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
				this._bgRawImage.texture = this._assets.BgEmptyTexture;
			}
			else
			{
				this._titleText.text = data.TitleText;
				this._descriptionText.text = data.DescriptionText;
				this._xpText.text = data.XpAmount.ToString("0");
				string text = HudUtils.RGBToHex(this._progressCurrentValueTextColor);
				string text2 = HudUtils.RGBToHex((!data.IsPremium) ? this._progressTotalValueFreeTextColor : this._progressTotalValuePremiumTextColor);
				this._progressText.text = string.Format("<color=#{0}>{1}</color><color=#{2}> / {3}</color>", new object[]
				{
					(!data.IsCompleted) ? text : text2,
					data.ProgressCurrentValue,
					text2,
					data.ProgressTotalValue
				});
				this._progressImage.fillAmount = ((data.ProgressTotalValue != 0) ? ((float)data.ProgressCurrentValue / (float)data.ProgressTotalValue) : 1f);
				this._progressImage.sprite = ((!data.IsPremium) ? this._assets.FreeProgressBarSprite : this._assets.PremiumProgressBarSprite);
				this._progressBgImage.color = ((!data.IsPremium) ? this._progressBarBgFreeColor : this._progressBarBgPremiumColor);
				this._ticketRawImage.texture = ((!data.IsPremium) ? this._assets.TicketFreeTexture : this._assets.TicketPremiumTexture);
				if (data.IsPremium)
				{
					this._bgRawImage.texture = ((!data.IsLocked) ? this._assets.BgPremiumTexture : this._assets.BgPremiumLockedTexture);
				}
				else
				{
					this._bgRawImage.texture = this._assets.BgFreeTexture;
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

		protected void OnEnable()
		{
			if (this._showUnlockAnimationOnEnable)
			{
				this._showUnlockAnimationOnEnable = false;
				this._unlockPremiumAnimation.Play();
			}
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
		private RawImage _bgRawImage;

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
		private Text _descriptionText;

		[SerializeField]
		private GameObject _completedIconGameObject;

		[SerializeField]
		private RawImage _ticketRawImage;

		[SerializeField]
		private Text _progressText;

		[SerializeField]
		private Image _progressImage;

		[SerializeField]
		private Image _progressBgImage;

		[SerializeField]
		private Animation _unlockPremiumAnimation;

		[SerializeField]
		private Button _unlockPremiumButton;

		private UnityUiBattlepassMissionsListMissionCellView.OnUnlockPremiumClickDelegate _onUnlockPremiumClick;

		private bool _showUnlockAnimationOnEnable;

		public delegate void OnUnlockPremiumClickDelegate();

		[Serializable]
		private struct MissionCellViewAssets
		{
			public Texture BgEmptyTexture;

			public Texture BgFreeTexture;

			public Texture BgPremiumLockedTexture;

			public Texture BgPremiumTexture;

			public Texture TicketFreeTexture;

			public Texture TicketPremiumTexture;

			public Sprite FreeProgressBarSprite;

			public Sprite PremiumProgressBarSprite;
		}
	}
}
