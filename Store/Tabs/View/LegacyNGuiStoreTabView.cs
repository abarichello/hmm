using System;
using System.Collections.Generic;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Store.View;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Store.Tabs.View
{
	public class LegacyNGuiStoreTabView : MonoBehaviour, ICashStoreTabView, IDriversStoreTabView, ISkinsStoreTabView, IBoostersTabView, IEffectsStoreTabView, ISpraysStoreTabView, IEmotesStoreTabView, IStoreTabView
	{
		private void Awake()
		{
			switch (this._tabType)
			{
			case ShopGUI.Tab.Cash:
				this._viewProvider.Bind<ICashStoreTabView>(this, null);
				break;
			case ShopGUI.Tab.Drivers:
				this._viewProvider.Bind<IDriversStoreTabView>(this, null);
				break;
			case ShopGUI.Tab.Skins:
				this._viewProvider.Bind<ISkinsStoreTabView>(this, null);
				break;
			case ShopGUI.Tab.Boosters:
				this._viewProvider.Bind<IBoostersTabView>(this, null);
				break;
			case ShopGUI.Tab.Effects:
				this._viewProvider.Bind<IEffectsStoreTabView>(this, null);
				break;
			case ShopGUI.Tab.Sprays:
				this._viewProvider.Bind<ISpraysStoreTabView>(this, null);
				break;
			case ShopGUI.Tab.Emotes:
				this._viewProvider.Bind<IEmotesStoreTabView>(this, null);
				break;
			default:
				throw new NotImplementedException("Unknown ShopGUI.Tab: " + this._tabType);
			}
		}

		private void OnDestroy()
		{
			switch (this._tabType)
			{
			case ShopGUI.Tab.Cash:
				this._viewProvider.Unbind<ICashStoreTabView>(null);
				break;
			case ShopGUI.Tab.Drivers:
				this._viewProvider.Unbind<IDriversStoreTabView>(null);
				break;
			case ShopGUI.Tab.Skins:
				this._viewProvider.Unbind<ISkinsStoreTabView>(null);
				break;
			case ShopGUI.Tab.Boosters:
				this._viewProvider.Unbind<IBoostersTabView>(null);
				break;
			case ShopGUI.Tab.Effects:
				this._viewProvider.Unbind<IEffectsStoreTabView>(null);
				break;
			case ShopGUI.Tab.Sprays:
				this._viewProvider.Unbind<ISpraysStoreTabView>(null);
				break;
			case ShopGUI.Tab.Emotes:
				this._viewProvider.Unbind<IEmotesStoreTabView>(null);
				break;
			default:
				throw new NotImplementedException("Unknown ShopGUI.Tab: " + this._tabType);
			}
		}

		public void Reposition()
		{
			throw new NotImplementedException();
		}

		public IObservable<Unit> AnimateShow()
		{
			return this._legacyShopGui.NavigateToTab(this._tabType);
		}

		public IObservable<Unit> AnimateHide()
		{
			return Observable.ReturnUnit();
		}

		public List<IStoreItemView> StoreItems { get; private set; }

		public IUiNavigationSubGroupHolder UiNavigationSubGroupHolder { get; private set; }

		public IUiNavigationAxisSelector UiNavigationAxisSelector { get; private set; }

		public void Show()
		{
			throw new NotImplementedException();
		}

		public void Hide()
		{
			throw new NotImplementedException();
		}

		[Inject]
		[UsedImplicitly]
		private IViewProvider _viewProvider;

		[SerializeField]
		private ShopGUI _legacyShopGui;

		[SerializeField]
		private ShopGUI.Tab _tabType;
	}
}
