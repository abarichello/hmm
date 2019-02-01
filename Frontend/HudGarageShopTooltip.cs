using System;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudGarageShopTooltip : GameHubBehaviour
	{
		public void Awake()
		{
			this.Hide();
		}

		public void Hide()
		{
			this.TooltipGroupGameObject.SetActive(false);
		}

		public void Show(TooltipInfo tooltipInfo, bool isUpgrade)
		{
			this.PopulateTooltip(tooltipInfo, isUpgrade);
			this.TooltipGroupGameObject.SetActive(true);
		}

		private void PopulateTooltip(TooltipInfo tooltipInfo, bool isUpgrade)
		{
			this.IconSpriteUpgrade.gameObject.SetActive(isUpgrade);
			this.BorderSpriteUpgrade.gameObject.SetActive(isUpgrade);
			this.IconSpriteGadget.gameObject.SetActive(!isUpgrade);
			this.BorderSpriteGadGet.gameObject.SetActive(!isUpgrade);
			HMMUI2DDynamicSprite hmmui2DDynamicSprite = (!isUpgrade) ? this.IconSpriteGadget : this.IconSpriteUpgrade;
			if (string.IsNullOrEmpty(tooltipInfo.SpriteName))
			{
				hmmui2DDynamicSprite.sprite2D = tooltipInfo.IconSprite;
			}
			else
			{
				hmmui2DDynamicSprite.SpriteName = tooltipInfo.SpriteName;
			}
			this.NameLabel.text = tooltipInfo.Title;
			this.HintGroupGameObject.SetActive(false);
			this.CooldownGroupGameObject.SetActive(!string.IsNullOrEmpty(tooltipInfo.CooldownDescription));
			this.CooldownLabel.text = tooltipInfo.CooldownDescription;
			this.DescriptionLabel.text = tooltipInfo.DescriptionFull;
			this.ValueLabel.text = tooltipInfo.Price;
			this.ValueLabel.color = Color.white;
			this.BalanceGroupGameObject.SetActive(!string.IsNullOrEmpty(tooltipInfo.Price));
			this.IconBuybackSellSprite.gameObject.SetActive(false);
			Vector3 localPosition = this.IconBuybackSellSprite.transform.localPosition;
			localPosition.x = (float)this.BuybackSellSpriteHideX;
			this.IconBuybackSellSprite.transform.localPosition = localPosition;
			this.BuySellLabel.gameObject.SetActive(false);
			this.IconMetalBuySprite.gameObject.SetActive(false);
			this.IconMetalSellSprite.gameObject.SetActive(false);
			switch (tooltipInfo.Type)
			{
			case TooltipInfo.TooltipType.Normal:
				this.BuySellLabel.gameObject.SetActive(true);
				this.BuySellLabel.text = Language.Get(this.BuyTranslationKey, this.BuySellTranslationSheet) + " ";
				this.IconMetalBuySprite.gameObject.SetActive(true);
				break;
			case TooltipInfo.TooltipType.GadgetShopSell:
				this.IconBuybackSellSprite.gameObject.SetActive(true);
				this.IconBuybackSellSprite.sprite2D = this.SellSprite;
				localPosition.x = (float)this.BuybackSellSpriteVisibleX;
				this.IconBuybackSellSprite.transform.localPosition = localPosition;
				this.BuySellLabel.gameObject.SetActive(true);
				this.BuySellLabel.text = Language.Get(this.SellTranslationKey, this.BuySellTranslationSheet) + " ";
				this.IconMetalSellSprite.gameObject.SetActive(true);
				this.ValueLabel.color = this.SellRevertColor;
				break;
			case TooltipInfo.TooltipType.GadgetShopRevert:
				this.IconBuybackSellSprite.gameObject.SetActive(true);
				this.IconBuybackSellSprite.sprite2D = this.BuybackSprite;
				localPosition.x = (float)this.BuybackSellSpriteVisibleX;
				this.IconBuybackSellSprite.transform.localPosition = localPosition;
				this.BuySellLabel.gameObject.SetActive(true);
				this.BuySellLabel.text = Language.Get(this.BuybackTranslationKey, this.BuySellTranslationSheet) + " ";
				this.IconMetalSellSprite.gameObject.SetActive(true);
				this.ValueLabel.color = this.SellRevertColor;
				break;
			case TooltipInfo.TooltipType.GadgetShopNoFunds:
				this.BuySellLabel.gameObject.SetActive(true);
				this.BuySellLabel.text = Language.Get(this.BuyTranslationKey, this.BuySellTranslationSheet) + " ";
				this.IconMetalBuySprite.gameObject.SetActive(true);
				break;
			}
		}

		public GameObject TooltipGroupGameObject;

		public HMMUI2DDynamicSprite IconSpriteUpgrade;

		public HMMUI2DDynamicSprite IconSpriteGadget;

		public UILabel NameLabel;

		[Header("[Borders]")]
		public UI2DSprite BorderSpriteUpgrade;

		public UI2DSprite BorderSpriteGadGet;

		[Header("[Hint]")]
		public GameObject HintGroupGameObject;

		public UILabel HintLabel;

		[Header("[Description]")]
		public GameObject CooldownGroupGameObject;

		public UILabel CooldownLabel;

		public UILabel DescriptionLabel;

		[Header("[Balance]")]
		public GameObject BalanceGroupGameObject;

		public UI2DSprite IconBuybackSellSprite;

		public UILabel BuySellLabel;

		public UI2DSprite IconMetalBuySprite;

		public UI2DSprite IconMetalSellSprite;

		public UILabel ValueLabel;

		[Header("[Buy/Sell Label Translation]")]
		public TranslationSheets BuySellTranslationSheet;

		public string BuyTranslationKey;

		public string SellTranslationKey;

		public string BuybackTranslationKey;

		[Header("[Buyback/Sell Sprites]")]
		public Sprite BuybackSprite;

		public Sprite SellSprite;

		[Header("[Color]")]
		public Color SellRevertColor;

		[Header("[Buyback/Sell anchor hack]")]
		public int BuybackSellSpriteVisibleX;

		public int BuybackSellSpriteHideX;
	}
}
