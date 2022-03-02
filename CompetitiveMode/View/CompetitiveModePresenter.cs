using System;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.CompetitiveMode.View.Matchmaking;
using HeavyMetalMachines.Frontend.Region;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Matchmaking.Configuration;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using HeavyMetalMachines.Presenting.Navigation;
using Hoplon.Localization;
using Hoplon.Localization.TranslationTable;
using Hoplon.Time;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View
{
	public class CompetitiveModePresenter : ICompetitiveModePresenter, IPresenter
	{
		public CompetitiveModePresenter(IViewLoader viewLoader, IViewProvider viewProvider, IGetCurrentOrNextCompetitiveSeason getCurrentOrNextSeason, ILocalizeKey translation, IGetThenObserveCompetitiveQueueConfiguration getThenObserveQueueConfiguration, ICurrentTime currentTime, ILocalizeDateTime localizeDateTime, ICompetitiveQueueJoinPresenter competitiveQueueJoinPresenter, IMainMenuPresenterTree mainMenuPresenterTree, IRegionStatusPresenter regionStatusPresenter, CompetitiveModeTreeBranch treeBranch)
		{
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._getCurrentOrNextSeason = getCurrentOrNextSeason;
			this._translation = translation;
			this._getThenObserveQueueConfiguration = getThenObserveQueueConfiguration;
			this._currentTime = currentTime;
			this._localizeDateTime = localizeDateTime;
			this._competitiveQueueJoinPresenter = competitiveQueueJoinPresenter;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
			this._regionStatusPresenter = regionStatusPresenter;
			this._treeBranch = treeBranch;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_RankingInfo"), delegate(Unit _)
			{
				this.InitializeView();
			});
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<ICompetitiveModeView>(null);
			this._presenterButtonsClick = new CompositeDisposable();
			this._view.MainCanvas.Enable();
			this.InitializeTitle();
			this.InitializeBackButton();
			this.InitializeSeasonInformation();
			this.InitializeJoinPresenter();
			this.InitializeRegionStatusPresenter();
			this.StartObservingQueueConfiguration();
		}

		private void InitializeTitle()
		{
			string title = this._translation.Get("RANKING_RANKING_TITLE_WINDOW", TranslationContext.Ranked);
			string description = this._translation.Get("RANKING_SUBTITLE_WINDOW", TranslationContext.Ranked);
			this._view.Title.Title = title;
			this._view.Title.Description = description;
			ActivatableExtensions.Deactivate(this._view.Title.SubtitleActivatable);
			ActivatableExtensions.Activate(this._view.Title.DescriptionActivatable);
			ActivatableExtensions.Deactivate(this._view.Title.InfoButton);
		}

		private void InitializeBackButton()
		{
			ObservableExtensions.Subscribe<Unit>(ButtonExtensions.OnClickNavigateBackwards(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree));
		}

		private void RegisterPresentersButtonsClicks()
		{
			this.OpenPresenterOnClick(this._view.OpenQueuePeriodsButton, this._treeBranch.QueuePeriodsNode);
			this.OpenPresenterOnClick(this._view.OpenDivisionsButton, this._treeBranch.DivisionsNode);
			this.OpenPresenterOnClick(this._view.OpenRewardsButton, this._treeBranch.RewardsNode);
			this.OpenPresenterOnClick(this._view.OpenRankingButton, this._treeBranch.RankingNode);
		}

		private void OpenPresenterOnClick(IButton button, IPresenterNode presenterNode)
		{
			IDisposable disposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.SelectMany<Unit, Unit>(Observable.Do<Unit>(button.OnClick(), delegate(Unit _)
			{
				this.SetButtonsInteractable(false);
			}), (Unit _) => this._mainMenuPresenterTree.PresenterTree.NavigateToNode(presenterNode)), delegate(Unit _)
			{
				this.SetButtonsInteractable(true);
			}));
			this._presenterButtonsClick.Add(disposable);
		}

		private void InitializeSeasonInformation()
		{
			CompetitiveSeason competitiveSeason = this._getCurrentOrNextSeason.Get();
			string format = this._translation.Get("RANKING_SEASON_NUMBER", TranslationContext.Ranked);
			this._view.SeasonNameLabel.Text = string.Format(format, competitiveSeason.Id);
			DateTime dateTime = competitiveSeason.StartDateTime.ToLocalTime();
			this._view.SeasonStartDateLabel.Text = LocalizationExtensions.GetShortDateString(this._localizeDateTime, dateTime);
			this._view.SeasonStartTimeLabel.Text = LocalizationExtensions.GetShortTimeString(this._localizeDateTime, dateTime);
			DateTime dateTime2 = competitiveSeason.EndDateTime.ToLocalTime();
			this._view.SeasonEndDateLabel.Text = LocalizationExtensions.GetShortDateString(this._localizeDateTime, dateTime2);
			this._view.SeasonEndTimeLabel.Text = LocalizationExtensions.GetShortTimeString(this._localizeDateTime, dateTime2);
			this._view.InformationLabel.Text = string.Empty;
		}

		private void InitializeJoinPresenter()
		{
			this._competitiveQueueJoinPresenter.ViewProviderContext = UnityCompetitiveQueueJoinView.ViewProviderContext;
			this._competitiveQueueJoinPresenter.Initialize();
		}

		private void InitializeRegionStatusPresenter()
		{
			this._regionStatusPresenter.Initialize();
		}

		private void StartObservingQueueConfiguration()
		{
			this._queueConfigurationObservation = ObservableExtensions.Subscribe<QueueConfiguration>(Observable.Do<QueueConfiguration>(Observable.Do<QueueConfiguration>(this._getThenObserveQueueConfiguration.GetThenObserve(), new Action<QueueConfiguration>(this.InitializeNextQueuePeriod)), new Action<QueueConfiguration>(this.InitializeCompetitiveModeInformation)));
		}

		private void InitializeCompetitiveModeInformation(QueueConfiguration queueConfiguration)
		{
			string arg = this._translation.Get("RANKED_INFO_GENERAL", TranslationContext.Ranked);
			string[] arenasNames = this.GetArenasNames(queueConfiguration.AvailableArenas);
			string arg2 = string.Join(", ", arenasNames);
			string text = this._translation.Get("RANKED_INFO_ROTATIONARENAS", TranslationContext.Ranked);
			text = string.Format(text, arg2);
			string arg3;
			if (queueConfiguration.LockedCharacters.Length > 0)
			{
				string[] charactersNames = this.GetCharactersNames(queueConfiguration.LockedCharacters);
				string text2 = string.Join(", ", charactersNames);
				arg3 = this._translation.GetFormatted("RANKED_INFO_BANNEDCHARACTERS", TranslationContext.Ranked, new object[]
				{
					text2
				});
			}
			else
			{
				arg3 = this._translation.Get("RANKED_INFO_CHARACTERS", TranslationContext.Ranked);
			}
			string text3 = string.Format("{0}\n{1}\n{2}", arg, text, arg3);
			this._view.InformationLabel.Text = text3;
		}

		private string[] GetArenasNames(QueueConfigArenaData[] arenasData)
		{
			string[] array = new string[arenasData.Length];
			for (int i = 0; i < arenasData.Length; i++)
			{
				array[i] = this._translation.Get(arenasData[i].NameDraft, TranslationContext.MainMenuGui);
			}
			return array;
		}

		private string[] GetCharactersNames(QueueConfigCharacterData[] charactersData)
		{
			string[] array = new string[charactersData.Length];
			for (int i = 0; i < charactersData.Length; i++)
			{
				array[i] = this._translation.Get(charactersData[i].NameDraft, TranslationContext.CharactersBaseInfo);
			}
			return array;
		}

		private void InitializeNextQueuePeriod(QueueConfiguration queueConfiguration)
		{
			DateTime dateTime = queueConfiguration.GetCurrentOrNextPeriod(this._currentTime).OpenDateTimeUtc.ToLocalTime();
			this._view.NextQueuePeriodOpenDateLabel.Text = LocalizationExtensions.GetShortDateString(this._localizeDateTime, dateTime);
			this._view.NextQueuePeriodOpenTimeLabel.Text = LocalizationExtensions.GetShortTimeString(this._localizeDateTime, dateTime);
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.SetButtonsInteractable(false);
				this.RegisterPresentersButtonsClicks();
				return Observable.Do<Unit>(this._view.ShowAnimation.Play(), delegate(Unit _)
				{
					this.SetButtonsInteractable(true);
					this._view.UiNavigationGroupHolder.AddGroup();
				});
			});
		}

		private void SetButtonsInteractable(bool interactable)
		{
			this._view.BackButton.IsInteractable = interactable;
			this._view.OpenDivisionsButton.IsInteractable = interactable;
			this._view.OpenRankingButton.IsInteractable = interactable;
			this._view.OpenRewardsButton.IsInteractable = interactable;
			this._view.OpenQueuePeriodsButton.IsInteractable = interactable;
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._presenterButtonsClick.Dispose();
				this.SetButtonsInteractable(false);
				return Observable.Do<Unit>(this._view.HideAnimation.Play(), delegate(Unit _)
				{
					this._view.UiNavigationGroupHolder.RemoveGroup();
					this._hideSubject.OnNext(Unit.Default);
				});
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._queueConfigurationObservation.Dispose();
			}), delegate(Unit _)
			{
				this._competitiveQueueJoinPresenter.Dispose();
			}), delegate(Unit _)
			{
				this._regionStatusPresenter.Dispose();
			}), (Unit _) => this._viewLoader.UnloadView("UI_ADD_RankingInfo"));
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private const string SceneName = "UI_ADD_RankingInfo";

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly IGetCurrentOrNextCompetitiveSeason _getCurrentOrNextSeason;

		private readonly IGetThenObserveCompetitiveQueueConfiguration _getThenObserveQueueConfiguration;

		private readonly ILocalizeKey _translation;

		private readonly ICompetitiveQueueJoinPresenter _competitiveQueueJoinPresenter;

		private readonly ICurrentTime _currentTime;

		private readonly ILocalizeDateTime _localizeDateTime;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private readonly CompetitiveModeTreeBranch _treeBranch;

		private readonly IRegionStatusPresenter _regionStatusPresenter;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private CompositeDisposable _presenterButtonsClick;

		private ICompetitiveModeView _view;

		private IDisposable _queueConfigurationObservation;
	}
}
