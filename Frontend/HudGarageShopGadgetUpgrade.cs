using System;
using System.Diagnostics;
using System.Globalization;
using FMod;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudGarageShopGadgetUpgrade : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudGarageShopGadgetUpgrade.OnGadgetBuy ListenToGadgetBuy;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudGarageShopGadgetUpgrade.OnGadgetSell ListenToGadgetSell;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudGarageShopGadgetUpgrade.OnGadgetDenied ListenToGadgetDeniedByBlock;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudGarageShopGadgetUpgrade.OnGadgetDenied ListenToGadgetDeniedByBalance;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudGarageShopGadgetUpgrade.OnHoverOver ListenToHoverOver;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudGarageShopGadgetUpgrade.OnHoverOut ListenToHoverOut;

		public int Level
		{
			get
			{
				return (this._upgradeInstance != null) ? this._upgradeInstance.Level : 0;
			}
		}

		public void Awake()
		{
			if (!this._configured)
			{
				this._state = HudGarageShopGadgetUpgrade.GadgetUpgradeState.Blocked;
			}
		}

		public void Start()
		{
			this.HoverArrowUi2DSprite.gameObject.SetActive(false);
			this.JoystickButtonUiButton.onClick.Clear();
			this.JoystickButtonUiButton.onClick.Add(new EventDelegate(new EventDelegate.Callback(this.EventTriggerOnClick)));
			this.HoverEventTrigger.onHoverOver.Clear();
			this.HoverEventTrigger.onHoverOver.Add(new EventDelegate(new EventDelegate.Callback(this.EventTriggerOnHoverOver)));
			this.HoverEventTrigger.onHoverOut.Clear();
			this.HoverEventTrigger.onHoverOut.Add(new EventDelegate(new EventDelegate.Callback(this.EventTriggerOnHoverOut)));
			if (!this._configured)
			{
				this._price = 0;
				this._canBuy = false;
				this.SetState(HudGarageShopGadgetUpgrade.GadgetUpgradeState.Blocked, 0);
			}
		}

		private void OnDestroy()
		{
			this._controller = null;
		}

		private void EventTriggerOnClick()
		{
			this.ListenToGadgetBuy(this._upgradeInstance.Info.Name);
		}

		private void EventTriggerOnHoverOver()
		{
			this.HoverArrowUi2DSprite.gameObject.SetActive(true);
			this.ControllerUpdateIconVisibility(true);
			if (this._upgradeInstance != null && this.ListenToHoverOver != null)
			{
				this.ListenToHoverOver(this._state, this._upgradeInstance, this._canBuy, this._canBuyback);
			}
		}

		private void EventTriggerOnHoverOut()
		{
			this.HoverArrowUi2DSprite.gameObject.SetActive(false);
			this.JoystickGroupTransform.gameObject.SetActive(false);
			if (this._upgradeInstance != null)
			{
				this.ListenToHoverOut();
			}
		}

		public void Setup(GadgetBehaviour gadgetBehaviour, GadgetBehaviour.UpgradeInstance upgradeInstance, int playerBalance, bool useBigIcon, HudGarageShopController controller)
		{
			this._configured = true;
			this._price = 0;
			this._level = 0;
			this._canBuy = false;
			this._canBuyback = false;
			this.ListenToGadgetBuy = null;
			this.ListenToGadgetDeniedByBalance = null;
			this.ListenToGadgetDeniedByBlock = null;
			this.ListenToGadgetSell = null;
			this.ListenToHoverOut = null;
			this.ListenToHoverOver = null;
			this._upgradeInstance = upgradeInstance;
			this.IconUi2DSprite.SpriteName = HudUtils.GetGadgetUpgradeIconName(gadgetBehaviour.Combat.Player.Character.Asset, gadgetBehaviour.Slot, gadgetBehaviour.Info, upgradeInstance.Info, useBigIcon);
			this._price = upgradeInstance.CurrentPrice();
			this._controller = controller;
			this.SetState(HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available, playerBalance);
		}

		public void SetState(HudGarageShopGadgetUpgrade.GadgetUpgradeState state, int playerBalance)
		{
			if (this._controller && this._controller.IsWindowVisible() && this._state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available && state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Purchased && this.HudGarageShopSettings.BuyAudioFmodAsset != null)
			{
				FMODAudioManager.PlayOneShotAt(this.HudGarageShopSettings.BuyAudioFmodAsset, Vector3.zero, 0);
			}
			this._state = state;
			this._canBuy = (this._configured && playerBalance >= this._price);
			if (state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Blocked)
			{
				this.UpdateIconBlock(this._price);
				return;
			}
			if (state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available)
			{
				if (playerBalance < this._price)
				{
					this.UpdateIconCantBuy(this._price);
				}
				else
				{
					this.UpdateIconCanBuy(this._price);
				}
				return;
			}
			if (state != HudGarageShopGadgetUpgrade.GadgetUpgradeState.Purchased)
			{
				return;
			}
			if (this._canBuyback)
			{
				this.UpdateIconRevert(this._price);
			}
			else
			{
				this.UpdateIconSell(this._price);
			}
		}

		public void UpdateIconBlock(int value)
		{
			this.SetValue(value);
			this.IconUi2DSprite.gameObject.SetActive(true);
			this.IconUi2DSprite.alpha = 0.5f;
			this.IconBorderUi2DSprite.gameObject.SetActive(true);
			this.IconFullUi2DSprite.gameObject.SetActive(false);
			this.IconBlockUi2DSprite.gameObject.SetActive(false);
			this.IconPurchasedUi2DSprite.gameObject.SetActive(false);
			this.SellUi2DSprite.gameObject.SetActive(false);
			this.RevertUi2DSprite.gameObject.SetActive(false);
			this.BalanceGroupPanel.gameObject.SetActive(true);
			this.BalanceGroupPanel.alpha = 0.5f;
			this.BalanceLabel.color = this.BalanceLabelNormalColor;
			this.ControllerUpdateIconVisibility(false);
		}

		public void UpdateIconCanBuy(int value)
		{
			bool activeSelf = this.HoverArrowUi2DSprite.gameObject.activeSelf;
			this.SetValue(value);
			this.IconUi2DSprite.gameObject.SetActive(true);
			this.IconUi2DSprite.alpha = 1f;
			this.IconBorderUi2DSprite.gameObject.SetActive(true);
			this.IconFullUi2DSprite.gameObject.SetActive(false);
			this.IconBlockUi2DSprite.gameObject.SetActive(false);
			this.IconPurchasedUi2DSprite.gameObject.SetActive(false);
			this.SellUi2DSprite.gameObject.SetActive(false);
			this.RevertUi2DSprite.gameObject.SetActive(false);
			this.BalanceGroupPanel.gameObject.SetActive(true);
			this.BalanceGroupPanel.alpha = 1f;
			this.BalanceLabel.color = this.BalanceLabelNormalColor;
			this.JoystickIconUi2DSprite.gameObject.SetActive(true);
			this.BalanceLabel.gameObject.SetActive(true);
			this.ControllerUpdateIconVisibility(activeSelf);
		}

		public void UpdateIconCantBuy(int value)
		{
			this.SetValue(value);
			this.IconUi2DSprite.gameObject.SetActive(true);
			this.IconUi2DSprite.alpha = 0.5f;
			this.IconBorderUi2DSprite.gameObject.SetActive(false);
			this.IconFullUi2DSprite.gameObject.SetActive(true);
			this.IconBlockUi2DSprite.gameObject.SetActive(false);
			this.IconPurchasedUi2DSprite.gameObject.SetActive(false);
			this.SellUi2DSprite.gameObject.SetActive(false);
			this.RevertUi2DSprite.gameObject.SetActive(false);
			this.BalanceGroupPanel.gameObject.SetActive(true);
			this.BalanceGroupPanel.alpha = 0.5f;
			this.BalanceLabel.color = this.BalanceLabelNormalColor;
			this.ControllerUpdateIconVisibility(false);
		}

		public void UpdateIconSell(int value)
		{
			this.SetValue(Mathf.RoundToInt((float)value * this.HudGarageShopSettings.NonBuybackRefundModifier));
			bool activeSelf = this.HoverArrowUi2DSprite.gameObject.activeSelf;
			this.IconUi2DSprite.gameObject.SetActive(true);
			this.IconUi2DSprite.alpha = 1f;
			this.IconBorderUi2DSprite.gameObject.SetActive(false);
			this.IconFullUi2DSprite.gameObject.SetActive(false);
			this.IconBlockUi2DSprite.gameObject.SetActive(false);
			this.IconPurchasedUi2DSprite.gameObject.SetActive(true);
			this.SellUi2DSprite.gameObject.SetActive(true);
			this.RevertUi2DSprite.gameObject.SetActive(false);
			this.BalanceGroupPanel.gameObject.SetActive(true);
			this.BalanceGroupPanel.alpha = 1f;
			this.BalanceLabel.color = this.BalanceLabelSellColor;
			this.JoystickIconUi2DSprite.gameObject.SetActive(true);
			this.ControllerUpdateIconVisibility(activeSelf);
		}

		public void UpdateIconRevert(int value)
		{
			this.SetValue(value);
			bool activeSelf = this.HoverArrowUi2DSprite.gameObject.activeSelf;
			this.IconUi2DSprite.gameObject.SetActive(true);
			this.IconUi2DSprite.alpha = 1f;
			this.IconBorderUi2DSprite.gameObject.SetActive(false);
			this.IconFullUi2DSprite.gameObject.SetActive(false);
			this.IconBlockUi2DSprite.gameObject.SetActive(false);
			this.IconPurchasedUi2DSprite.gameObject.SetActive(true);
			this.SellUi2DSprite.gameObject.SetActive(false);
			this.RevertUi2DSprite.gameObject.SetActive(true);
			this.BalanceGroupPanel.gameObject.SetActive(true);
			this.BalanceGroupPanel.alpha = 1f;
			this.BalanceLabel.color = this.BalanceLabelSellColor;
			this.JoystickIconUi2DSprite.gameObject.SetActive(true);
			this.ControllerUpdateIconVisibility(activeSelf);
		}

		public void SetValue(int value)
		{
			string text = value.ToString(CultureInfo.InvariantCulture);
			this.BalanceLabel.text = text;
		}

		public void BalanceUpdated(int playerBalance)
		{
			this._canBuy = (playerBalance >= this._price);
			if (this._state != HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available)
			{
				return;
			}
			if (playerBalance < this._price)
			{
				this.UpdateIconCantBuy(this._price);
			}
			else
			{
				this.UpdateIconCanBuy(this._price);
			}
		}

		public void GadgetUpgraded(string upgradeName, int level)
		{
			if (!this._configured)
			{
				return;
			}
			if (this._upgradeInstance.Info.Name != upgradeName)
			{
				return;
			}
			if (level == this._level)
			{
				return;
			}
			if (level > this._level)
			{
				this.GadgetUpgraded();
			}
			else
			{
				this.GadgetDowngraded();
			}
			this._level = level;
			this.EventTriggerOnHoverOver();
		}

		private void GadgetUpgraded()
		{
			if (this._state != HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available)
			{
				return;
			}
			this.SetState(HudGarageShopGadgetUpgrade.GadgetUpgradeState.Purchased, 0);
		}

		private void GadgetDowngraded()
		{
			if (this._state != HudGarageShopGadgetUpgrade.GadgetUpgradeState.Purchased)
			{
				return;
			}
			this.SetState(HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available, this._price);
		}

		public void GadgetShopOpen()
		{
			if (this._state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Purchased)
			{
				this.UpdateIconSell(this._price);
			}
			if (UICamera.selectedObject != base.gameObject)
			{
				this.HoverArrowUi2DSprite.gameObject.SetActive(false);
			}
		}

		public void GadgetShopClose()
		{
			this._canBuyback = false;
		}

		public void SetBLockedIfAvailable()
		{
			if (this._state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available)
			{
				this.SetState(HudGarageShopGadgetUpgrade.GadgetUpgradeState.Blocked, 0);
			}
		}

		public void SetAvailableIfBlocked(int playerBalance)
		{
			if (this._upgradeInstance == null)
			{
				return;
			}
			if (this._state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Blocked)
			{
				this.SetState(HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available, playerBalance);
			}
		}

		public bool IsAvailable()
		{
			return this._state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available;
		}

		public int GetPrice()
		{
			return this._price;
		}

		private void ControllerUpdateIconVisibility(bool isHover)
		{
			if (!this._configured)
			{
				this.JoystickGroupTransform.gameObject.SetActive(false);
				return;
			}
			bool flag = (this._state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Available && this._canBuy) || this._state == HudGarageShopGadgetUpgrade.GadgetUpgradeState.Purchased;
			bool flag2 = GameHubBehaviour.Hub.Input.CurrentController.Inputs.IsControllerActive();
			this.JoystickGroupTransform.gameObject.SetActive(flag2 && isHover && flag);
		}

		public HudGarageShopSettings HudGarageShopSettings;

		[Header("[Icon Group]")]
		public HMMUI2DDynamicSprite IconUi2DSprite;

		public UI2DSprite IconBorderUi2DSprite;

		public UI2DSprite IconFullUi2DSprite;

		public UI2DSprite IconBlockUi2DSprite;

		public UI2DSprite IconPurchasedUi2DSprite;

		[Header("[Revert]")]
		public UI2DSprite RevertUi2DSprite;

		[Header("[Sell]")]
		public UI2DSprite SellUi2DSprite;

		[Header("[Hover Arrow]")]
		public UI2DSprite HoverArrowUi2DSprite;

		public UIEventTrigger HoverEventTrigger;

		[Header("[Balance Group]")]
		public UIPanel BalanceGroupPanel;

		public Color BalanceLabelNormalColor = Color.white;

		public Color BalanceLabelSellColor = new Color(0f, 1f, 0.5882353f, 1f);

		public UILabel BalanceLabel;

		[Header("[Joystick Group]")]
		public Transform JoystickGroupTransform;

		public UI2DSprite JoystickIconUi2DSprite;

		public UIButton JoystickButtonUiButton;

		private bool _canBuy;

		private bool _canBuyback;

		private int _price;

		private int _level;

		private GadgetBehaviour.UpgradeInstance _upgradeInstance;

		private HudGarageShopGadgetUpgrade.GadgetUpgradeState _state;

		private bool _configured;

		private HudGarageShopController _controller;

		public enum GadgetUpgradeState
		{
			Blocked,
			Available,
			Purchased
		}

		public delegate void OnGadgetBuy(string upgradeName);

		public delegate void OnGadgetSell(string upgradeName, bool isBuyback);

		public delegate void OnGadgetDenied();

		public delegate void OnHoverOver(HudGarageShopGadgetUpgrade.GadgetUpgradeState state, GadgetBehaviour.UpgradeInstance upgradeInstance, bool canBuy, bool canBuyback);

		public delegate void OnHoverOut();
	}
}
