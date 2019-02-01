using System;
using System.Diagnostics;
using FMod;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudInstancesShopItem : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private event HudInstancesShopItem.InstancesShopItemOnClickDelegate ShopItemOnClickEvent;

		public string GetUpgradeInfoName()
		{
			return this._upgradeInfoName;
		}

		protected void OnDestroy()
		{
			this.ShopItemOnClickEvent = null;
			this.ItemButton.onClick.Clear();
		}

		public void Setup(UpgradeInfo upgradeInfo, HudInstancesShopItem.InstancesShopItemOnClickDelegate onClickCallback, bool didAnimation)
		{
			this._upgradeInfoName = upgradeInfo.Name;
			base.GetComponent<NGUIWidgetAlpha>().alpha = 1f;
			this.PivotWidgetAlpha.alpha = ((!didAnimation) ? 0f : 1f);
			if (!didAnimation)
			{
				this.SelectionGui.SetActive(false);
			}
			this.SetCategoryData(upgradeInfo.Category);
			this.TitleLabel.text = Language.Get(upgradeInfo.ContentName, TranslationSheets.Instances);
			this.DescriptionLabel.text = upgradeInfo.LocalizedDescription;
			this.DescriptionSummaryLabel.text = Language.Get((!string.IsNullOrEmpty(upgradeInfo.DescriptionSummary)) ? upgradeInfo.DescriptionSummary : "NO INFO", TranslationSheets.Instances);
			this.IconSprite.SpriteName = HudUtils.GetInstanceIconName(GameHubBehaviour.Hub.Players.CurrentPlayerData.Character.Asset, upgradeInfo, false);
			this.ItemButton.onClick.Clear();
			EventDelegate item = new EventDelegate(new EventDelegate.Callback(this.ButtonOnClick));
			this.ItemButton.onClick.Add(item);
			this.ShopItemOnClickEvent = onClickCallback;
		}

		protected void ButtonOnClick()
		{
			this.ShopItemOnClickEvent(this);
		}

		public void SetButtonState(UIButtonColor.State buttonState, bool instant)
		{
			UIButton[] components = base.GetComponents<UIButton>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].SetState(buttonState, instant);
			}
		}

		public void SetSelection(bool isSelected, bool animateButton = true)
		{
			this.ItemButton.isEnabled = !isSelected;
			if (isSelected)
			{
				this.SetButtonState(UIButtonColor.State.Normal, true);
			}
			this.SelectionGui.WidgetAlpha.alpha = 0f;
			this.SelectionGui.GlowSprite.GetComponent<NGUIWidgetAlpha>().alpha = 0f;
			this.SelectionGui.SetActive(isSelected);
			this.SelectionGui.HolderAnimation.Play(this.SelectionGui.AnimationInName);
			this.SelectionResizeUiCard(isSelected, !animateButton);
		}

		private void SetCategoryData(InstanceCategory instanceCategory)
		{
			for (int i = 0; i < this.ItemCategoryDatas.Length; i++)
			{
				HudInstancesShopItem.InstanceShopItemCategoryData instanceShopItemCategoryData = this.ItemCategoryDatas[i];
				if (instanceShopItemCategoryData.InstanceCategory == instanceCategory)
				{
					this.TitleLabel.color = instanceShopItemCategoryData.NameTextColor;
					this.BorderSprite.sprite2D = instanceShopItemCategoryData.NormalSprite;
					this.ItemButton.normalSprite2D = instanceShopItemCategoryData.NormalSprite;
					this.ItemButton.hoverSprite2D = instanceShopItemCategoryData.HoverSprite;
					this.ItemButton.pressedSprite2D = instanceShopItemCategoryData.NormalSprite;
					this.ItemButton.disabledSprite2D = instanceShopItemCategoryData.NormalSprite;
					this.RoleCategoryLabel.text = Language.Get(instanceShopItemCategoryData.DraftName, TranslationSheets.Instances);
					this.RoleCategoryLabel.color = instanceShopItemCategoryData.CategoryTextColor;
					this.SelectionGui.BorderSprite.sprite2D = instanceShopItemCategoryData.SelectionSprite;
					this.SelectionGui.GlowSprite.color = instanceShopItemCategoryData.SelectionGlowColor;
					this.BorderHoverButton.defaultColor = instanceShopItemCategoryData.BorderNormalColor;
					this.BorderHoverButton.hover = instanceShopItemCategoryData.BorderHoverColor;
					this.BorderHoverButton.pressed = instanceShopItemCategoryData.BorderPressedColor;
					this.BorderHoverButton.disabledColor = instanceShopItemCategoryData.BorderDisabledColor;
					this.BorderHoverGlowSprite.color = instanceShopItemCategoryData.BorderHoverGlowColor;
					for (int j = 0; j < this.SelectionGui.Particles.Length; j++)
					{
						this.SelectionGui.Particles[j].main.startColor = new ParticleSystem.MinMaxGradient(instanceShopItemCategoryData.SelectionParticleColor);
					}
					break;
				}
			}
		}

		private void SelectionResizeUiCard(bool isSelected, bool imediate = false)
		{
			if (!imediate)
			{
				TweenScale.Begin(base.gameObject, this.SelectedResizeDurationInSec, (!isSelected) ? this.NormalScale : Vector3.Scale(this.NormalScale, this.ResizedlScale)).method = UITweener.Method.EaseInOut;
			}
			else
			{
				base.transform.localScale = ((!isSelected) ? this.NormalScale : this.ResizedlScale);
			}
		}

		public void PlayInAnimation()
		{
			this.InAnimation.Play(this.InAnimationName);
		}

		public void PlayOutAnimation(bool isMainCard)
		{
			this.SelectionGui.MainParticleGameObject.SetActive(false);
			this.InAnimation.Play((!isMainCard) ? this.OutAnimationName : this.MainOutAnimationName);
		}

		public void DisableButton()
		{
			this.ItemButton.isEnabled = false;
			this.SetButtonState(UIButtonColor.State.Normal, true);
			base.GetComponent<NGUIWidgetAlpha>().alpha = this.CardDisabledAlpha;
		}

		public void SelectedCardOnClick()
		{
			this.SelectionGui.HolderAnimation.Play(this.SelectionGui.AnimationDeniedName);
			FMODAudioManager.PlayOneShotAt(this.SelectionGui.DeniedAudio, Vector3.zero, 0);
		}

		public float CardDisabledAlpha = 0.4f;

		public string InAnimationName;

		public string OutAnimationName;

		public string MainOutAnimationName;

		[SerializeField]
		private Animation InAnimation;

		[SerializeField]
		private UILabel TitleLabel;

		[SerializeField]
		private UILabel DescriptionSummaryLabel;

		[SerializeField]
		private UILabel DescriptionLabel;

		[SerializeField]
		private UILabel RoleCategoryLabel;

		[SerializeField]
		private HMMUI2DDynamicSprite IconSprite;

		[SerializeField]
		private UI2DSprite BorderSprite;

		[SerializeField]
		private UIButton BorderHoverButton;

		[SerializeField]
		private UI2DSprite BorderHoverGlowSprite;

		[SerializeField]
		private UIButton ItemButton;

		[SerializeField]
		private NGUIWidgetAlpha PivotWidgetAlpha;

		[Header("[Selection Resize]")]
		public float SelectedResizeDurationInSec = 1f;

		public Vector3 NormalScale = Vector3.one;

		public Vector3 ResizedlScale = Vector3.one * 2f;

		[SerializeField]
		private HudInstancesShopItem.SelectionGuiComponents SelectionGui;

		[SerializeField]
		private HudInstancesShopItem.InstanceShopItemCategoryData[] ItemCategoryDatas;

		private string _upgradeInfoName;

		[Serializable]
		private struct SelectionGuiComponents
		{
			public void SetActive(bool isActive)
			{
				this.MainGameObject.SetActive(isActive);
				this.MainParticleGameObject.SetActive(isActive);
			}

			public string AnimationInName;

			public string AnimationDeniedName;

			public GameObject MainGameObject;

			public UI2DSprite BorderSprite;

			public UI2DSprite GlowSprite;

			public NGUIWidgetAlpha WidgetAlpha;

			public Animation HolderAnimation;

			public FMODAsset DeniedAudio;

			public GameObject MainParticleGameObject;

			public ParticleSystem[] Particles;
		}

		[Serializable]
		private struct InstanceShopItemCategoryData
		{
			public InstanceCategory InstanceCategory;

			public string DraftName;

			public Color NameTextColor;

			public Color CategoryTextColor;

			public Sprite NormalSprite;

			public Sprite HoverSprite;

			public Sprite SelectionSprite;

			public Color SelectionGlowColor;

			public Color SelectionParticleColor;

			public Color BorderNormalColor;

			public Color BorderHoverColor;

			public Color BorderHoverGlowColor;

			public Color BorderPressedColor;

			public Color BorderDisabledColor;
		}

		public delegate void InstancesShopItemOnClickDelegate(HudInstancesShopItem sender);
	}
}
