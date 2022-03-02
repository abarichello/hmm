using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Localization.Business;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting.Navigation;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Store.Business.Filter;
using HeavyMetalMachines.Store.Presenter;
using HeavyMetalMachines.Store.Tabs.Presenter;
using HeavyMetalMachines.Store.Tabs.View;
using HeavyMetalMachines.Store.View;
using HeavyMetalMachines.Utils;
using Hoplon.Input;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class ShopGUI : GameHubBehaviour, IShopGui
	{
		protected void Awake()
		{
			this.rootGameObject.SetActive(false);
			this._currentTab = ShopGUI.Tab.Drivers;
			this._storePaginationPresenter = new StorePaginationPresenter(this._storePaginationView);
			this.effectPresenter = new StoreTabPresenter(new Guid[]
			{
				InventoryMapper.VfxKillCategoryGuid,
				InventoryMapper.VfxRespawnCategoryGuid,
				InventoryMapper.VfxScoreCategoryGuid,
				InventoryMapper.VfxTakeOffCategoryGuid
			}, this.effectStoreTabView, GameHubBehaviour.Hub.InventoryColletion, this._storeBusinessFactory, StoreFilterType.CategoryNameAscending, this._storePaginationPresenter);
			this.sprayPresenter = new StoreTabPresenter(new Guid[]
			{
				InventoryMapper.SprayCategoryGuid
			}, this.sprayStoreTabView, GameHubBehaviour.Hub.InventoryColletion, this._storeBusinessFactory, StoreFilterType.ShopTitleAscending, this._storePaginationPresenter);
			this.emotePresenter = new StoreTabPresenter(new Guid[]
			{
				InventoryMapper.EmoteCategoryGuid
			}, this.EmotesGUI, GameHubBehaviour.Hub.InventoryColletion, this._storeBusinessFactory, StoreFilterType.ShopTitleAscending, this._storePaginationPresenter);
			this.skinPresenter = new StoreTabPresenter(new Guid[]
			{
				InventoryMapper.SkinsCategoryGuid
			}, this.skinStoreTabView, GameHubBehaviour.Hub.InventoryColletion, this._storeBusinessFactory, StoreFilterType.PriceSoftAscendingAndInventoryTitle, this._storePaginationPresenter);
			this.driverPresenter = new StoreTabPresenter(new Guid[]
			{
				InventoryMapper.CharactersCategoryGuid
			}, this.driverStoreTabView, GameHubBehaviour.Hub.InventoryColletion, this._storeBusinessFactory, StoreFilterType.PriceSoftAscendingAndShopTitle, this._storePaginationPresenter);
			this._storeFilterPresenter = new StoreFilterPresenter(this._storeFilterView, this._getLocalizedStoreSorterTitle, this._inputActiveDeviceChangeNotifier);
		}

		public void CleanUp()
		{
			this.BoosterGUI.CleanUp();
			this.CashGUI.CleanUp();
			ObservableExtensions.Subscribe<Unit>(this.driverPresenter.Dispose());
			ObservableExtensions.Subscribe<Unit>(this.skinPresenter.Dispose());
			ObservableExtensions.Subscribe<Unit>(this.effectPresenter.Dispose());
			ObservableExtensions.Subscribe<Unit>(this.sprayPresenter.Dispose());
			ObservableExtensions.Subscribe<Unit>(this.emotePresenter.Dispose());
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
			this._emotesToggle.Set(false, false);
		}

		public void AnimateShow(ShopGUI.Tab tab, Guid itemTypeId)
		{
			this.SetBackButtonCollider(true);
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
		}

		private void ShowReturnToShopFromTab()
		{
			this._toggleBlockerCollider.enabled = false;
		}

		[Obsolete("Use mainmenutree.StoreCash")]
		public void AnimateShowAndOpenCash()
		{
			ShopGUI.Log.WarnFormat("called obsolete AnimateShowAndOpenCash Use mainmenutree.StoreCash", new object[0]);
		}

		public void AnimateReturnfromSkinDetails()
		{
			this.GoBackToTab(ShopGUI.Tab.Skins);
		}

		public void AnimateReturnToCash()
		{
			this._animation.Play("DetailsOut");
			this.GoBackToTab(ShopGUI.Tab.Cash);
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
			this._bgWidgetAlpha.Alpha = 1f;
			this._animation.Play("DetailsIn");
			this._toggleBlockerCollider.enabled = true;
		}

		[Obsolete]
		public void ReturnToMainMenu()
		{
			ShopGUI.Log.Warn("using Obsolete ReturnToMainMenu");
		}

		private void HideToReturnMainMenu()
		{
			ShopGUI.Log.Warn("using Obsolete HideToReturnMainMenu");
		}

		private void TryNavigate(IPresenterNode node)
		{
			ShopGUI.Tab currentTab = this._currentTab;
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(node), delegate(Unit success)
			{
			}, delegate(Exception onError)
			{
				this.ReactivateToggle(currentTab);
			});
		}

		public void SwitchToHoplonsScreen()
		{
			this._clientButtonBiLogger.LogButtonClick(ButtonName.ShopTabCash);
			this.TryNavigate(this._mainMenuPresenterTree.StoreCashNode);
		}

		public void SwitchToDriversScreen()
		{
			this._clientButtonBiLogger.LogButtonClick(ButtonName.ShopTabMachines);
			this.TryNavigate(this._mainMenuPresenterTree.StoreDriversNode);
		}

		public void SwitchToSkinsScreen()
		{
			this._clientButtonBiLogger.LogButtonClick(ButtonName.ShopTabModels);
			this.TryNavigate(this._mainMenuPresenterTree.StoreSkinsNode);
		}

		public void SwitchToBoostersScreen()
		{
			this._clientButtonBiLogger.LogButtonClick(ButtonName.ShopTabBoosters);
			this.TryNavigate(this._mainMenuPresenterTree.StoreBoostersNode);
		}

		public void SwitchToEffectsScreen()
		{
			this._clientButtonBiLogger.LogButtonClick(ButtonName.ShopTabEffects);
			this.TryNavigate(this._mainMenuPresenterTree.StoreEffectsNode);
		}

		public void SwitchToSpraysScreen()
		{
			this._clientButtonBiLogger.LogButtonClick(ButtonName.ShopTabSprays);
			this.TryNavigate(this._mainMenuPresenterTree.StoreSpraysNode);
		}

		public void SwitchToEmotesScreen()
		{
			this._clientButtonBiLogger.LogButtonClick(ButtonName.ShopTabEmotes);
			this.TryNavigate(this._mainMenuPresenterTree.StoreEmotesNode);
		}

		private void ReactivateToggle(ShopGUI.Tab tab)
		{
			this.UntoggleMenu();
			switch (tab)
			{
			case ShopGUI.Tab.Cash:
				this._currentTab = ShopGUI.Tab.Cash;
				this._cashToggle.Set(true, true);
				break;
			case ShopGUI.Tab.Drivers:
				this._currentTab = ShopGUI.Tab.Drivers;
				this._driversToggle.Set(true, true);
				break;
			case ShopGUI.Tab.Skins:
				this._currentTab = ShopGUI.Tab.Skins;
				this._skinsToggle.Set(true, true);
				break;
			case ShopGUI.Tab.Boosters:
				this._currentTab = ShopGUI.Tab.Boosters;
				this._boosterToggle.Set(true, true);
				break;
			case ShopGUI.Tab.Effects:
				this._currentTab = ShopGUI.Tab.Effects;
				this._effectsToggle.Set(true, true);
				break;
			case ShopGUI.Tab.Sprays:
				this._currentTab = ShopGUI.Tab.Sprays;
				this._spraysToggle.Set(true, true);
				break;
			case ShopGUI.Tab.Emotes:
				this._currentTab = ShopGUI.Tab.Emotes;
				this._emotesToggle.Set(true, true);
				break;
			}
		}

		public void OpenScreen(ShopGUI.Tab tab)
		{
			this.UntoggleMenu();
			if (this.Details.IsVisible())
			{
				this.Details.Hide();
			}
			if (this._currentTab != tab)
			{
				this.DisableTabs();
			}
			switch (tab)
			{
			case ShopGUI.Tab.Cash:
				this._currentTab = ShopGUI.Tab.Cash;
				this.CashGUI.Show();
				this._cashToggle.Set(true, false);
				this._storePaginationPresenter.DisablePagination();
				this._storeFilterPresenter.DisableFilter();
				break;
			case ShopGUI.Tab.Drivers:
				this._currentTab = ShopGUI.Tab.Drivers;
				ObservableExtensions.Subscribe<Unit>(this.driverPresenter.Show());
				this._driversToggle.Set(true, false);
				this._storePaginationPresenter.InitializeTabPagination(this.driverPresenter);
				this._storeFilterPresenter.InitializeFilter(new StoreFilterContract(true, new List<StoreFilterType>
				{
					StoreFilterType.PriceSoftAscendingAndShopTitle,
					StoreFilterType.PriceHardAscendingAndShopTitle,
					StoreFilterType.ShopTitleAscending,
					StoreFilterType.ShopTitleDescending,
					StoreFilterType.CharacterRoleInterceptor,
					StoreFilterType.CharacterRoleTransporter,
					StoreFilterType.CharacterRoleSupport
				}), this.driverPresenter);
				break;
			case ShopGUI.Tab.Skins:
				this._currentTab = ShopGUI.Tab.Skins;
				ObservableExtensions.Subscribe<Unit>(this.skinPresenter.Show());
				this._skinsToggle.Set(true, false);
				this._storePaginationPresenter.InitializeTabPagination(this.skinPresenter);
				this._storeFilterPresenter.InitializeFilter(new StoreFilterContract(true, new List<StoreFilterType>
				{
					StoreFilterType.PriceSoftAscendingAndInventoryTitle,
					StoreFilterType.PriceHardAscendingAndInventoryTitle,
					StoreFilterType.InventoryTitleAscending,
					StoreFilterType.SkinIdol,
					StoreFilterType.SkinRockstar,
					StoreFilterType.SkinMetalLegend,
					StoreFilterType.SkinHeavyMetal,
					StoreFilterType.SkinRoleInterceptor,
					StoreFilterType.SkinRoleTransporter,
					StoreFilterType.SkinRoleSupport
				}), this.skinPresenter);
				break;
			case ShopGUI.Tab.Boosters:
				this._currentTab = ShopGUI.Tab.Boosters;
				this.BoosterGUI.Show();
				this._boosterToggle.Set(true, false);
				this._storePaginationPresenter.DisablePagination();
				this._storeFilterPresenter.DisableFilter();
				break;
			case ShopGUI.Tab.Effects:
				this._currentTab = ShopGUI.Tab.Effects;
				ObservableExtensions.Subscribe<Unit>(this.effectPresenter.Show());
				this._effectsToggle.Set(true, false);
				this._storePaginationPresenter.InitializeTabPagination(this.effectPresenter);
				this._storeFilterPresenter.InitializeFilter(new StoreFilterContract(true, new List<StoreFilterType>
				{
					StoreFilterType.CategoryNameAscending,
					StoreFilterType.VfxKill,
					StoreFilterType.VfxRespawn,
					StoreFilterType.VfxScore,
					StoreFilterType.VfxTakeOff
				}), this.effectPresenter);
				break;
			case ShopGUI.Tab.Sprays:
				this._currentTab = ShopGUI.Tab.Sprays;
				ObservableExtensions.Subscribe<Unit>(this.sprayPresenter.Show());
				this._spraysToggle.Set(true, false);
				this._storePaginationPresenter.InitializeTabPagination(this.sprayPresenter);
				this._storeFilterPresenter.DisableFilter();
				break;
			case ShopGUI.Tab.Emotes:
				this._currentTab = ShopGUI.Tab.Emotes;
				ObservableExtensions.Subscribe<Unit>(this.emotePresenter.Show());
				this._emotesToggle.Set(true, false);
				this._storePaginationPresenter.InitializeTabPagination(this.emotePresenter);
				this._storeFilterPresenter.DisableFilter();
				break;
			}
		}

		public void DisableTabs()
		{
			this.CashGUI.Hide();
			this.BoosterGUI.Hide();
			ObservableExtensions.Subscribe<Unit>(this.driverPresenter.Hide());
			ObservableExtensions.Subscribe<Unit>(this.skinPresenter.Hide());
			ObservableExtensions.Subscribe<Unit>(this.effectPresenter.Hide());
			ObservableExtensions.Subscribe<Unit>(this.sprayPresenter.Hide());
			ObservableExtensions.Subscribe<Unit>(this.emotePresenter.Hide());
			this._storePaginationPresenter.DisablePagination();
		}

		private void ResetCurrentPages()
		{
			this.CashGUI.CurrentPage = 0;
			this.BoosterGUI.CurrentPage = 0;
		}

		public void ShowDriverDetails(IItemType charItem)
		{
			this.DisableTabs();
			this.AnimateHide();
			this.Details.ShowDriverDetails(charItem);
		}

		public void ShowSkinDetails(IItemType skinItem)
		{
			this.DisableTabs();
			this.AnimateHide();
			this.Details.ShowSkinDetails(skinItem);
		}

		public void ShowItemTypeDetails(IItemType itemType)
		{
			this.DisableTabs();
			this.AnimateHide();
			this._itemTypeDetails.Show(itemType);
		}

		public void OnCustomizationItemBought()
		{
			ShopGUI.MainMenuGui.OnCustomizationItemBought();
		}

		public void SetBackButtonCollider(bool isEnabled)
		{
			this._backButtonCollider.enabled = isEnabled;
		}

		public ShopGUI.Tab GetCurrentTab()
		{
			return this._currentTab;
		}

		private void OnDestroy()
		{
			this.driverPresenter.Hide();
			this.skinPresenter.Hide();
			this.effectPresenter.Hide();
			this.emotePresenter.Hide();
			this.sprayPresenter.Hide();
		}

		public IObservable<Unit> Show()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Defer<Unit>(delegate()
			{
				this.BeforeShowAnimation();
				return Observable.ReturnUnit();
			}), Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(this.PlayAnimation("ShopIn"), delegate(Unit _)
			{
				this.CreateCompositeDisposable();
			}), delegate(Unit _)
			{
				this.SubscribePresenterClicks();
			}), delegate(Unit _)
			{
				this.AfterShowAnimation();
			}));
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.driverPresenter.Initialize();
			}), delegate(Unit _)
			{
				this.skinPresenter.Initialize();
			}), delegate(Unit _)
			{
				this.effectPresenter.Initialize();
			}), delegate(Unit _)
			{
				this.emotePresenter.Initialize();
			}), delegate(Unit _)
			{
				this.sprayPresenter.Initialize();
			});
		}

		public IObservable<Unit> Hide()
		{
			this.BeforeHideAnimation();
			return Observable.Do<Unit>(this.PlayAnimation("ShopOut"), delegate(Unit _)
			{
				this.AfterHideAnimation();
			});
		}

		private IObservable<Unit> PlayAnimation(string stateName)
		{
			AnimationClip clip = this._animation.GetClip(stateName);
			return Observable.Delay<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._animation.Play(clip.name);
			}), TimeSpan.FromSeconds((double)clip.length), Scheduler.MainThreadIgnoreTimeScale);
		}

		private void CreateCompositeDisposable()
		{
			this._compositeDisposable = new CompositeDisposable();
		}

		private void SubscribePresenterClicks()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<IItemType>(this.driverPresenter.OnClick, new Action<IItemType>(this.OnItemTypeClick));
			IDisposable disposable2 = ObservableExtensions.Subscribe<IItemType>(this.skinPresenter.OnClick, new Action<IItemType>(this.OnItemTypeClick));
			IDisposable disposable3 = ObservableExtensions.Subscribe<IItemType>(this.effectPresenter.OnClick, new Action<IItemType>(this.OnItemTypeClick));
			IDisposable disposable4 = ObservableExtensions.Subscribe<IItemType>(this.emotePresenter.OnClick, new Action<IItemType>(this.OnItemTypeClick));
			IDisposable disposable5 = ObservableExtensions.Subscribe<IItemType>(this.sprayPresenter.OnClick, new Action<IItemType>(this.OnItemTypeClick));
			this._compositeDisposable.Add(disposable);
			this._compositeDisposable.Add(disposable2);
			this._compositeDisposable.Add(disposable3);
			this._compositeDisposable.Add(disposable4);
			this._compositeDisposable.Add(disposable5);
		}

		private void BeforeHideAnimation()
		{
			this.ClearCompositeDisposable();
			base.StopAllCoroutines();
			if (this.Details.IsVisible())
			{
				this.Details.Hide();
			}
			this._inputBlocker.SetActive(true);
			this._toggleBlockerCollider.enabled = true;
			this.SetBackButtonCollider(false);
			this.DisableTabs();
		}

		private void ClearCompositeDisposable()
		{
			if (this._compositeDisposable != null)
			{
				this._compositeDisposable.Dispose();
				this._compositeDisposable = null;
			}
		}

		private void AfterHideAnimation()
		{
			this._inputBlocker.SetActive(false);
			if (this.rootGameObject.activeSelf)
			{
				this.rootGameObject.SetActive(false);
			}
		}

		private void BeforeShowAnimation()
		{
			if (!this.rootGameObject.activeSelf)
			{
				this.rootGameObject.SetActive(true);
			}
			this._toggleBlockerCollider.enabled = true;
		}

		private void AfterShowAnimation()
		{
			this.SetBackButtonCollider(true);
			base.StopAllCoroutines();
			this._inputBlocker.SetActive(false);
			this.ResetCurrentPages();
			this._toggleBlockerCollider.enabled = false;
		}

		private void OnItemTypeClick(IItemType itemTypeSelected)
		{
			if (itemTypeSelected.ItemCategoryId == InventoryMapper.CharactersCategoryGuid)
			{
				this.ShowDriverDetails(itemTypeSelected);
			}
			else if (itemTypeSelected.ItemCategoryId == InventoryMapper.SkinsCategoryGuid)
			{
				this.ShowSkinDetails(itemTypeSelected);
			}
			else
			{
				this.ShowItemTypeDetails(itemTypeSelected);
			}
		}

		public IObservable<Unit> NavigateToTab(ShopGUI.Tab tab)
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.OpenScreen(tab);
			});
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ShopGUI));

		[Header("[Shop GUI Controllers]")]
		[SerializeField]
		public ShopDetails Details;

		[SerializeField]
		private ShopScreen BoosterGUI;

		[SerializeField]
		private ShopCashGUI CashGUI;

		[SerializeField]
		private ItemTypeShopDetails _itemTypeDetails;

		[SerializeField]
		private NguiStoreTabView driverStoreTabView;

		[SerializeField]
		private NguiStoreTabView skinStoreTabView;

		[SerializeField]
		private NguiStoreTabView effectStoreTabView;

		[SerializeField]
		private NguiStoreTabView sprayStoreTabView;

		[SerializeField]
		private NguiStoreTabView EmotesGUI;

		private IStoreTabPresenter driverPresenter;

		private IStoreTabPresenter skinPresenter;

		private IStoreTabPresenter effectPresenter;

		private IStoreTabPresenter sprayPresenter;

		private IStoreTabPresenter emotePresenter;

		[SerializeField]
		private NguiStorePaginationView _storePaginationView;

		[SerializeField]
		private NguiStoreFilterView _storeFilterView;

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
		private UIToggle _emotesToggle;

		[SerializeField]
		private Collider _toggleBlockerCollider;

		[SerializeField]
		private NGUIWidgetAlpha _bgWidgetAlpha;

		[SerializeField]
		private BoxCollider _backButtonCollider;

		public GameObject rootGameObject;

		private IStorePaginationPresenter _storePaginationPresenter;

		private IStoreFilterPresenter _storeFilterPresenter;

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

		[Inject]
		private IStoreBusinessFactory _storeBusinessFactory;

		[Inject]
		private IMainMenuPresenterTree _mainMenuPresenterTree;

		[Inject]
		private IGetLocalizedStoreSorterTitle _getLocalizedStoreSorterTitle;

		[Inject]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[Inject]
		private IClientButtonBILogger _clientButtonBiLogger;

		private ShopGUI.Tab _currentTab = ShopGUI.Tab.Drivers;

		private CompositeDisposable _compositeDisposable;

		public enum Tab
		{
			Cash,
			Drivers,
			Skins,
			Boosters,
			Effects,
			Sprays,
			Emotes
		}
	}
}
