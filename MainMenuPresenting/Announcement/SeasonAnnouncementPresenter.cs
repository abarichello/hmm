using System;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.Battlepass.Business;
using HeavyMetalMachines.Battlepass.Seasons;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Swordfish;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavyMetalMachines.MainMenuPresenting.Announcement
{
	public class SeasonAnnouncementPresenter : ISeasonAnnouncementPresenter, IPresenter
	{
		public SeasonAnnouncementPresenter(IViewLoader viewLoader, IViewProvider viewProvider, IGetCurrentOrNextCompetitiveSeason getCurrentOrNextCompetitiveSeason, IBattlepassDetailComponent battlepassDetailComponent, ILocalizeKey translation, IConsumeCurrentCompetitiveSeasonNews consumeCurrentCompetitiveSeasonNews, IMainMenuPresenterTree mainMenuPresenterTree, IConsumeCurrentBattlepassSeasonNews consumeCurrentBattlepassSeasonNews, IGetBattlepassSeason getBattlepassSeason, IGetBattlepassDateTranslation getBattlepassDateTranslation, IClientBILogger clientBiLogger, IClientButtonBILogger clientButtonBiLogger)
		{
			this.AssertValidParameters(viewLoader, viewProvider, getCurrentOrNextCompetitiveSeason, battlepassDetailComponent, translation, consumeCurrentCompetitiveSeasonNews, mainMenuPresenterTree, consumeCurrentBattlepassSeasonNews);
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._getCurrentOrNextCompetitiveSeason = getCurrentOrNextCompetitiveSeason;
			this._battlepassDetailComponent = battlepassDetailComponent;
			this._translation = translation;
			this._consumeCurrentCompetitiveSeasonNews = consumeCurrentCompetitiveSeasonNews;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
			this._consumeCurrentBattlepassSeasonNews = consumeCurrentBattlepassSeasonNews;
			this._getBattlepassSeason = getBattlepassSeason;
			this._getBattlepassDateTranslation = getBattlepassDateTranslation;
			this._clientBiLogger = clientBiLogger;
			this._clientButtonBiLogger = clientButtonBiLogger;
			this._closingPresenter = false;
		}

		public IObservable<Unit> OnOpenBattlepassRequested
		{
			get
			{
				return this._onOpenBattlepassRequested;
			}
		}

		private void AssertValidParameters(IViewLoader viewLoader, IViewProvider viewProvider, IGetCurrentOrNextCompetitiveSeason getCurrentOrNextCompetitiveSeason, IBattlepassDetailComponent battlepassDetailComponent, ILocalizeKey translation, IConsumeCurrentCompetitiveSeasonNews consumeCurrentCompetitiveSeasonNews, IMainMenuPresenterTree mainMenuPresenterTree, IConsumeCurrentBattlepassSeasonNews consumeCurrentBattlepassSeasonNews)
		{
		}

		private IObservable<Unit> GetBattlepassAnnouncementState()
		{
			return Observable.AsUnitObservable<bool>(Observable.Do<bool>(this._consumeCurrentBattlepassSeasonNews.TryConsume(), delegate(bool hasBattlepassNews)
			{
				this._announcementState.ShowBattlepass = hasBattlepassNews;
			}));
		}

		private IObservable<Unit> GetCompetitiveAnnouncementState()
		{
			return Observable.AsUnitObservable<bool>(Observable.Do<bool>(this._consumeCurrentCompetitiveSeasonNews.TryConsume(), delegate(bool hasCompetitiveNews)
			{
				this._announcementState.ShowCompetitiveMode = hasCompetitiveNews;
			}));
		}

		public bool ShouldShow()
		{
			return this._announcementState.ShowCompetitiveMode || this._announcementState.ShowBattlepass;
		}

		public IObservable<Unit> Initialize()
		{
			this._announcementState = new AnnouncementState
			{
				ShowBattlepass = false,
				ShowCompetitiveMode = false
			};
			return Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ReturnUnit(), (Unit _) => this.GetBattlepassAnnouncementState()), (Unit _) => this.GetCompetitiveAnnouncementState());
		}

		private void InitializeBackground()
		{
			if (this._announcementState.ShowBattlepass && this._announcementState.ShowCompetitiveMode)
			{
				this._view.BackgroundImage.SetImageName(this._view.MergedBackgroundImageName);
				return;
			}
			if (this._announcementState.ShowBattlepass)
			{
				this._view.BackgroundImage.SetImageName(this._view.BattlepassBackgroundImageName);
				return;
			}
			if (this._announcementState.ShowCompetitiveMode)
			{
				this._view.BackgroundImage.SetImageName(this._view.CompetitiveModeBackgroundImageName);
			}
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<ISeasonAnnouncementView>(null);
			this.InitializeBackground();
			this.InitializeCompetitiveModeSubPresenter();
			this.InitializeBattlepassSubPresenter();
			this._view.CloseButton.IsInteractable = false;
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Where<Unit>(Observable.First<Unit>(this._view.CloseButton.OnClick()), (Unit _) => !this._closingPresenter), delegate(Unit _)
			{
				this._clientButtonBiLogger.LogButtonClick(ButtonName.WelcomeFeaturesOk);
			}), (Unit _) => this.Hide()));
			this._view.MainCanvas.Enable();
		}

		private void InitializeCompetitiveModeSubPresenter()
		{
			this._competitiveAnnouncementPresenter = new CompetitiveModeSeasonAnnouncementPresenter(this._viewProvider, this._getCurrentOrNextCompetitiveSeason);
			this._competitiveAnnouncementPresenter.Initialize(this._announcementState.ShowCompetitiveMode);
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Where<Unit>(Observable.First<Unit>(this._competitiveAnnouncementPresenter.OnOpenWindowButtonClick), (Unit _) => !this._closingPresenter), delegate(Unit _)
			{
				this._clientButtonBiLogger.LogButtonClick(ButtonName.WelcomeFeaturesRanked);
			}), (Unit _) => this.Hide()), (Unit _) => this.OpenCompetitiveModeWindow()));
		}

		private void InitializeBattlepassSubPresenter()
		{
			BattlepassSeason battlepassSeason = this._getBattlepassSeason.Get();
			this._battlepassAnnouncementPresenter = new BattlepassSeasonAnnouncementPresenter(this._viewProvider, battlepassSeason, this._getBattlepassDateTranslation);
			this._battlepassAnnouncementPresenter.Initialize(this._announcementState.ShowBattlepass);
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Where<Unit>(Observable.First<Unit>(this._battlepassAnnouncementPresenter.OnOpenWindowButtonClick), (Unit _) => !this._closingPresenter), delegate(Unit _)
			{
				this._clientButtonBiLogger.LogButtonClick(ButtonName.WelcomeFeaturesBattlepass);
			}), (Unit _) => this.Hide()), (Unit _) => this.OpenBattlepassWindow()));
		}

		private IObservable<Unit> OpenBattlepassWindow()
		{
			return this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.BattlepassNode);
		}

		private IObservable<Unit> OpenCompetitiveModeWindow()
		{
			return this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.CompetitiveModeNode);
		}

		private void SetButtonsInteractable(bool interactable)
		{
			this._view.CloseButton.IsInteractable = interactable;
			this._competitiveAnnouncementPresenter.SetOpenWindowButtonInteractable(interactable);
			this._battlepassAnnouncementPresenter.SetOpenWindowButtonInteractable(interactable);
		}

		public IObservable<Unit> Show()
		{
			return Observable.AsUnitObservable<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_WelcomeFeatures"), delegate(Unit _)
			{
				this.InitializeView();
			}), delegate(Unit _)
			{
				this._clientBiLogger.BILogClientMsg(122, string.Empty, false);
			}), (Unit _) => this._view.ShowAnimation.Play()), delegate(Unit _)
			{
				this.SetButtonsInteractable(true);
				this._view.UiNavigationGroupHolder.AddGroup();
			}));
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._closingPresenter = true;
				this.SetButtonsInteractable(false);
				return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(this._view.HideAnimation.Play(), delegate(Unit _)
				{
					this._view.UiNavigationGroupHolder.RemoveGroup();
					this._hideSubject.OnNext(Unit.Default);
				}), (Unit _) => this.Dispose());
			});
		}

		public IObservable<Unit> Dispose()
		{
			return this._viewLoader.UnloadView("UI_ADD_WelcomeFeatures");
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private const string SceneName = "UI_ADD_WelcomeFeatures";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly IGetCurrentOrNextCompetitiveSeason _getCurrentOrNextCompetitiveSeason;

		private readonly IBattlepassDetailComponent _battlepassDetailComponent;

		private readonly ILocalizeKey _translation;

		private readonly IConsumeCurrentCompetitiveSeasonNews _consumeCurrentCompetitiveSeasonNews;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private readonly IConsumeCurrentBattlepassSeasonNews _consumeCurrentBattlepassSeasonNews;

		private readonly IGetBattlepassSeason _getBattlepassSeason;

		private readonly IGetBattlepassDateTranslation _getBattlepassDateTranslation;

		private readonly IClientBILogger _clientBiLogger;

		private readonly IClientButtonBILogger _clientButtonBiLogger;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private CompetitiveModeSeasonAnnouncementPresenter _competitiveAnnouncementPresenter;

		private BattlepassSeasonAnnouncementPresenter _battlepassAnnouncementPresenter;

		private ISeasonAnnouncementView _view;

		private AnnouncementState _announcementState;

		private bool _closingPresenter;

		private readonly Subject<Unit> _onOpenBattlepassRequested = new Subject<Unit>();
	}
}
