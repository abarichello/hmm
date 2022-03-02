using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using HeavyMetalMachines.Store.Details.Infra;
using HeavyMetalMachines.Store.Details.View;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Store.Details.Presenter
{
	public class LegacyStoreItemDetailsPresenter : IStoreItemDetailsPresenter, IPresenter
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._disposables = new CompositeDisposable();
			}), delegate(Unit _)
			{
				this._view = this._viewProvider.Provide<IStoreItemDetailView>(null);
			}), delegate(Unit _)
			{
				this._itemType = this._storeItemDetailsRequest.ConsumeItemToShow();
			});
		}

		public IObservable<Unit> Show()
		{
			IObservable<Unit> observable;
			if (this._itemType.ItemCategoryId == InventoryMapper.CharactersCategoryGuid)
			{
				observable = this._view.ShowDriver(this._itemType);
			}
			else if (this._itemType.ItemCategoryId == InventoryMapper.SkinsCategoryGuid)
			{
				observable = this._view.ShowSkin(this._itemType);
			}
			else
			{
				observable = this._view.ShowGeneric(this._itemType);
			}
			return Observable.Do<Unit>(observable, delegate(Unit _)
			{
				this.InitializeButtons();
			});
		}

		private void InitializeButtons()
		{
			IDisposable disposable = ButtonExtensions.AddNavigationBackwards(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree);
			this._disposables.Add(disposable);
		}

		public IObservable<Unit> Hide()
		{
			return this._view.Hide();
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._disposables.Dispose();
			});
		}

		public IObservable<Unit> ObserveHide()
		{
			throw new NotImplementedException();
		}

		[Inject]
		private readonly IStoreItemDetailsRequestProvider _storeItemDetailsRequest;

		[Inject]
		private readonly IViewProvider _viewProvider;

		[Inject]
		private IMainMenuPresenterTree _mainMenuPresenterTree;

		private IStoreItemDetailView _view;

		private IItemType _itemType;

		private CompositeDisposable _disposables;
	}
}
