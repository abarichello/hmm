using System;
using System.Linq;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Extensions;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Divisions
{
	public class CompetitiveDivisionsPresenter : ICompetitiveDivisionsPresenter, IPresenter
	{
		public CompetitiveDivisionsPresenter(ILocalizeKey translation, IViewLoader viewLoader, IViewProvider viewProvider, IGetPlayerCompetitiveState getPlayerCompetitiveState, IGetCompetitiveDivisions getCompetitiveDivisions, IGetCurrentOrNextCompetitiveSeason getCurrentOrNextCompetitiveSeason, ICompetitiveDivisionsBadgeNameBuilder competitiveDivisionsBadgeNameBuilder, IMainMenuPresenterTree mainMenuPresenterTree)
		{
			this._getPlayerCompetitiveState = getPlayerCompetitiveState;
			this._translation = translation;
			this._viewLoader = viewLoader;
			this._viewProvider = viewProvider;
			this._getCompetitiveDivisions = getCompetitiveDivisions;
			this._getCurrentOrNextCompetitiveSeason = getCurrentOrNextCompetitiveSeason;
			this._competitiveDivisionsBadgeNameBuilder = competitiveDivisionsBadgeNameBuilder;
			this._mainMenuPresenterTree = mainMenuPresenterTree;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.AsUnitObservable<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(Observable.ContinueWith<Unit, PlayerCompetitiveState>(Observable.Do<Unit>(this._viewLoader.LoadView("UI_ADD_RankingLeague"), delegate(Unit _)
			{
				this.InitializeView();
			}), (Unit _) => this._getPlayerCompetitiveState.GetMine()), new Action<PlayerCompetitiveState>(this.InitializePlayerCompetitiveState)));
		}

		private void InitializeBackButton()
		{
			ObservableExtensions.Subscribe<Unit>(ButtonExtensions.OnClickNavigateBackwards(this._view.BackButton, this._mainMenuPresenterTree.PresenterTree));
		}

		private void InitializeDivisionsData()
		{
			this._divisions = this._getCompetitiveDivisions.Get();
			ICompetitiveDivisionView[] array = this._view.DivisionViews.ToArray<ICompetitiveDivisionView>();
			string format = this._translation.Get("RANKING_SCORE_ABBREVIATION", TranslationContext.Ranked);
			int num = this._divisions.Length - 1;
			for (int i = 0; i < num; i++)
			{
				Division division = this._divisions[i];
				string arg = string.Format("{0} - {1}", division.StartingScore, division.EndingScore);
				array[i].ScoreRangesLabel.Text = string.Format(format, arg);
			}
			Division division2 = this._divisions[num];
			string arg2 = string.Format("{0}+", division2.StartingScore);
			array[num].ScoreRangesLabel.Text = string.Format(format, arg2);
			CompetitiveSeason competitiveSeason = this._getCurrentOrNextCompetitiveSeason.Get();
			string text = this._translation.Get("RANKING_HEAVYMETAL_LEAGUE", TranslationContext.Ranked);
			this._view.DivisionTopRankingDescriptionLabel.Text = text;
			this._view.TopPlacementDescriptionLabel.Text = string.Format(this._view.TopPlacementDescriptionLabel.Text, competitiveSeason.TopPlayersCount);
			string format2 = this._translation.Get("RANKING_TOP_LEAGUE", TranslationContext.Ranked);
			this._view.TopPlacementDivisionView.ScoreRangesLabel.Text = string.Format(format2, competitiveSeason.TopPlayersCount);
			string format3 = this._translation.Get("RANKING_SUBLEAGUE_DESCRIPTION", TranslationContext.Ranked);
			this._view.SubdivisionsDescriptionLabel.Text = string.Format(format3, competitiveSeason.TopPlayersCount);
		}

		private void InitializePlayerCompetitiveState(PlayerCompetitiveState state)
		{
			this._playerState = state;
			this.InitializePlayerCompetitiveCalibration(state);
			this.InitializePlayerRank(state);
		}

		private void InitializePlayerCompetitiveCalibration(PlayerCompetitiveState state)
		{
			int totalRequiredMatches = state.CalibrationState.TotalRequiredMatches;
			int totalMatchesPlayed = state.CalibrationState.TotalMatchesPlayed;
			this._view.CalibrationTotalMatchesLabel.Text = string.Format("/ {0}", totalRequiredMatches);
			this._view.CalibrationMatchesPlayedIncompleteLabel.Text = totalMatchesPlayed.ToString();
			this._view.CalibrationMatchesPlayedCompletedLabel.Text = this._view.CalibrationMatchesPlayedIncompleteLabel.Text;
			this._view.CalibrationMatchesPlayedIncompleteLabel.IsActive = (state.Status != 2);
			this._view.CalibrationMatchesPlayedCompletedLabel.IsActive = (state.Status == 2);
			string format = this._translation.Get("RANKING_STATUS_DESCRIPTION", TranslationContext.Ranked);
			this._view.CalibrationDescriptionLabel.Text = string.Format(format, totalRequiredMatches);
		}

		private void InitializePlayerRank(PlayerCompetitiveState state)
		{
			if (state.Status != 2)
			{
				ICompetitiveDivisionView competitiveDivisionView = this._view.DivisionViews.ElementAt(0);
				competitiveDivisionView.Toggle.IsOn = true;
				ActivatableExtensions.Deactivate(this._view.TopPlacementPositionActivatable);
				return;
			}
			ICompetitiveDivisionView competitiveDivisionView2;
			if (state.Rank.CurrentRank.TopPlacementPosition != null)
			{
				ActivatableExtensions.Deactivate(this._view.SubdivisionsDescriptionActivatable);
				this._view.TopPlacementPositionLabel.Text = state.Rank.CurrentRank.TopPlacementPosition.Value.ToString();
				ActivatableExtensions.Activate(this._view.TopPlacementPositionActivatable);
				competitiveDivisionView2 = this._view.TopPlacementDivisionView;
			}
			else
			{
				ActivatableExtensions.Deactivate(this._view.TopPlacementPositionActivatable);
				competitiveDivisionView2 = this._view.DivisionViews.ElementAt(state.Rank.CurrentRank.Division);
				ICompetitiveSubdivisionView competitiveSubdivisionView = this._view.SubdivisionViews.ElementAt(state.Rank.CurrentRank.Subdivision);
				competitiveSubdivisionView.ArrowImage.IsActive = true;
			}
			competitiveDivisionView2.Toggle.IsOn = true;
			ActivatableExtensions.Activate(competitiveDivisionView2.PlayerDivisionIndicator);
			ObservableExtensions.Subscribe<Unit>(competitiveDivisionView2.Animation.Play());
		}

		private void InitializeView()
		{
			this._view = this._viewProvider.Provide<ICompetitiveDivisionsView>(null);
			this.InitializeTitle();
			this.InitializeBackButton();
			this.InitializeDivisionsData();
			this.InitializeDivisionsToggles();
		}

		private void InitializeDivisionsToggles()
		{
			int num = 0;
			foreach (ICompetitiveDivisionView competitiveDivisionView in this._view.DivisionViews)
			{
				this.UpdateSubdivisionsOnDivisionToggle(competitiveDivisionView.Toggle, num);
				num++;
			}
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.TopPlacementDivisionView.Toggle.OnToggleOn(), delegate(Unit _)
			{
				this.ShowTopPlacementDescription();
			}));
		}

		private void ShowTopPlacementDescription()
		{
			ActivatableExtensions.Deactivate(this._view.SubdivisionsDescriptionActivatable);
			ActivatableExtensions.Activate(this._view.TopPlacementDescriptionActivatable);
		}

		private void UpdateSubdivisionsOnDivisionToggle(IToggle divisionButton, int buttonIndex)
		{
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(divisionButton.OnToggleOn(), delegate(Unit _)
			{
				this.UpdateSubdivisions(buttonIndex);
			}));
		}

		private void InitializeTitle()
		{
			string title = this._translation.Get("RANKING_LEAGUE_TITLE_WINDOW", TranslationContext.Ranked);
			string description = this._translation.Get("RANKING_LEAGUE_DESCRIPTION_WINDOW", TranslationContext.Ranked);
			this._view.SetTitleInfo(title, string.Empty, description);
		}

		private void UpdateSubdivisions(int divisionIndex)
		{
			ActivatableExtensions.Activate(this._view.SubdivisionsDescriptionActivatable);
			ActivatableExtensions.Deactivate(this._view.TopPlacementDescriptionActivatable);
			int division = this._playerState.Rank.CurrentRank.Division;
			int subdivision = this._playerState.Rank.CurrentRank.Subdivision;
			Division division2 = this._divisions[divisionIndex];
			int num = division2.Subdivisions.Length;
			int num2 = 0;
			string[] array = new string[]
			{
				"V",
				"IV",
				"III",
				"II",
				"I"
			};
			string arg = this._translation.Get(division2.NameDraft, TranslationContext.Ranked);
			string format = this._translation.Get("RANKING_SCORE_ABBREVIATION", TranslationContext.Ranked);
			foreach (ICompetitiveSubdivisionView competitiveSubdivisionView in this._view.SubdivisionViews)
			{
				competitiveSubdivisionView.NameLabel.Text = string.Format("{0} {1}", arg, array[num2]);
				Subdivision subdivision2 = division2.Subdivisions[num2];
				string arg2;
				if (divisionIndex == this._divisions.Length - 1 && num2 == num - 1)
				{
					arg2 = string.Format("{0}+", subdivision2.StartingScore);
				}
				else
				{
					arg2 = string.Format("{0} - {1}", subdivision2.StartingScore, subdivision2.EndingScore);
				}
				competitiveSubdivisionView.RangeLabel.Text = string.Format(format, arg2);
				string subdivisionBadgeFileName = this._competitiveDivisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(division2, num2, 200);
				competitiveSubdivisionView.IconImage.SetImageName(subdivisionBadgeFileName);
				if (this.ShouldShowSubdivisionArrow())
				{
					competitiveSubdivisionView.ArrowImage.IsActive = (divisionIndex == division && num2 == subdivision);
				}
				num2++;
			}
		}

		private bool ShouldShowSubdivisionArrow()
		{
			return this._playerState.Status == 2 && this._playerState.Rank.CurrentRank.TopPlacementPosition == null;
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._view.MainCanvas.Enable();
				this._view.BackButton.IsInteractable = false;
				return Observable.Do<Unit>(Observable.Do<Unit>(this._view.ShowAnimation.Play(), delegate(Unit _)
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
			return Observable.Defer<Unit>(() => Observable.Do<Unit>(Observable.Do<Unit>(this._view.HideAnimation.Play(), delegate(Unit _)
			{
				this._view.UiNavigationGroupHolder.RemoveGroup();
			}), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
			}));
		}

		public IObservable<Unit> Dispose()
		{
			return this._viewLoader.UnloadView("UI_ADD_RankingLeague");
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private const string SceneName = "UI_ADD_RankingLeague";

		private readonly ILocalizeKey _translation;

		private readonly IGetPlayerCompetitiveState _getPlayerCompetitiveState;

		private readonly IViewLoader _viewLoader;

		private readonly IViewProvider _viewProvider;

		private ICompetitiveDivisionsView _view;

		private readonly IGetCompetitiveDivisions _getCompetitiveDivisions;

		private readonly IMainMenuPresenterTree _mainMenuPresenterTree;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private Division[] _divisions;

		private readonly IGetCurrentOrNextCompetitiveSeason _getCurrentOrNextCompetitiveSeason;

		private readonly ICompetitiveDivisionsBadgeNameBuilder _competitiveDivisionsBadgeNameBuilder;

		private PlayerCompetitiveState _playerState;
	}
}
