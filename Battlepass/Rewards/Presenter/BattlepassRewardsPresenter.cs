using System;
using HeavyMetalMachines.Battlepass.Rewards.View;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Exceptions;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.Battlepass.Rewards.Presenter
{
	public class BattlepassRewardsPresenter : IBattlepassRewardsPresenter
	{
		public BattlepassRewardsPresenter(IViewLoader viewLoader, IViewProvider viewProvider, IBattlepassRewardComponent battlepassRewardComponent)
		{
			if (viewLoader == null)
			{
				throw new ArgumentNullException("viewLoader");
			}
			if (viewProvider == null)
			{
				throw new ArgumentNullException("viewProvider");
			}
			if (battlepassRewardComponent == null)
			{
				throw new ArgumentNullException("battlepassRewardComponent");
			}
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._battlepassRewardComponent = battlepassRewardComponent;
			this._isClaimingRewards = false;
		}

		private IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(this.LoadScene(), delegate(Unit _)
			{
				this.GetViewFromProvider();
			}), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		private IObservable<Unit> Show()
		{
			return Observable.Do<Unit>(this._view.PlayInAnimation(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.AddHighPriorityGroup();
			});
		}

		private IObservable<Unit> WaitForHide()
		{
			return Observable.Do<Unit>(this._view.LegacyObserveHide(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveHighPriorityGroup();
			});
		}

		public IObservable<Unit> LoadScene()
		{
			return this._viewLoader.LoadView("UI_ADD_BattlepassReward");
		}

		public IObservable<Unit> UnloadScene()
		{
			return this._viewLoader.UnloadView("UI_ADD_BattlepassReward");
		}

		private IObservable<Unit> Dispose()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				if (this._disposables != null)
				{
					this._disposables.Dispose();
					this._disposables = null;
				}
			});
		}

		public IObservable<bool> TryToOpenRewardsToClaim()
		{
			if (this._isClaimingRewards)
			{
				BattlepassRewardsPresenter.Log.Warn("Already claiming rewards.");
				return Observable.Return(false);
			}
			if (!this._battlepassRewardComponent.HasRewardToClaim())
			{
				BattlepassRewardsPresenter.Log.Warn("Do not have rewards to claim.");
				return Observable.Return(false);
			}
			this._isClaimingRewards = true;
			return Observable.Select<Unit, bool>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(this.Initialize(), (Unit _) => this.Show()), (Unit _) => this.WaitForHide()), delegate(Unit _)
			{
				this._isClaimingRewards = false;
			}), this.UnloadScene()), this.Dispose()), (Unit _) => true);
		}

		private void InitializeView()
		{
			this.TryDisposeAndCreateCompositeDisposable();
			this.GetViewFromProvider();
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(this._view.NextRewardButton.OnClick(), delegate(Unit _)
			{
				this._view.LegacyOnClickNextRewardButton();
			});
			IDisposable disposable2 = ObservableExtensions.Subscribe<Unit>(this._view.ClaimAllButton.OnClick(), delegate(Unit _)
			{
				this._view.LegacyOnClickClaimAllButton();
			});
			this._disposables.Add(disposable);
			this._disposables.Add(disposable2);
		}

		private void TryDisposeAndCreateCompositeDisposable()
		{
			if (this._disposables != null)
			{
				this._disposables.Dispose();
			}
			this._disposables = new CompositeDisposable();
		}

		private void GetViewFromProvider()
		{
			this._view = this._viewProvider.Provide<IBattlepassRewardsView>(null);
			if (this._view == null)
			{
				throw new ViewNotFoundException(typeof(IBattlepassRewardsView));
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BattlepassRewardsPresenter));

		private const string SceneName = "UI_ADD_BattlepassReward";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly IBattlepassRewardComponent _battlepassRewardComponent;

		private IBattlepassRewardsView _view;

		private CompositeDisposable _disposables;

		private bool _isClaimingRewards;
	}
}
