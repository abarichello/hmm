using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudGarageShopGadgetObject : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudGarageShopGadgetObject.OnBuyUpgradeDelegate OnBuyUpgradeEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudGarageShopGadgetObject.OnSellUpgradeDelegate OnSellUpgradeEvent;

		public virtual void Setup(GadgetBehaviour customGadget, int playerBalance, HudGarageShopGadgetObject.OnBuyUpgradeDelegate onBuyUpgradeEvent, HudGarageShopGadgetObject.OnSellUpgradeDelegate onSellUpgradeEvent, HudGarageShopController controller)
		{
			this._gadgetBehaviour = customGadget;
			this.Slot = customGadget.Slot;
			this._name = customGadget.Info.LocalizedName;
			this._description = customGadget.Info.LocalizedDescription;
			this._cooldownInfoText = string.Empty;
			if (customGadget.Info.ShowTooltipCooldownInfo)
			{
				float cooldown = customGadget.Cooldown;
				if (cooldown > 0f)
				{
					this._cooldownInfoText = string.Format(Language.Get("HUD_GARAGE_SHOP_RECHARGE", TranslationSheets.Hud), cooldown);
				}
			}
			this._descriptionSummaryType = TooltipInfo.DescriptionSummaryType.None;
			if (customGadget.IsPassiveGadget)
			{
				this._descriptionSummaryType = TooltipInfo.DescriptionSummaryType.Passive;
			}
			this.OnBuyUpgradeEvent += onBuyUpgradeEvent;
			this.OnSellUpgradeEvent += onSellUpgradeEvent;
			if (this.GadgetSkillGroupTransform.childCount == 0)
			{
				this.GadgetSkill = UnityEngine.Object.Instantiate<HudGarageShopGadgetObjectSkill>(this.GadgetSkillReference, Vector3.zero, Quaternion.identity);
				this.GadgetSkill.transform.parent = this.GadgetSkillGroupTransform;
				this.GadgetSkill.transform.localPosition = Vector3.zero;
				this.GadgetSkill.transform.localScale = Vector3.one;
			}
			else
			{
				this.GadgetSkill = this.GadgetSkillGroupTransform.GetChild(0).GetComponent<HudGarageShopGadgetObjectSkill>();
			}
			this.GadgetSkill.IconArrowSprite.gameObject.SetActive(false);
			this.GadgetSkill.IconHoverEventTrigger.onHoverOver.Clear();
			this.GadgetSkill.IconHoverEventTrigger.onHoverOver.Add(new EventDelegate(new EventDelegate.Callback(this.SkillIconEventTriggerOnHoverOver)));
			this.GadgetSkill.IconHoverEventTrigger.onHoverOut.Clear();
			this.GadgetSkill.IconHoverEventTrigger.onHoverOut.Add(new EventDelegate(new EventDelegate.Callback(this.SkillIconEventTriggerOnHoverOut)));
			this.GadgetSkill.IconSprite.SpriteName = HudUtils.GetGadgetIconName(GameHubBehaviour.Hub.Players.CurrentPlayerData.Character, customGadget.Slot);
			this.GadgetUpgrades = new List<HudGarageShopGadgetUpgrade>(customGadget.Upgrades.Length);
			int childCount = this.GadgetGrid.transform.childCount;
			for (int i = 0; i < customGadget.Upgrades.Length; i++)
			{
				GadgetBehaviour.UpgradeInstance upgradeInstance = customGadget.Upgrades[i];
				if (upgradeInstance.Available)
				{
					HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade;
					if (i < childCount)
					{
						hudGarageShopGadgetUpgrade = this.GadgetUpgradeReference;
					}
					else
					{
						hudGarageShopGadgetUpgrade = UnityEngine.Object.Instantiate<HudGarageShopGadgetUpgrade>(this.GadgetUpgradeReference, Vector3.zero, Quaternion.identity);
					}
					hudGarageShopGadgetUpgrade.gameObject.SetActive(true);
					this.GadgetGrid.AddChild(hudGarageShopGadgetUpgrade.transform);
					hudGarageShopGadgetUpgrade.transform.localScale = Vector3.one;
					hudGarageShopGadgetUpgrade.transform.localPosition = Vector3.zero;
					hudGarageShopGadgetUpgrade.Setup(this._gadgetBehaviour, upgradeInstance, playerBalance, this.LoadBigUpgradeIcon, controller);
					hudGarageShopGadgetUpgrade.ListenToGadgetBuy += this.EventOnGadgetBuy;
					hudGarageShopGadgetUpgrade.ListenToGadgetSell += this.EventOnGadgetSell;
					hudGarageShopGadgetUpgrade.ListenToHoverOver += this.EventOnHoverOver;
					hudGarageShopGadgetUpgrade.ListenToHoverOut += this.EventOnHoverOut;
					this.GadgetUpgrades.Add(hudGarageShopGadgetUpgrade);
					if (this.SingleUpgrade)
					{
						break;
					}
				}
			}
			this.GadgetGrid.Reposition();
			this._gadgetCount = 0;
			this._gadgetCountMax = this.GetMaxCount(this.Slot);
			this.UpdateGadgetCountLabel();
		}

		private void EventOnHoverOver(HudGarageShopGadgetUpgrade.GadgetUpgradeState state, GadgetBehaviour.UpgradeInstance upgradeInstance, bool canBuy, bool canBuyback)
		{
			if (this._tooltipEnabled)
			{
				TooltipInfo.TooltipType type = TooltipInfo.TooltipType.Normal;
				string text = string.Empty;
				int num = upgradeInstance.CurrentPrice();
				if (state != HudGarageShopGadgetUpgrade.GadgetUpgradeState.Purchased)
				{
					if (state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available)
					{
						if (!canBuy)
						{
							type = TooltipInfo.TooltipType.GadgetShopNoFunds;
							text = "HUD_GARAGE_SHOP_TOOLTIP_HINT_NO_FUNDS";
						}
					}
				}
				else
				{
					type = ((!canBuyback) ? TooltipInfo.TooltipType.GadgetShopSell : TooltipInfo.TooltipType.GadgetShopRevert);
					text = ((!canBuyback) ? "HUD_GARAGE_SHOP_TOOLTIP_HINT_SELL" : "HUD_GARAGE_SHOP_TOOLTIP_HINT_REVERT");
					if (!canBuyback)
					{
						num = Mathf.RoundToInt((float)num * this.HudGarageShopSettings.NonBuybackRefundModifier);
					}
				}
				TooltipInfo.DescriptionSummaryType summaryType = TooltipInfo.DescriptionSummaryType.None;
				Vector3 position = base.transform.position;
				position.x *= -1.5f;
				position.y *= 4f;
				string gadgetUpgradeIconName = HudUtils.GetGadgetUpgradeIconName(this._gadgetBehaviour.Combat.Player.Character.Asset, this.Slot, this._gadgetBehaviour.Info, upgradeInstance.Info, false);
				TooltipInfo tooltipInfo = new TooltipInfo(type, summaryType, PreferredDirection.None, null, gadgetUpgradeIconName, upgradeInstance.Info.LocalizedName, string.Empty, HudGarageShopGadgetObject.GetUpgradeDescription(this._gadgetBehaviour.Info, upgradeInstance.Info.LocalizedDescription, upgradeInstance.Info.Name), string.Empty, num.ToString(), (!string.IsNullOrEmpty(text)) ? Language.Get(text, TranslationSheets.Hud) : string.Empty, position, string.Empty);
				this.HudGarageShopTooltip.Show(tooltipInfo, true);
			}
		}

		private void EventOnHoverOut()
		{
			this.HudGarageShopTooltip.Hide();
		}

		private void EventOnGadgetBuy(string upgradename)
		{
			this.OnBuyUpgradeEvent(this.Slot, upgradename);
		}

		private void EventOnGadgetSell(string upgradename, bool isBuyback)
		{
			this.OnSellUpgradeEvent(this.Slot, upgradename, isBuyback);
		}

		public void BalanceUpdated(int playerBalance)
		{
			this._playerBalanceCache = playerBalance;
			for (int i = 0; i < this.GadgetUpgrades.Count; i++)
			{
				this.GadgetUpgrades[i].BalanceUpdated(playerBalance);
			}
		}

		public bool GadgetUpgraded(string upgradeName, int level)
		{
			this._gadgetCount = 0;
			for (int i = 0; i < this.GadgetUpgrades.Count; i++)
			{
				this.GadgetUpgrades[i].GadgetUpgraded(upgradeName, level);
				if (this.GadgetUpgrades[i].Level > 0)
				{
					this._gadgetCount++;
				}
			}
			if (this._gadgetCountMax == 0)
			{
				return false;
			}
			this.UpdateGadgetCountLabel();
			if (this.IsGadgetCountMaxed())
			{
				this.GadgetSkill.UpgradeLabel.color = this.HudGarageShopSettings.GadgetTitleLabelFullColor;
				for (int j = 0; j < this.GadgetUpgrades.Count; j++)
				{
					this.GadgetUpgrades[j].SetBLockedIfAvailable();
				}
				return true;
			}
			this.GadgetSkill.UpgradeLabel.color = this.HudGarageShopSettings.GadgetTitleLabelNormalColor;
			for (int k = 0; k < this.GadgetUpgrades.Count; k++)
			{
				this.GadgetUpgrades[k].SetAvailableIfBlocked(this._playerBalanceCache);
			}
			return false;
		}

		private void SkillIconEventTriggerOnHoverOver()
		{
			this.GadgetSkill.IconArrowSprite.gameObject.SetActive(true);
			if (this._tooltipEnabled)
			{
				Vector3 position = base.transform.position;
				position.x *= -1.5f;
				position.y *= 4f;
				TooltipInfo tooltipInfo = new TooltipInfo(TooltipInfo.TooltipType.Normal, this._descriptionSummaryType, PreferredDirection.None, this.GadgetSkill.IconSprite.sprite2D, string.Empty, this._name, this._cooldownInfoText, HudGarageShopGadgetObject.GetUpgradeDescription(this._gadgetBehaviour.Info, this._description, "MainGadget"), this._gadgetBehaviour.Info.LocalizedCooldownDescription, string.Empty, string.Empty, position, string.Empty);
				this.HudGarageShopTooltip.Show(tooltipInfo, false);
			}
		}

		private void SkillIconEventTriggerOnHoverOut()
		{
			this.GadgetSkill.IconArrowSprite.gameObject.SetActive(false);
			this.HudGarageShopTooltip.Hide();
		}

		public void GadgetShopOpen()
		{
			for (int i = 0; i < this.GadgetUpgrades.Count; i++)
			{
				this.GadgetUpgrades[i].GadgetShopOpen();
			}
		}

		public void GadgetShopClose()
		{
			for (int i = 0; i < this.GadgetUpgrades.Count; i++)
			{
				this.GadgetUpgrades[i].GadgetShopClose();
			}
			this.HudGarageShopTooltip.Hide();
		}

		private int GetMaxCount(GadgetSlot slot)
		{
			int result;
			this.HudGarageShopSettings.GetGadgetMaxQuantity(slot, out result);
			return result;
		}

		private void UpdateGadgetCountLabel()
		{
			this.GadgetSkill.UpgradeLabel.text = this._gadgetCount + "/" + this._gadgetCountMax;
		}

		private void GadgetCountInc()
		{
			this._gadgetCount++;
			if (this._gadgetCount > this._gadgetCountMax)
			{
				this._gadgetCount = this._gadgetCountMax;
			}
		}

		private void GadgetCountDec()
		{
			this._gadgetCount--;
			if (this._gadgetCount < 0)
			{
				this._gadgetCount = 0;
			}
		}

		private bool IsGadgetCountMaxed()
		{
			return this._gadgetCount >= this._gadgetCountMax;
		}

		public bool GetMinorAvailableGadgetPrice(out int minorAvailableGadgetPrice)
		{
			minorAvailableGadgetPrice = -1;
			for (int i = 0; i < this.GadgetUpgrades.Count; i++)
			{
				HudGarageShopGadgetUpgrade hudGarageShopGadgetUpgrade = this.GadgetUpgrades[i];
				if (hudGarageShopGadgetUpgrade.IsAvailable())
				{
					int price = hudGarageShopGadgetUpgrade.GetPrice();
					if (minorAvailableGadgetPrice == -1 || price < minorAvailableGadgetPrice)
					{
						minorAvailableGadgetPrice = price;
					}
				}
			}
			return minorAvailableGadgetPrice != -1;
		}

		public void SetTooltipEnable(bool enable)
		{
			this._tooltipEnabled = enable;
		}

		public static string GetUpgradeDescription(GadgetInfo gadgetInfo, string description, string upgradeName)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (stringBuilder.Length == 0)
			{
				return description;
			}
			string[] args = stringBuilder.ToString().Split(new char[]
			{
				'|'
			});
			return string.Format(description, args);
		}

		private static string FormatStatString(float statValue, GadgetStatKind kind)
		{
			StringBuilder stringBuilder = new StringBuilder();
			switch (kind)
			{
			case GadgetStatKind.FloatValue:
				stringBuilder.Append(statValue.ToString("0.##"));
				break;
			case GadgetStatKind.IntValue:
				stringBuilder.Append(Math.Round((double)statValue).ToString());
				break;
			case GadgetStatKind.Percentage:
				stringBuilder.Append((statValue * 100f).ToString("0.###"));
				stringBuilder.Append('%');
				break;
			case GadgetStatKind.Boolean:
				stringBuilder.Append(Language.Get((statValue != 0f) ? "GADGET_STATS_YES" : "GADGET_STATS_NO", TranslationSheets.CharactersMatchInfo));
				break;
			}
			return stringBuilder.ToString();
		}

		public void OnDestroy()
		{
			this._gadgetBehaviour = null;
		}

		public HudGarageShopSettings HudGarageShopSettings;

		private const string TooltipInfoColorHex = "[F9CB9C]";

		[Header("[Gadget Skill]")]
		public HudGarageShopGadgetObjectSkill GadgetSkillReference;

		public Transform GadgetSkillGroupTransform;

		[HideInInspector]
		public HudGarageShopGadgetObjectSkill GadgetSkill;

		[Header("[Upgrade]")]
		public HudGarageShopGadgetUpgrade GadgetUpgradeReference;

		public UIGrid GadgetGrid;

		[HideInInspector]
		public List<HudGarageShopGadgetUpgrade> GadgetUpgrades;

		[Header("[Tooltip]")]
		public HudGarageShopTooltip HudGarageShopTooltip;

		[Header("[Config]")]
		public bool SingleUpgrade;

		[Header("[Icon]")]
		public bool LoadBigUpgradeIcon;

		private GadgetBehaviour _gadgetBehaviour;

		[HideInInspector]
		public GadgetSlot Slot;

		private int _gadgetCountMax;

		private int _gadgetCount;

		private int _playerBalanceCache;

		private string _name;

		private string _description;

		private bool _tooltipEnabled;

		private TooltipInfo.DescriptionSummaryType _descriptionSummaryType;

		private string _cooldownInfoText;

		public delegate void OnBuyUpgradeDelegate(GadgetSlot customGadgetSlot, string upgradeName);

		public delegate void OnSellUpgradeDelegate(GadgetSlot customGadgetSlot, string upgradeName, bool isBuyback);
	}
}
