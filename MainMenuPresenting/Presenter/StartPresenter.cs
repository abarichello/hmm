using System;
using System.Collections.Generic;
using HeavyMetalMachines.Achievements;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting.Announcement;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.MatchMakingQueue.Infra;
using HeavyMetalMachines.News.Presenting;
using HeavyMetalMachines.Onboarding;
using HeavyMetalMachines.PeriodicRefresh;
using HeavyMetalMachines.PeriodicRefresh.API;
using HeavyMetalMachines.PeriodicRefresh.Infra;
using HeavyMetalMachines.PlayerSummary;
using HeavyMetalMachines.PlayerTooltip.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.ContextMenu;
using HeavyMetalMachines.Presenting.GenericConfirmWindow;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.SessionService;
using HeavyMetalMachines.ReportSystem;
using HeavyMetalMachines.ReportSystem.Infra;
using HeavyMetalMachines.ShopPopup.Presenting;
using HeavyMetalMachines.ShopPopup.Presenting.Business;
using HeavyMetalMachines.ShopPopup.Presenting.Infra;
using HeavyMetalMachines.Social.Friends.Presenting.FriendsList;
using HeavyMetalMachines.Social.Teams.Business;
using HeavyMetalMachines.Tournaments.API;
using HeavyMetalMachines.Training.Business;
using HeavyMetalMachines.Training.Presenter;
using HeavyMetalMachines.Utils;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using Pocketverse;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.MainMenuPresenting.Presenter
{
	public class StartPresenter : IStartPresenter, IPresenter
	{
		public IObservable<Unit> Initialize()
		{
			return Observable.DoOnCompleted<Unit>(Observable.DoOnSubscribe<Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				Observable.ContinueWith<Unit, Unit>(this.InitializePeriodicRefresh(), this.InitializeSocialPresenters()),
				this._mainMenuCompetitiveModePresenter.Initialize(),
				this.InitializeSeasonAnnouncementPresenter(),
				this.InitializePlayerSummaryPresenter(),
				this._newsPresenter.Initialize(),
				this._viewLoader.LoadView("UI_ADD_Battlepass"),
				Observable.DoOnCompleted<Unit>(Observable.DoOnSubscribe<Unit>(this._fetchAchievements.Fetch(), delegate()
				{
					this._logger.Info("FetchAchievements.Subscribe()");
				}), delegate()
				{
					this._logger.Info("FetchAchievements.Completed()");
				}),
				this._noviceTrialsAmountProvider.InitializeProvider(),
				this._shopPopupConfigProvider.InitializeProvider()
			}), delegate()
			{
				this._logger.Info("Initialize.Subscribed()");
			}), delegate()
			{
				this._logger.Info("Initialize.Completed()");
				this.InitializeContinuouslyCheckAndCancelMatchSearchOnCrossplayChange();
				this.InitializeContinuouslyShowFeedbackOnPublisherSessionError();
				this.ConsumePlatformSessionInvitationQueue();
			});
		}

		private void ConsumePlatformSessionInvitationQueue()
		{
			IEnumerable<SessionInvitationData> sessionInvitations = Platform.Current.GetSessionInvitations();
			foreach (SessionInvitationData sessionInvitationData in sessionInvitations)
			{
				this._notifyPlatformSessionInvitation.Notify(sessionInvitationData);
			}
			Platform.Current.StartNotifyingSessionInvitations();
		}

		private IObservable<Unit> InitializeSocialPresenters()
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this._playerTooltipPresenter.Initialize(),
				this._contextMenuPresenter.Initialize(),
				this._friendsListPresenter.Initialize()
			});
		}

		private IObservable<Unit> InitializeSeasonAnnouncementPresenter()
		{
			return this._seasonAnnouncementPresenter.Initialize();
		}

		private IObservable<Unit> InitializePlayerSummaryPresenter()
		{
			return this._playerSummaryPresenter.Initialize();
		}

		private IObservable<Unit> InitializePeriodicRefresh()
		{
			return Observable.DoOnCompleted<Unit>(Observable.ContinueWith<Unit, Unit>(this.FirstPeriodicRefresh(), Observable.Merge<Unit>(this._tournamentUpdate.Update(), new IObservable<Unit>[]
			{
				this._initializeTournamentTiers.Initialize(),
				this._initializeTournamentSeason.Initialize()
			})), new Action(this.StartPeriodicRefreshing));
		}

		private IObservable<Unit> FirstPeriodicRefresh()
		{
			return Observable.AsUnitObservable<PeriodicRefreshData>(Observable.Do<PeriodicRefreshData>(Observable.ContinueWith<Unit, PeriodicRefreshData>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._feedbackWatcher.Initialize();
			}), this._periodicRefreshDataService.Get()), delegate(PeriodicRefreshData refreshData)
			{
				this._setLocalTeam.Set(refreshData.Team);
				this._feedbackStorage.Set(refreshData.Feedbacks);
				this._restrictionStorage.Set(refreshData.Restrictions);
			}));
		}

		private void StartPeriodicRefreshing()
		{
			float floatValue = this._configLoader.GetFloatValue(ConfigAccess.PeriodicRefreshTimeInSeconds);
			TimeSpan timeSpan = TimeSpan.FromSeconds((double)floatValue);
			IObservable<Unit> observable = Observable.AsUnitObservable<long>(Observable.Interval(timeSpan));
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(this._periodicRefreshService.Initialize(observable)), this._disposables);
		}

		public IObservable<Unit> Show()
		{
			return Observable.DoOnCompleted<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(this.ShowTrainingPopUp(), this._playerSummaryPresenter.Show()), this.ShowOnboardingConfgratulations()), this._mainMenuCompetitiveModePresenter.Show()), this.ShowSeasonAnnouncementPresenter()), this.ShowShopPopup()), delegate()
			{
				this._biUtils.MarkConversion();
			});
		}

		private IObservable<Unit> ShowShopPopup()
		{
			ShopPopupScreenMode shopPopupScreenMode;
			if (this._shouldShowShopPopup.ShouldShow(this._seasonAnnouncementShowed, this._onboardingRewardClaimed, ref shopPopupScreenMode))
			{
				return Observable.ContinueWith<Unit, Unit>(this._shopPopupPresenter.Initialize(shopPopupScreenMode), (Unit _) => this._shopPopupPresenter.Show());
			}
			return Observable.ReturnUnit();
		}

		private IObservable<Unit> ShowSeasonAnnouncementPresenter()
		{
			this._seasonAnnouncementShowed = this._seasonAnnouncementPresenter.ShouldShow();
			if (!this._seasonAnnouncementShowed)
			{
				return Observable.ReturnUnit();
			}
			return Observable.ContinueWith<Unit, Unit>(this._seasonAnnouncementPresenter.Show(), (Unit _) => Observable.First<Unit>(this._seasonAnnouncementPresenter.ObserveHide()));
		}

		private IObservable<Unit> ShowTrainingPopUp()
		{
			if (!this._trainingPopUpRules.CanOpenPopUp())
			{
				return Observable.ReturnUnit();
			}
			return Observable.ContinueWith<Unit, Unit>(this._trainingPopUpPresenter.Initialize(), this._trainingPopUpPresenter.ShowAndWaitForConclusion());
		}

		private IObservable<Unit> ShowOnboardingConfgratulations()
		{
			this._onboardingRewardClaimed = this._shouldClaimOnboardingReward.ShouldClaim();
			if (this._onboardingRewardClaimed)
			{
				return this._onboardingCongratulationsPresenter.ShowAndWaitForConclusion();
			}
			return Observable.ReturnUnit();
		}

		private void InitializeContinuouslyCheckAndCancelMatchSearchOnCrossplayChange()
		{
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(this._continuouslyObserveCrossplayChangeAndCancelMatchSearch.Initialize()), this._disposables);
		}

		private void InitializeContinuouslyShowFeedbackOnPublisherSessionError()
		{
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<DialogConfiguration, Unit>(Observable.Select<Exception, DialogConfiguration>(Observable.Do<Exception>(Observable.FromEvent<Exception>(delegate(Action<Exception> handler)
			{
				this._publisherSessionService.OnError += handler;
			}, delegate(Action<Exception> handler)
			{
				this._publisherSessionService.OnError -= handler;
			}), new Action<Exception>(this.LogPublisherSessionError)), (Exception _) => this.ConvertToDialogConfiguration()), (DialogConfiguration dialogConfig) => this._confirmWindowPresenter.Show(dialogConfig))), this._disposables);
		}

		private void LogPublisherSessionError(Exception error)
		{
			this._logger.ErrorFormat("Error on PublisherSessionService. Will show feedback. Error={0}", new object[]
			{
				error
			});
		}

		private DialogConfiguration ConvertToDialogConfiguration()
		{
			return new DialogConfiguration
			{
				Message = this._localizeKey.GetFormatted("PARTY_ERROR_ALREADY_CLOSED", TranslationContext.MainMenuGui, new object[0])
			};
		}

		public IObservable<Unit> Hide()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(this._viewLoader.UnloadView("UI_ADD_Battlepass"), delegate(Unit _)
			{
				this.DisposeMainMenuCompetitiveModePresenter();
			}), delegate(Unit _)
			{
				this._feedbackStorage.Dispose();
			}), delegate(Unit _)
			{
				this._restrictionStorage.Dispose();
			}), delegate(Unit _)
			{
				this._feedbackWatcher.Dispose();
			}), delegate(Unit _)
			{
				this._disposables.Dispose();
			}), this.DisposeSocialPresenters()), this._playerSummaryPresenter.Dispose()), this._newsPresenter.Dispose());
		}

		private IObservable<Unit> DisposeSocialPresenters()
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this._playerTooltipPresenter.Dispose(),
				this._contextMenuPresenter.Dispose(),
				this._friendsListPresenter.Dispose()
			});
		}

		private void DisposeMainMenuCompetitiveModePresenter()
		{
			this._mainMenuCompetitiveModePresenter.Dispose();
		}

		public IObservable<Unit> ObserveHide()
		{
			throw new NotImplementedException();
		}

		private const string BattlepassSceneName = "UI_ADD_Battlepass";

		[Inject]
		private readonly IViewLoader _viewLoader;

		[Inject]
		private readonly IMainMenuCompetitiveModePresenter _mainMenuCompetitiveModePresenter;

		[Inject]
		private readonly ISeasonAnnouncementPresenter _seasonAnnouncementPresenter;

		[Inject]
		private readonly IFriendsListPresenter _friendsListPresenter;

		[Inject]
		private readonly IBiUtils _biUtils;

		[Inject]
		private readonly IContextMenuPresenter _contextMenuPresenter;

		[Inject]
		private readonly IPlayerTooltipPresenter _playerTooltipPresenter;

		[Inject]
		private readonly IPlayerSummaryPresenter _playerSummaryPresenter;

		[Inject]
		private readonly ITrainingPopUpRulesV3 _trainingPopUpRules;

		[Inject]
		private readonly ITrainingPopUpPresenterV3 _trainingPopUpPresenter;

		[Inject]
		private readonly INewsPresenter _newsPresenter;

		[Inject]
		private readonly IPeriodicRefreshService _periodicRefreshService;

		[Inject]
		private readonly ITournamentUpdate _tournamentUpdate;

		[Inject]
		private readonly IConfigLoader _configLoader;

		[Inject]
		private readonly IInitializeTournamentTiers _initializeTournamentTiers;

		[Inject]
		private readonly IPeriodicRefreshDataService _periodicRefreshDataService;

		[Inject]
		private readonly ISetLocalTeam _setLocalTeam;

		[Inject]
		private readonly IFeedbackWatcher _feedbackWatcher;

		[Inject]
		private readonly IFeedbackStorage _feedbackStorage;

		[Inject]
		private readonly IRestrictionStorage _restrictionStorage;

		[Inject]
		private readonly IInitializeTournamentSeason _initializeTournamentSeason;

		[Inject]
		private readonly IContinuouslyObserveCrossplayChangeAndCancelMatchSearch _continuouslyObserveCrossplayChangeAndCancelMatchSearch;

		[Inject]
		private readonly INotifyPublisherSessionInvitation _notifyPlatformSessionInvitation;

		[Inject]
		private readonly IFetchAchievements _fetchAchievements;

		[Inject]
		private readonly IOnboardingCongratulationsPresenter _onboardingCongratulationsPresenter;

		[Inject]
		private readonly IShouldClaimOnboardingReward _shouldClaimOnboardingReward;

		[Inject]
		private readonly INoviceTrialsAmountProvider _noviceTrialsAmountProvider;

		[Inject]
		private readonly IShopPopupConfigProvider _shopPopupConfigProvider;

		[Inject]
		private readonly IShouldShowShopPopup _shouldShowShopPopup;

		[Inject]
		private readonly IShopPopupPresenter _shopPopupPresenter;

		[Inject]
		private readonly IPublisherSessionService _publisherSessionService;

		[Inject]
		private readonly IGenericConfirmWindowPresenter _confirmWindowPresenter;

		[Inject]
		private readonly ILocalizeKey _localizeKey;

		[Inject]
		private readonly ILogger<StartPresenter> _logger;

		private CompositeDisposable _disposables = new CompositeDisposable();

		private bool _seasonAnnouncementShowed;

		private bool _onboardingRewardClaimed;
	}
}
