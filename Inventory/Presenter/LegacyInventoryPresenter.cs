using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.DLC;
using HeavyMetalMachines.Inventory.Tab.View;
using HeavyMetalMachines.Inventory.View;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Exceptions;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Inventory.Presenter
{
	public class LegacyInventoryPresenter : IInventoryPresenter, IPresenter
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_Inventory"), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		private void InitializeView()
		{
			this._hideSubject = new Subject<Unit>();
			this._disposables = new CompositeDisposable();
			this._tabViews = new List<IInventoryTabView>();
			this.GetViewsFromProvider();
		}

		public IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(this.ShowView(), delegate(Unit _)
			{
				this.InitializeButtons();
			}), delegate(Unit _)
			{
				this.InitializeDlcObservation();
			});
		}

		private void InitializeDlcObservation()
		{
			this._observeDlcChangeDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._observeDlcChange.Observe(), delegate(Unit _)
			{
				ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.MainMenuNode));
			}));
		}

		private void InitializeButtons()
		{
			this.InitializeBackButton();
			this.InitializeTabToggles();
		}

		private void InitializeBackButton()
		{
			IButton button = this._view.BackButton;
			ActivatableExtensions.Activate(button);
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(button.OnClick(), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.InventoryBack);
				button.IsInteractable = false;
				ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateBackwards(), delegate(Unit onNext)
				{
				}, delegate(Exception onError)
				{
					throw onError;
				});
			});
			this._disposables.Add(disposable);
		}

		private void InitializeTabToggles()
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Repeat<Unit>(Observable.Do<Unit>(Observable.ContinueWith<string, Unit>(Observable.Select<IInventoryTabView, string>(Observable.Do<IInventoryTabView>(Observable.First<IInventoryTabView>(this.ListenToTabClicks()), delegate(IInventoryTabView _)
			{
				this.DisableToggleInteractivity();
			}), new Func<IInventoryTabView, string>(this.GetNavigationName)), new Func<string, IObservable<Unit>>(this.NavigateToRelativeNode)), delegate(Unit _)
			{
				this.EnableToggleInteractivity();
			})));
			this._disposables.Add(disposable);
		}

		private void EnableToggleInteractivity()
		{
			for (int i = 0; i < this._tabViews.Count; i++)
			{
				IInventoryTabView inventoryTabView = this._tabViews[i];
				if (!inventoryTabView.ToggleButton.IsOn)
				{
					inventoryTabView.ToggleButton.IsInteractable = true;
				}
			}
		}

		private void DisableToggleInteractivity()
		{
			for (int i = 0; i < this._tabViews.Count; i++)
			{
				IInventoryTabView inventoryTabView = this._tabViews[i];
				inventoryTabView.ToggleButton.IsInteractable = false;
			}
		}

		private IObservable<Unit> NavigateToRelativeNode(string navigationName)
		{
			return this._mainMenuPresenterTree.PresenterTree.NavigateToRelativeNode(navigationName);
		}

		private IObservable<IInventoryTabView> OnTabClick(IInventoryTabView tabView)
		{
			return Observable.Defer<IInventoryTabView>(delegate()
			{
				IInventoryTabView tab = tabView;
				return Observable.Select<Unit, IInventoryTabView>(tab.ToggleButton.OnToggleOn(), (Unit _) => tab);
			});
		}

		private string GetNavigationName(IInventoryTabView tabView)
		{
			return string.Format("../{0}", tabView.NavigationName);
		}

		private IObservable<IInventoryTabView> ListenToTabClicks()
		{
			return Observable.Merge<IInventoryTabView>(this._tabViews.Select(new Func<IInventoryTabView, IObservable<IInventoryTabView>>(this.OnTabClick)));
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(this._view.PlayOutAnimation(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
				this._hideSubject.OnNext(Unit.Default);
				this.TryToDisposeDlcChangeObservation();
			});
		}

		private void TryToDisposeDlcChangeObservation()
		{
			if (this._observeDlcChangeDisposable != null)
			{
				this._observeDlcChangeDisposable.Dispose();
				this._observeDlcChangeDisposable = null;
			}
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._disposables.Dispose();
			}), delegate(Unit _)
			{
				this.TryToDisposeDlcChangeObservation();
			}), this._viewLoader.UnloadView("UI_ADD_Inventory"));
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private void GetViewsFromProvider()
		{
			this._view = this._viewProvider.Provide<IInventoryView>(null);
			if (this._view == null)
			{
				throw new ViewNotFoundException(typeof(IInventoryView));
			}
			this._tabViews.Add(this._viewProvider.Provide<IPortraitInventoryTabView>(null));
			this._tabViews.Add(this._viewProvider.Provide<IAvatarsInventoryTabView>(null));
			this._tabViews.Add(this._viewProvider.Provide<IKillsInventoryTabView>(null));
			this._tabViews.Add(this._viewProvider.Provide<IScoreInventoryTabView>(null));
			this._tabViews.Add(this._viewProvider.Provide<ILoreInventoryTabView>(null));
			this._tabViews.Add(this._viewProvider.Provide<IRespawnsInventoryTabView>(null));
			this._tabViews.Add(this._viewProvider.Provide<ISkinsInventoryTabView>(null));
			this._tabViews.Add(this._viewProvider.Provide<ISpraysInventoryTabView>(null));
			this._tabViews.Add(this._viewProvider.Provide<ITakeoffInventoryTabView>(null));
			this._tabViews.Add(this._viewProvider.Provide<IEmotesInventoryTabView>(null));
		}

		private IObservable<Unit> ShowView()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.UiNavigationGroupHolder.AddGroup();
				return this._view.PlayInAnimation();
			});
		}

		private const string InventorySceneName = "UI_ADD_Inventory";

		[Inject]
		private IViewLoader _viewLoader;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		[Inject]
		private IMainMenuPresenterTree _mainMenuPresenterTree;

		[Inject]
		private IObserveDlcChange _observeDlcChange;

		private IInventoryView _view;

		private ISubject<Unit> _hideSubject;

		private CompositeDisposable _disposables;

		private List<IInventoryTabView> _tabViews;

		private IPortraitInventoryTabView _portraitInventoryTabView;

		private IScoreInventoryTabView _scoreInventoryTabView;

		private IAvatarsInventoryTabView _avatarsInventoryTabView;

		private IKillsInventoryTabView _killsInventoryTabView;

		private ILoreInventoryTabView _loreInventoryTabView;

		private IRespawnsInventoryTabView _respawnsInventoryTabView;

		private ISkinsInventoryTabView _skinsInventoryTabView;

		private ISpraysInventoryTabView _spraysInventoryTabView;

		private ITakeoffInventoryTabView _takeoffInventoryTabView;

		private IEmotesInventoryTabView _emotesInventoryTabView;

		private IDisposable _observeDlcChangeDisposable;
	}
}
