using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Exceptions;
using HeavyMetalMachines.Presenting.Extensions;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Battlepass.View
{
	public class BattlepassPresenter : IBattlepassPresenter, IPresenter
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		private void InitializeView()
		{
			this._hideSubject = new Subject<Unit>();
			this._disposables = new CompositeDisposable();
			this.GetViewFromProvider();
		}

		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(this.ShowView(), delegate(Unit _)
			{
				this.InitializeButtons();
			});
		}

		private void InitializeButtons()
		{
			IDisposable disposable = ButtonExtensions.InitializeNavigationAndBiToNode(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree, this._mainMenuPresenterTree.MainMenuNode, this._buttonBILogger, ButtonName.MetalpassBack);
			this._disposables.Add(disposable);
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(this._view.PlayOutAnimation(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
				this._hideSubject.OnNext(Unit.Default);
			});
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
			return this._hideSubject;
		}

		private void GetViewFromProvider()
		{
			this._view = this._viewProvider.Provide<IBattlepassView>(null);
			if (this._view == null)
			{
				throw new ViewNotFoundException(typeof(IBattlepassView));
			}
		}

		private IObservable<Unit> ShowView()
		{
			return Observable.Do<Unit>(this._view.PlayInAnimation(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.AddGroup();
			});
		}

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		[Inject]
		private IMainMenuPresenterTree _mainMenuPresenterTree;

		private IBattlepassView _view;

		private ISubject<Unit> _hideSubject;

		private CompositeDisposable _disposables;
	}
}
