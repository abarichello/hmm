using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.DLC;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using HeavyMetalMachines.Store.View;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Store.Presenter
{
	public class LegacyNGuiStorePresenter : IStorePresenter, IPresenter
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.ReturnUnit(), (Unit _) => this.InitializeView());
		}

		private IObservable<Unit> InitializeView()
		{
			this._disposables = new CompositeDisposable();
			this._view = this._viewProvider.Provide<IStoreView>(null);
			return this._view.Initialize();
		}

		private void InitializeButtons()
		{
			IDisposable disposable = ButtonExtensions.InitializeNavigationAndBiToNode(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree, this._mainMenuPresenterTree.MainMenuNode, this._buttonBILogger, ButtonName.ShopBack);
			this._disposables.Add(disposable);
		}

		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(this._view.AnimateShow(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.AddGroup();
				this.InitializeButtons();
				this.InitializeDlcObservation();
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(this._view.AnimateHide(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
				this.TryToDisposeDlcChangeObservation();
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._disposables.Dispose();
			}), delegate(Unit _)
			{
				this.TryToDisposeDlcChangeObservation();
			});
		}

		public IObservable<Unit> ObserveHide()
		{
			throw new NotImplementedException();
		}

		private void InitializeDlcObservation()
		{
			this._observeDlcChangeDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._observeDlcChange.Observe(), delegate(Unit _)
			{
				ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.MainMenuNode));
			}));
		}

		private void TryToDisposeDlcChangeObservation()
		{
			if (this._observeDlcChangeDisposable != null)
			{
				this._observeDlcChangeDisposable.Dispose();
				this._observeDlcChangeDisposable = null;
			}
		}

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		[Inject]
		private IMainMenuPresenterTree _mainMenuPresenterTree;

		[Inject]
		private IObserveDlcChange _observeDlcChange;

		private CompositeDisposable _disposables;

		private IDisposable _observeDlcChangeDisposable;

		private IStoreView _view;
	}
}
