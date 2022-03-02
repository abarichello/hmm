using System;
using HeavyMetalMachines.Customization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Inventory.View
{
	public class InventoryViewWrapper : MonoBehaviour, IInventoryView
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

		public IObservable<Unit> PlayInAnimation()
		{
			return this._legacyView.AnimateShow();
		}

		public IObservable<Unit> PlayOutAnimation()
		{
			return this._legacyView.AnimateHide();
		}

		private void Awake()
		{
			this._viewProvider.Bind<IInventoryView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IInventoryView>(null);
		}

		[Inject]
		[UsedImplicitly]
		private IViewProvider _viewProvider;

		[SerializeField]
		private CustomizationInventoryView _legacyView;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
