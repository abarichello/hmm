using System;
using System.Linq;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Ranking;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public class CompetitiveRankingPresenter : ICompetitiveRankingPresenter, IPresenter
	{
		public CompetitiveRankingPresenter(IViewLoader viewLoader, IViewProvider viewProvider, ICompetitiveRankingListPresenter globalRankingPresenter, ICompetitiveRankingListPresenter friendsRankingPresenter, IFetchPlayerCompetitiveState fetchPlayerCompetitiveState, ILocalizeKey translation, IGetCompetitiveGlobalRanking getCompetitiveGlobalRanking, IGetCompetitiveFriendsRanking getCompetitiveFriendsRanking, IGetCompetitiveDivisions getCompetitiveDivisions, ICompetitiveDivisionsBadgeNameBuilder competitiveDivisionsBadgeNameBuilder, IMainMenuPresenterTree mainMenuPresenterTree)
		{
			CompetitiveRankingPresenter.AssertValidConstructorParameters(viewLoader, viewProvider, globalRankingPresenter, friendsRankingPresenter, fetchPlayerCompetitiveState, translation, getCompetitiveGlobalRanking, getCompetitiveFriendsRanking, competitiveDivisionsBadgeNameBuilder, mainMenuPresenterTree);
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._globalRankingPresenter = globalRankingPresenter;
			this._friendsRankingPresenter = friendsRankingPresenter;
			this._fetchPlayerCompetitiveState = fetchPlayerCompetitiveState;
			this._translation = translation;
			this._getCompetitiveGlobalRanking = getCompetitiveGlobalRanking;
			this._getCompetitiveFriendsRanking = getCompetitiveFriendsRanking;
			this._getCompetitiveDivisions = getCompetitiveDivisions;
			this._competitiveDivisionsBadgeNameBuilder = competitiveDivisionsBadgeNameBuilder;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
		}

		private static void AssertValidConstructorParameters(IViewLoader viewLoader, IViewProvider viewProvider, ICompetitiveRankingListPresenter globalRankingPresenter, ICompetitiveRankingListPresenter friendsRankingPresenter, IFetchPlayerCompetitiveState fetchPlayerCompetitiveState, ILocalizeKey translation, IGetCompetitiveGlobalRanking getCompetitiveGlobalRanking, IGetCompetitiveFriendsRanking getCompetitiveFriendsRanking, ICompetitiveDivisionsBadgeNameBuilder competitiveDivisionsBadgeNameBuilder, IMainMenuPresenterTree mainMenuPresenterTree)
		{
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ReturnUnit(), (Unit _) => this._viewLoader.LoadView("UI_ADD_RankingList")), delegate(Unit _)
			{
				this.InitializeView();
			}), delegate(Unit _)
			{
				this.StartFetchingPlayerCompetitiveState();
			}), (Unit _) => this.InitializeInitialListPresenter());
		}

		private void StartFetchingPlayerCompetitiveState()
		{
			this._searchDisposable = ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<Unit, Unit>(Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				Observable.ReturnUnit(),
				this._globalRankingPresenter.OnListRefreshed,
				this._friendsRankingPresenter.OnListRefreshed
			}), Observable.AsUnitObservable<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(this._fetchPlayerCompetitiveState.FetchMine(), new Action<PlayerCompetitiveState>(this.InitializePlayerInformation)))));
		}

		private void InitializePlayerInformation(PlayerCompetitiveState playerCompetitiveState)
		{
			if (playerCompetitiveState.Status == 2)
			{
				this.InitializePlayerRankInformation(playerCompetitiveState);
			}
			else
			{
				this.InitializePlayerCalibrationInformation(playerCompetitiveState);
			}
		}

		private void InitializePlayerRankInformation(PlayerCompetitiveState playerCompetitiveState)
		{
			ActivatableExtensions.Deactivate(this._view.CalibratingStateGroup);
			ActivatableExtensions.Activate(this._view.DivisionGroup);
			CompetitiveRank currentRank = playerCompetitiveState.Rank.CurrentRank;
			this._view.RankScoreLabel.Text = this._translation.GetFormatted("RANKING_SCORE_ABBREVIATION", TranslationContext.Ranked, new object[]
			{
				currentRank.Score.ToString()
			});
			this._view.DivisionNameLabel.Text = this._competitiveDivisionsBadgeNameBuilder.GetDivisionWithSubdivisionNameTranslated(currentRank);
			if (currentRank.TopPlacementPosition != null)
			{
				this._view.TopPositionLabel.IsActive = true;
				this._view.TopPositionLabel.Text = currentRank.TopPlacementPosition.Value.ToString();
			}
			else
			{
				this._view.TopPositionLabel.IsActive = false;
			}
			Division[] array = this._getCompetitiveDivisions.Get();
			int index = (currentRank.TopPlacementPosition == null) ? currentRank.Division : array.Length;
			IRankBadgeComponents rankBadgeComponents = this._view.RanksBadgesComponents.ElementAt(index);
			ActivatableExtensions.Activate(rankBadgeComponents.Group);
			foreach (IRankBadgeComponents rankBadgeComponents2 in this._view.RanksBadgesComponents)
			{
				if (rankBadgeComponents2 != rankBadgeComponents)
				{
					ActivatableExtensions.Deactivate(rankBadgeComponents2.Group);
				}
			}
			string divisionBadgeFileName = this.GetDivisionBadgeFileName(currentRank, array);
			rankBadgeComponents.SubleagueDynamicImage.SetImageName(divisionBadgeFileName);
			ObservableExtensions.Subscribe<Unit>(rankBadgeComponents.BadgeAnimation.Play());
		}

		private string GetDivisionBadgeFileName(CompetitiveRank rank, Division[] divisions)
		{
			if (rank.TopPlacementPosition != null)
			{
				return this._competitiveDivisionsBadgeNameBuilder.GetTopDivisionBadgeFileName(700);
			}
			int division = rank.Division;
			int subdivision = rank.Subdivision;
			Division division2 = divisions[division];
			return this._competitiveDivisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(division2, subdivision, 700);
		}

		private void InitializePlayerCalibrationInformation(PlayerCompetitiveState playerCompetitiveState)
		{
			ActivatableExtensions.Activate(this._view.CalibratingStateGroup);
			int totalRequiredMatches = playerCompetitiveState.CalibrationState.TotalRequiredMatches;
			int totalMatchesPlayed = playerCompetitiveState.CalibrationState.TotalMatchesPlayed;
			this._view.CalibrationTotalMatchesLabel.Text = string.Format("/ {0}", totalRequiredMatches);
			this._view.CalibrationMatchesPlayedLabel.Text = totalMatchesPlayed.ToString();
		}

		private IObservable<Unit> InitializeInitialListPresenter()
		{
			this._view.Title.Subtitle = this._globalSubtitle;
			this._currentListPresenter = this._globalRankingPresenter;
			return this._currentListPresenter.Initialize();
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<ICompetitiveRankingView>(null);
			this._view.ShowGlobalToggle.IsOn = true;
			this._view.ShowGlobalToggle.IsInteractable = false;
			this.InitializeBackButton();
			this.InitializeGlobalToggleButton();
			this.InitializeFriendsToggleButton();
			this.InitializeTitle();
		}

		private void InitializeBackButton()
		{
			ObservableExtensions.Subscribe<Unit>(ButtonExtensions.OnClickNavigateBackwards(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree));
		}

		private void InitializeGlobalToggleButton()
		{
			this._view.ShowGlobalToggleLabel.Text = this._translation.GetFormatted("RANKING_TOP_BUTTON", TranslationContext.Ranked, new object[]
			{
				30
			});
			this._globalRankingPresenter.RankingPlayersCount = 30;
			this._globalRankingPresenter.GetRankings = new Func<int, IObservable<PlayerCompetitiveRankingPosition[]>>(this._getCompetitiveGlobalRanking.Get);
			ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<Unit, Unit>(Observable.Do<Unit>(this._view.ShowGlobalToggle.OnToggleOn(), delegate(Unit _)
			{
				this._view.Title.Subtitle = this._globalSubtitle;
			}), (Unit _) => this.ChangeCurrentListPresenter(this._globalRankingPresenter)));
		}

		private void InitializeFriendsToggleButton()
		{
			this._view.ShowFriendsToggleLabel.Text = this._translation.Get("RANKING_FRIENDS_BUTTON", TranslationContext.Ranked);
			this._friendsRankingPresenter.RankingPlayersCount = 100;
			this._friendsRankingPresenter.GetRankings = new Func<int, IObservable<PlayerCompetitiveRankingPosition[]>>(this._getCompetitiveFriendsRanking.Get);
			ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<Unit, Unit>(Observable.Do<Unit>(this._view.ShowFriendsToggle.OnToggleOn(), delegate(Unit _)
			{
				this._view.Title.Subtitle = this._friendsSubtitle;
			}), (Unit _) => this.ChangeCurrentListPresenter(this._friendsRankingPresenter)));
		}

		private void InitializeTitle()
		{
			ActivatableExtensions.Deactivate(this._view.Title.InfoButton);
			ActivatableExtensions.Activate(this._view.Title.SubtitleActivatable);
			ActivatableExtensions.Deactivate(this._view.Title.DescriptionActivatable);
			this._view.Title.Title = this._translation.Get("RANKING_TITLE_WINDOW", TranslationContext.Ranked);
			this._globalSubtitle = this._translation.GetFormatted("RANKING_SUBTITLE_TOPS_WINDOW", TranslationContext.Ranked, new object[]
			{
				30
			});
			this._friendsSubtitle = this._translation.Get("RANKING_SUBTITLE_FRIENDS_WINDOW", TranslationContext.Ranked);
		}

		private void SetTogglesInteractable(bool interactable)
		{
			this._view.ShowFriendsToggle.IsInteractable = (interactable && this._currentListPresenter != this._friendsRankingPresenter);
			this._view.ShowGlobalToggle.IsInteractable = (interactable && this._currentListPresenter != this._globalRankingPresenter);
		}

		private IObservable<Unit> ChangeCurrentListPresenter(ICompetitiveRankingListPresenter rankingListPresenter)
		{
			return Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.Defer<Unit>(delegate()
			{
				this.SetTogglesInteractable(false);
				return this._currentListPresenter.Hide();
			}), delegate(Unit _)
			{
				this._currentListPresenter.Dispose();
			}), delegate(Unit _)
			{
				this._currentListPresenter = rankingListPresenter;
			}), (Unit _) => this._currentListPresenter.Initialize()), (Unit _) => this._currentListPresenter.Show()), delegate(Unit _)
			{
				this.SetTogglesInteractable(true);
			});
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.MainCanvas.Enable();
				return Observable.Do<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
				{
					this._view.BackButton.IsInteractable = false;
				}), (Unit _) => this._view.FadeInAnimation.Play()), delegate(Unit _)
				{
					this.SetTogglesInteractable(true);
				}), delegate(Unit _)
				{
					this._view.BackButton.IsInteractable = true;
				}), delegate(Unit _)
				{
					this._view.UiNavigationGroupHolder.AddGroup();
				});
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Defer<Unit>(() => Observable.Do<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(this._currentListPresenter.Hide(), delegate(Unit _)
			{
				this.SetTogglesInteractable(false);
			}), (Unit _) => this._view.FadeOutAnimation.Play()), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
			}), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
			}));
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._searchDisposable.Dispose();
				this._currentListPresenter.Dispose();
				return this._viewLoader.UnloadView("UI_ADD_RankingList");
			});
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private const int GlobalRankingPlayerCount = 30;

		private const int FriendRankingPlayerCount = 100;

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private readonly ICompetitiveRankingListPresenter _globalRankingPresenter;

		private readonly ICompetitiveRankingListPresenter _friendsRankingPresenter;

		private readonly IFetchPlayerCompetitiveState _fetchPlayerCompetitiveState;

		private readonly IGetCompetitiveGlobalRanking _getCompetitiveGlobalRanking;

		private readonly IGetCompetitiveFriendsRanking _getCompetitiveFriendsRanking;

		private readonly ILocalizeKey _translation;

		private readonly IGetCompetitiveDivisions _getCompetitiveDivisions;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private readonly ICompetitiveDivisionsBadgeNameBuilder _competitiveDivisionsBadgeNameBuilder;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private ICompetitiveRankingView _view;

		private ICompetitiveRankingListPresenter _currentListPresenter;

		private string _globalSubtitle;

		private string _friendsSubtitle;

		private IDisposable _searchDisposable;
	}
}
