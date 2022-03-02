using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Exceptions;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Battlepass.BuyLevels
{
	public class BattlepassBuyLevelsPresenter : IBattlepassBuyLevelsPresenter, IPresenter
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
			this._buyCurrentLevelSubject = new Subject<Unit>();
			this._buyAllLevelsSubject = new Subject<Unit>();
			this._compositeDisposable = new CompositeDisposable();
			this.GetViewFromProvider();
			ActivatableExtensions.Activate(this._view.WindowActivatable);
			this._view.UiNavigationGroupHolder.AddGroup();
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
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.PurchaseSelectedButton.OnClick(), delegate(Unit _)
			{
				this.OnPurchaseSelectedClick();
			});
			IDisposable disposable2 = ObservableExtensions.Subscribe<Unit>(this._view.PurchaseAllButton.OnClick(), delegate(Unit _)
			{
				this.OnPurchaseAllClick();
			});
			IDisposable disposable3 = ObservableExtensions.Subscribe<Unit>(this._view.BackButton.OnClick(), delegate(Unit _)
			{
				this.OnBackClick();
			});
			this._compositeDisposable.Add(disposable);
			this._compositeDisposable.Add(disposable2);
			this._compositeDisposable.Add(disposable3);
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(this._view.HideWindowAnimation.Play(), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				ActivatableExtensions.Deactivate(this._view.WindowActivatable);
			}), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
			}), delegate(Unit _)
			{
				this._compositeDisposable.Dispose();
			});
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		public void SetupLevelValues(int selectedLevelsPrice, int targetLevel, int allLevelsPrice)
		{
			targetLevel++;
			this._view.SelectedLevelsPriceLabel.Text = selectedLevelsPrice.ToString("0");
			this._view.TargetLevelLabel.Text = targetLevel.ToString("0");
			this._view.AllLevelsPriceLabel.Text = allLevelsPrice.ToString("0");
		}

		public IObservable<Unit> ObserveBuyCurrentLevel()
		{
			return this._buyCurrentLevelSubject;
		}

		public IObservable<Unit> ObserveBuyAllLevels()
		{
			return this._buyAllLevelsSubject;
		}

		private void GetViewFromProvider()
		{
			this._view = this._viewProvider.Provide<IBattlepassBuyLevelsView>(null);
			if (this._view == null)
			{
				throw new ViewNotFoundException(typeof(IBattlepassBuyLevelsView));
			}
		}

		private IObservable<Unit> ShowView()
		{
			return this._view.ShowWindowAnimation.Play();
		}

		private void OnPurchaseSelectedClick()
		{
			this._buyCurrentLevelSubject.OnNext(Unit.Default);
			this.CloseWindow();
		}

		private void OnPurchaseAllClick()
		{
			this._buyAllLevelsSubject.OnNext(Unit.Default);
			this.CloseWindow();
		}

		private void OnBackClick()
		{
			this.CloseWindow();
		}

		private void CloseWindow()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this.Hide(), (Unit _) => this.Dispose()));
		}

		[Inject]
		private IViewProvider _viewProvider;

		private IBattlepassBuyLevelsView _view;

		private Subject<Unit> _hideSubject;

		private CompositeDisposable _compositeDisposable;

		private ISubject<Unit> _buyCurrentLevelSubject;

		private ISubject<Unit> _buyAllLevelsSubject;
	}
}
