using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using Hoplon.Input.UiNavigation;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Store.View
{
	public class LegacyNGuiStoreView : MonoBehaviour, IStoreView
	{
		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IStoreView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IStoreView>(null);
		}

		public IObservable<Unit> Initialize()
		{
			return this._legacyShopGui.Initialize();
		}

		public IObservable<Unit> AnimateShow()
		{
			return this._legacyShopGui.Show();
		}

		public IObservable<Unit> AnimateHide()
		{
			return this._legacyShopGui.Hide();
		}

		[Inject]
		[UsedImplicitly]
		private IViewProvider _viewProvider;

		[SerializeField]
		private ShopGUI _legacyShopGui;

		[SerializeField]
		private NGuiButton _backButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
