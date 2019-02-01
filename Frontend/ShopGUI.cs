using System;
using System.Collections;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopGUI : GameHubBehaviour
	{
		protected void Awake()
		{
			this.rootGameObject.SetActive(false);
			this._currentTab = ShopGUI.Tab.Drivers;
		}

		public void CleanUp()
		{
			this.DriversGUI.CleanUp();
			this.SkinsGUI.CleanUp();
			this.BoosterGUI.CleanUp();
			this.CashGUI.CleanUp();
			this.EffectsGUI.CleanUp();
			this.SpraysGUI.CleanUp();
		}

		private static MainMenuGui MainMenuGui
		{
			get
			{
				return GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			}
		}

		private void UntoggleMenu()
		{
			this._driversToggle.Set(false, false);
			this._skinsToggle.Set(false, false);
			this._boosterToggle.Set(false, false);
			this._cashToggle.Set(false, false);
			this._effectsToggle.Set(false, false);
			this._spraysToggle.Set(false, false);
		}

		public void AnimateShow()
		{
			this.AnimateShow(ShopGUI.Tab.Drivers, Guid.Empty);
		}

		public void AnimateShow(ShopGUI.Tab tab, Guid itemTypeId)
		{
			base.StopAllCoroutines();
			this._objectToBeDisabledAfterAnimationFinishes.SetActive(false);
			this._inputBlocker.SetActive(false);
			this.ResetCurrentPages();
			if (!this.rootGameObject.activeSelf)
			{
				this.rootGameObject.SetActive(true);
			}
			this._currentTab = tab;
			this.OpenScreen(this._currentTab);
			this.TheRealAnimateShow();
			if (itemTypeId == Guid.Empty)
			{
				return;
			}
			if (tab == ShopGUI.Tab.Drivers)
			{
				if (this.Details.TryToShowSkinDetails(itemTypeId))
				{
					this.DisableTabs();
					this.AnimateHide();
				}
				return;
			}
			ItemTypeScriptableObject itemTypeScriptableObject;
			if (GameHubBehaviour.Hub.InventoryColletion.AllItemTypes.TryGetValue(itemTypeId, out itemTypeScriptableObject) && itemTypeScriptableObject.IsItemEnableInShop())
			{
				this.ShowItemTypeDetails(itemTypeScriptableObject);
			}
		}

		private void TheRealAnimateShow()
		{
			this._objectToBeDisabledAfterAnimationFinishes.SetActive(true);
			this._animation.Play("ShopIn");
			this._toggleBlockerCollider.enabled = false;
			this.Visible = true;
		}

		private void ShowReturnToShopFromTab()
		{
			this._toggleBlockerCollider.enabled = false;
			this.Visible = true;
		}

		public void AnimateShowAndOpenCash()
		{
			if (!this.rootGameObject.activeSelf)
			{
				this.rootGameObject.SetActive(true);
			}
			this.TheRealAnimateShow();
			this._currentTab = ShopGUI.Tab.Hoplons;
			this.OpenScreen(this._currentTab);
		}

		public void AnimateReturnfromSkinDetails()
		{
			this.GoBackToTab(ShopGUI.Tab.Skins);
		}

		public void AnimateReturnToCash()
		{
			this._animation.Play("DetailsOut");
			this.GoBackToTab(ShopGUI.Tab.Hoplons);
		}

		public void AnimateReturnFromTab(ShopGUI.Tab tab)
		{
			this.GoBackToTab(tab);
		}

		public void AnimateReturn()
		{
			this._animation.Play("DetailsOut");
			this.GoBackToTab(this._currentTab);
		}

		private void GoBackToTab(ShopGUI.Tab tab)
		{
			if (!this.rootGameObject.activeSelf)
			{
				this.rootGameObject.SetActive(true);
			}
			this.ShowReturnToShopFromTab();
			this._currentTab = tab;
			this.OpenScreen(this._currentTab);
		}

		public void AnimateHide()
		{
			this._bgWidgetAlpha.alpha = 1f;
			this._animation.Play("DetailsIn");
			this.Visible = false;
			this._toggleBlockerCollider.enabled = true;
		}

		private IEnumerator WaitAndDisable(float seconds)
		{
			yield return new WaitForSecondsRealtime(seconds);
			this._objectToBeDisabledAfterAnimationFinishes.SetActive(false);
			this._inputBlocker.SetActive(false);
			yield break;
		}

		public void ReturnToMainMenu()
		{
			this._inputBlocker.SetActive(true);
			if (this.Details.IsVisible())
			{
				this.Details.Hide();
			}
			this.HideToReturnMainMenu();
			ShopGUI.MainMenuGui.AnimateReturnToLobby(false, false);
			this.DisableTabs();
		}

		private void HideToReturnMainMenu()
		{
			this._animation.Play("ShopOut");
			this.Visible = false;
			base.StartCoroutine(this.WaitAndDisable(this._animation.clip.length));
		}

		public void SwitchToHoplonsScreen()
		{
			if (!this._cashToggle.value)
			{
				this.UntoggleMenu();
				this._cashToggle.Set(true, false);
			}
			this.SwitchScreen(ShopGUI.Tab.Hoplons);
		}

		public void SwitchToDriversScreen()
		{
			this.SwitchScreen(ShopGUI.Tab.Drivers);
		}

		public void SwitchToSkinsScreen()
		{
			this.SwitchScreen(ShopGUI.Tab.Skins);
		}

		public void SwitchToBoostersScreen()
		{
			this.SwitchScreen(ShopGUI.Tab.Boosters);
		}

		public void SwitchToEffectsScreen()
		{
			this.SwitchScreen(ShopGUI.Tab.Effects);
		}

		public void SwitchToSpraysScreen()
		{
			this.SwitchScreen(ShopGUI.Tab.Sprays);
		}

		public void SwitchScreen(ShopGUI.Tab tab)
		{
			if (this._currentTab == tab)
			{
				return;
			}
			this.DisableTabs();
			this._currentTab = tab;
			switch (tab)
			{
			case ShopGUI.Tab.Hoplons:
				this.CashGUI.Show();
				break;
			case ShopGUI.Tab.Drivers:
				this.DriversGUI.Show();
				break;
			case ShopGUI.Tab.Skins:
				this.SkinsGUI.Show();
				break;
			case ShopGUI.Tab.Boosters:
				this.BoosterGUI.Show();
				break;
			case ShopGUI.Tab.Effects:
				this.EffectsGUI.Show();
				break;
			case ShopGUI.Tab.Sprays:
				this.SpraysGUI.Show();
				break;
			}
		}

		public void OpenScreen(ShopGUI.Tab tab)
		{
			this.UntoggleMenu();
			switch (tab)
			{
			case ShopGUI.Tab.Hoplons:
				this._currentTab = ShopGUI.Tab.Hoplons;
				this.CashGUI.Show();
				this._cashToggle.Set(true, false);
				break;
			case ShopGUI.Tab.Drivers:
				this._currentTab = ShopGUI.Tab.Drivers;
				this.DriversGUI.Show();
				this._driversToggle.Set(true, false);
				break;
			case ShopGUI.Tab.Skins:
				this._currentTab = ShopGUI.Tab.Skins;
				this.SkinsGUI.Show();
				this._skinsToggle.Set(true, false);
				break;
			case ShopGUI.Tab.Boosters:
				this._currentTab = ShopGUI.Tab.Boosters;
				this.BoosterGUI.Show();
				this._boosterToggle.Set(true, false);
				break;
			case ShopGUI.Tab.Effects:
				this._currentTab = ShopGUI.Tab.Effects;
				this.EffectsGUI.Show();
				this._effectsToggle.Set(true, false);
				break;
			case ShopGUI.Tab.Sprays:
				this._currentTab = ShopGUI.Tab.Sprays;
				this.SpraysGUI.Show();
				this._spraysToggle.Set(true, false);
				break;
			}
		}

		public void DisableTabs()
		{
			this.CashGUI.Hide();
			this.DriversGUI.Hide();
			this.SkinsGUI.Hide();
			this.BoosterGUI.Hide();
			this.EffectsGUI.Hide();
			this.SpraysGUI.Hide();
		}

		private void ResetCurrentPages()
		{
			this.SkinsGUI.CurrentPage = 0;
			this.CashGUI.CurrentPage = 0;
			this.DriversGUI.CurrentPage = 0;
			this.BoosterGUI.CurrentPage = 0;
			this.EffectsGUI.CurrentPage = 0;
			this.SpraysGUI.CurrentPage = 0;
		}

		public void ShowDriverDetails(CharactersItem charitem)
		{
			this.DisableTabs();
			this.AnimateHide();
			this.Details.ShowDriverDetails(charitem.CharacterItemTypeScriptableObject);
		}

		public void ShowSkinDetails(StoreItem storeitem)
		{
			this.DisableTabs();
			this.AnimateHide();
			this.Details.ShowSkinDetails(storeitem);
		}

		public void ShowItemTypeDetails(ItemTypeScriptableObject itemType)
		{
			this.DisableTabs();
			this.AnimateHide();
			this._itemTypeDetails.Show(itemType);
		}

		public void OnCustomizationItemBought()
		{
			ShopGUI.MainMenuGui.OnCustomizationItemBought();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ShopGUI));

		[Header("Shop GUI Controllers")]
		[SerializeField]
		private ShopDriversGUI DriversGUI;

		[SerializeField]
		private ShopScreen SkinsGUI;

		[SerializeField]
		public ShopDetails Details;

		[SerializeField]
		private ShopScreen BoosterGUI;

		[SerializeField]
		private ShopCashGUI CashGUI;

		[SerializeField]
		private ShopScreen EffectsGUI;

		[SerializeField]
		private ShopScreen SpraysGUI;

		[SerializeField]
		private ItemTypeShopDetails _itemTypeDetails;

		[SerializeField]
		private UIToggle _driversToggle;

		[SerializeField]
		private UIToggle _skinsToggle;

		[SerializeField]
		private UIToggle _boosterToggle;

		[SerializeField]
		private UIToggle _cashToggle;

		[SerializeField]
		private UIToggle _effectsToggle;

		[SerializeField]
		private UIToggle _spraysToggle;

		[SerializeField]
		private Collider _toggleBlockerCollider;

		[SerializeField]
		private NGUIWidgetAlpha _bgWidgetAlpha;

		public bool Visible;

		public GameObject rootGameObject;

		[SerializeField]
		private GameObject _inputBlocker;

		public Color CashColor;

		public Color FameColor;

		public Color NoFundsColor;

		[Header("[Animation]")]
		[SerializeField]
		private Animation _animation;

		[SerializeField]
		private GameObject _objectToBeDisabledAfterAnimationFinishes;

		private ShopGUI.Tab _currentTab = ShopGUI.Tab.Drivers;

		public enum Tab
		{
			Hoplons,
			Drivers,
			Skins,
			Boosters,
			Effects,
			Sprays
		}
	}
}
