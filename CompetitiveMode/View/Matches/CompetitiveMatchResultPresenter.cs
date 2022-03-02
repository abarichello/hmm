using System;
using System.Linq;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using Hoplon.Localization.TranslationTable;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class CompetitiveMatchResultPresenter : ICompetitiveMatchResultPresenter, IPresenter
	{
		public CompetitiveMatchResultPresenter(IViewProvider viewProvider, IWaitAndGetMyPlayerCompetitiveStateProgress waitAndGetMyPlayerCompetitiveStateProgress, IGetCompetitiveDivisions getCompetitiveDivisions, ILocalizeKey translation, ICompetitiveDivisionsBadgeNameBuilder divisionsBadgeNameBuilder, IGetCurrentOrNextCompetitiveSeason getCurrentOrNextCompetitiveSeason)
		{
			this._viewProvider = viewProvider;
			this._waitAndGetMyPlayerCompetitiveStateProgress = waitAndGetMyPlayerCompetitiveStateProgress;
			this._getCompetitiveDivisions = getCompetitiveDivisions;
			this._translation = translation;
			this._divisionsBadgeNameBuilder = divisionsBadgeNameBuilder;
			this._getCurrentOrNextCompetitiveSeason = getCurrentOrNextCompetitiveSeason;
		}

		public IObservable<Unit> Initialize()
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.ReturnUnit(), (Unit _) => this.InitializeView());
		}

		private IObservable<Unit> InitializeView()
		{
			this._view = this._viewProvider.Provide<ICompetitiveMatchResultView>(null);
			this._divisions = this._getCompetitiveDivisions.Get();
			return Observable.AsUnitObservable<PlayerCompetitiveProgress>(Observable.Do<PlayerCompetitiveProgress>(this._waitAndGetMyPlayerCompetitiveStateProgress.WaitAndGet(), delegate(PlayerCompetitiveProgress progress)
			{
				this._playerCompetitiveProgress = progress;
			}));
		}

		public IObservable<Unit> Show()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(this.GetCalibrationAnimation(), (Unit _) => this.GetRankedAnimation()), (Unit _) => this.Hide()), (Unit _) => this.Dispose()));
			return Observable.ReturnUnit();
		}

		private IObservable<Unit> GetCalibrationAnimation()
		{
			IObservable<Unit> observable = this._view.ShowAnimation.Play();
			if (this._playerCompetitiveProgress.InitialState.Status != 1)
			{
				return observable;
			}
			this.InitializeCalibrationInitialState();
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Merge<Unit>(observable, new IObservable<Unit>[]
			{
				this.PlayMatchesAnimations()
			}), (Unit _) => this._view.CalibrationView.ShowButtonAnimation.Play()), delegate(Unit _)
			{
				this._view.CalibrationView.UiNavigationGroupHolder.AddGroup();
			}), (Unit _) => Observable.First<Unit>(this._view.CalibrationView.ContinueButton.OnClick())), delegate(Unit _)
			{
				this._view.CalibrationView.UiNavigationGroupHolder.RemoveGroup();
			}), (Unit _) => Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this.PlayCalibrationMatchesHidingAnimation(),
				this.PlayCalibrationHideAnimation()
			}));
		}

		private IObservable<Unit> PlayCalibrationMatchesHidingAnimation()
		{
			return Observable.Merge<Unit>(from view in this._view.CalibrationView.MatchesViews
			select view.HideAnimation.Play());
		}

		private IObservable<Unit> PlayCalibrationHideAnimation()
		{
			if (this._playerCompetitiveProgress.FinalState.Status == 1)
			{
				return Observable.Merge<Unit>(this._view.CalibrationView.HideAnimation.Play(), new IObservable<Unit>[]
				{
					this._view.HideAnimation.Play()
				});
			}
			return this._view.CalibrationView.TransitionAnimation.Play();
		}

		private void InitializeCalibrationInitialState()
		{
			CompetitiveSeason competitiveSeason = this._getCurrentOrNextCompetitiveSeason.Get();
			string format = this._translation.Get("RANKING_SEASON_NUMBER", TranslationContext.Ranked);
			this._view.CalibrationView.SeasonNameLabel.Text = string.Format(format, competitiveSeason.Id);
			ActivatableExtensions.Activate(this._view.CalibrationView.Group);
			PlayerCompetitiveCalibrationState calibrationState = this._playerCompetitiveProgress.InitialState.CalibrationState;
			int num = calibrationState.TotalMatchesPlayed;
			if (this._playerCompetitiveProgress.FinalState.Status != 1)
			{
				num = 0;
			}
			this._view.CalibrationView.MatchesPlayedLabel.Text = num.ToString();
			this._view.CalibrationView.TotalMatchesLabel.Text = string.Format("/ {0}", calibrationState.TotalRequiredMatches);
			ICompetitiveMatchResultCalibrationMatchView[] array = this._view.CalibrationView.MatchesViews.ToArray<ICompetitiveMatchResultCalibrationMatchView>();
			bool[] matchesResults = calibrationState.MatchesResults;
			for (int i = 0; i < num; i++)
			{
				ICompetitiveMatchResultCalibrationMatchView competitiveMatchResultCalibrationMatchView = array[i];
				if (matchesResults[i])
				{
					ActivatableExtensions.Activate(competitiveMatchResultCalibrationMatchView.MatchWonGroup);
					competitiveMatchResultCalibrationMatchView.SetWonImage();
				}
				else
				{
					ActivatableExtensions.Activate(competitiveMatchResultCalibrationMatchView.MatchLostGroup);
					competitiveMatchResultCalibrationMatchView.SetLostImage();
				}
			}
		}

		private IObservable<Unit> PlayMatchesAnimations()
		{
			PlayerCompetitiveCalibrationState calibrationState = this._playerCompetitiveProgress.InitialState.CalibrationState;
			PlayerCompetitiveCalibrationState calibrationState2 = this._playerCompetitiveProgress.FinalState.CalibrationState;
			int num = calibrationState.TotalMatchesPlayed;
			int num2 = calibrationState2.TotalMatchesPlayed;
			if (this._playerCompetitiveProgress.FinalState.Status != 1)
			{
				num = 0;
				num2 = calibrationState.TotalRequiredMatches;
			}
			ICompetitiveMatchResultCalibrationMatchView[] array = this._view.CalibrationView.MatchesViews.ToArray<ICompetitiveMatchResultCalibrationMatchView>();
			bool[] matchesResults = calibrationState2.MatchesResults;
			IObservable<Unit>[] array2 = new IObservable<Unit>[num];
			for (int i = 0; i < num; i++)
			{
				ICompetitiveMatchResultCalibrationMatchView competitiveMatchResultCalibrationMatchView = array[i];
				array2[i] = competitiveMatchResultCalibrationMatchView.ShowAnimation.Play();
			}
			IObservable<Unit> observable = this._view.CalibrationView.ShowAnimation.Play();
			observable = Observable.Merge<Unit>(observable, array2);
			for (int j = num; j < num2; j++)
			{
				ICompetitiveMatchResultCalibrationMatchView view = array[j];
				int matchCount = j + 1;
				IObservable<Unit> calibrationMatchViewAnimation = this.GetCalibrationMatchViewAnimation(view, matchCount, matchesResults[j]);
				observable = Observable.ContinueWith<Unit, Unit>(observable, calibrationMatchViewAnimation);
			}
			return observable;
		}

		private IObservable<Unit> GetCalibrationMatchViewAnimation(ICompetitiveMatchResultCalibrationMatchView view, int matchCount, bool wasVictory)
		{
			if (wasVictory)
			{
				return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Delay<Unit>(Observable.ReturnUnit(), TimeSpan.FromMilliseconds((double)this._view.CalibrationView.MillisecondsBetweenAnimationsOfMatches)), delegate(Unit _)
				{
					view.WonAlpha.Alpha = 0f;
					ActivatableExtensions.Activate(view.MatchWonGroup);
					view.SetWonImage();
				}), (Unit _) => this.PlayMatchResultRaisingAnimation(view, matchCount));
			}
			view.LostAlpha.Alpha = 0f;
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Delay<Unit>(Observable.ReturnUnit(), TimeSpan.FromMilliseconds((double)this._view.CalibrationView.MillisecondsBetweenAnimationsOfMatches)), delegate(Unit _)
			{
				view.LostAlpha.Alpha = 0f;
				ActivatableExtensions.Activate(view.MatchLostGroup);
				view.SetLostImage();
			}), (Unit _) => this.PlayMatchResultRaisingAnimation(view, matchCount));
		}

		private IObservable<Unit> PlayMatchResultRaisingAnimation(ICompetitiveMatchResultCalibrationMatchView view, int matchCount)
		{
			return Observable.AsUnitObservable<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._view.CalibrationView.MatchesPlayedLabel.Text = matchCount.ToString();
				ObservableExtensions.Subscribe<Unit>(view.RaiseAnimation.Play());
			}));
		}

		private IObservable<Unit> GetRankedAnimation()
		{
			if (this._playerCompetitiveProgress.FinalState.Status != 2)
			{
				return Observable.ReturnUnit();
			}
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.InitializeRankedView();
			}), (Unit _) => this.PlayRankedAnimation()), (Unit _) => this._view.RankedView.ShowButtonAnimation.Play()), delegate(Unit _)
			{
				this._view.RankedView.UiNavigationGroupHolder.AddGroup();
			}), (Unit _) => Observable.First<Unit>(this._view.RankedView.ContinueButton.OnClick())), delegate(Unit _)
			{
				this._view.RankedView.UiNavigationGroupHolder.RemoveGroup();
			}), delegate(Unit _)
			{
				this.StopIdleAudio();
			}), (Unit _) => Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this._view.RankedView.HideAnimation.Play(),
				this._view.HideAnimation.Play()
			}));
		}

		private void StopIdleAudio()
		{
			if (this.FinishedAtTopPlacement())
			{
				int num = this._view.RankedView.DivisionsIdleAudios.Length - 1;
				this._view.RankedView.DivisionsIdleAudios[num].StopAll();
			}
			int division = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Division;
			this._view.RankedView.DivisionsIdleAudios[division].StopAll();
		}

		private IObservable<Unit> PlayRankedAnimation()
		{
			ICompetitiveMatchResultRankedDivisionView divisionView = this.GetDivisionView();
			if (this.LeftCalibration())
			{
				return this.PlayRankChangeAnimation(divisionView, Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
				{
					ActivatableExtensions.Activate(this._view.CalibrationView.LeavingCalibrationAnimationGroup);
				}), divisionView.DivisionUpAnimation.Play()));
			}
			if (this.WillAnimateScoreFilling())
			{
				return this.PlayUnchangedRankAnimation(divisionView);
			}
			IAnimation rankChangeAnimation = this.GetRankChangeAnimation(divisionView);
			return this.PlayRankChangeAnimation(divisionView, rankChangeAnimation.Play());
		}

		private IAnimation GetRankChangeAnimation(ICompetitiveMatchResultRankedDivisionView divisionView)
		{
			if (this.RankedDivisionUp() || this.EnteredTopPlacement())
			{
				return divisionView.DivisionUpAnimation;
			}
			if (this.RankedDivisionDown() || this.LeftTopPlacement())
			{
				return divisionView.DivisionDownAnimation;
			}
			if (this.TopPlacementUp() || this.RankedSubdivisionUp())
			{
				return divisionView.SubdivisionUpAnimation;
			}
			return divisionView.SubdivisionDownAnimation;
		}

		private bool DivisionOrSubdivisionChanged()
		{
			return this.RankedDivisionChanged() || this.RankedSubdivisionChanged();
		}

		private bool RankedDivisionChanged()
		{
			return this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Division != this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Division;
		}

		private bool RankedDivisionUp()
		{
			return this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Division > this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Division;
		}

		private bool RankedDivisionDown()
		{
			return this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Division < this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Division;
		}

		private bool RankedSubdivisionChanged()
		{
			return this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Division == this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Division && this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Subdivision != this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Subdivision;
		}

		private bool RankedSubdivisionUp()
		{
			return this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Division == this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Division && this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Subdivision > this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Subdivision;
		}

		private bool RankedSubdivisionDown()
		{
			return this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Division == this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Division && this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Subdivision < this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Subdivision;
		}

		private bool IsTopPlacementInvolved()
		{
			return this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.TopPlacementPosition != null || this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.TopPlacementPosition != null;
		}

		private bool FinishedAtTopPlacement()
		{
			return this._playerCompetitiveProgress.FinalState.Status == 2 && this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.TopPlacementPosition != null;
		}

		private bool EnteredTopPlacement()
		{
			return this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.TopPlacementPosition == null && this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.TopPlacementPosition != null;
		}

		private bool LeftTopPlacement()
		{
			return this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.TopPlacementPosition != null && this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.TopPlacementPosition == null;
		}

		private bool TopPlacementChanged()
		{
			int? topPlacementPosition = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.TopPlacementPosition;
			int valueOrDefault = topPlacementPosition.GetValueOrDefault();
			int? topPlacementPosition2 = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.TopPlacementPosition;
			return valueOrDefault != topPlacementPosition2.GetValueOrDefault() || (topPlacementPosition != null ^ topPlacementPosition2 != null);
		}

		private bool TopPlacementUp()
		{
			bool result;
			if (this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.TopPlacementPosition != null && this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.TopPlacementPosition != null)
			{
				int? topPlacementPosition = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.TopPlacementPosition;
				bool flag = topPlacementPosition != null;
				int? topPlacementPosition2 = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.TopPlacementPosition;
				result = ((flag & topPlacementPosition2 != null) && topPlacementPosition.GetValueOrDefault() < topPlacementPosition2.GetValueOrDefault());
			}
			else
			{
				result = false;
			}
			return result;
		}

		private bool TopPlacementDown()
		{
			bool result;
			if (this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.TopPlacementPosition != null && this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.TopPlacementPosition != null)
			{
				int? topPlacementPosition = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.TopPlacementPosition;
				bool flag = topPlacementPosition != null;
				int? topPlacementPosition2 = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.TopPlacementPosition;
				result = ((flag & topPlacementPosition2 != null) && topPlacementPosition.GetValueOrDefault() > topPlacementPosition2.GetValueOrDefault());
			}
			else
			{
				result = false;
			}
			return result;
		}

		private bool WillAnimateScoreFilling()
		{
			return !this.DivisionOrSubdivisionChanged() && !this.TopPlacementChanged() && !this.LeftCalibration();
		}

		private bool IsOnLastSubdivisionOfLastDivision()
		{
			return this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Division + 1 >= this._divisions.Length && this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Subdivision + 1 >= this._divisions[this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Division].Subdivisions.Length;
		}

		private bool LeftCalibration()
		{
			return this._playerCompetitiveProgress.InitialState.Status == 1;
		}

		private ICompetitiveMatchResultRankedDivisionView GetDivisionView()
		{
			if (this.IsTopPlacementInvolved())
			{
				return this._view.RankedView.TopPlacementDivisionView;
			}
			if (this.RankedDivisionDown() && !this.LeftCalibration())
			{
				CompetitiveRank currentRank = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank;
				return this._view.RankedView.DivisionViews.ElementAt(currentRank.Division);
			}
			CompetitiveRank currentRank2 = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			return this._view.RankedView.DivisionViews.ElementAt(currentRank2.Division);
		}

		private void InitializeRankedView()
		{
			ActivatableExtensions.Activate(this._view.RankedView.Group);
			ICompetitiveMatchResultRankedDivisionView divisionView = this.GetDivisionView();
			ActivatableExtensions.Activate(divisionView.Group);
			this.InitializeDivisionName();
			this.InitializeRankChangeDescription();
			this.InitializeDivisionGlowColor();
			this.InitializeDivisionImages(divisionView);
			this.InitializeTopPlacementPositionLabels(divisionView);
			this.InitializeScores();
			this.InitializeChangedScore();
		}

		private void InitializeDivisionImages(ICompetitiveMatchResultRankedDivisionView divisionView)
		{
			CompetitiveRank currentRank = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank;
			CompetitiveRank currentRank2 = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			if (this.LeftCalibration())
			{
				this.SetCompleteDivisionBadge(divisionView, currentRank2);
				string subdivisionNumberFileName = this._divisionsBadgeNameBuilder.GetSubdivisionNumberFileName(this._divisions[currentRank2.Division], currentRank2.Subdivision);
				divisionView.CurrentSubdivisionImage.SetImageName(subdivisionNumberFileName);
			}
			else if (this.RankedDivisionUp() || this.EnteredTopPlacement())
			{
				this.SetCompleteDivisionAndPreviousDivisionBadges(divisionView, currentRank, currentRank2);
				if (!this.EnteredTopPlacement())
				{
					string subdivisionNumberFileName2 = this._divisionsBadgeNameBuilder.GetSubdivisionNumberFileName(this._divisions[currentRank2.Division], currentRank2.Subdivision);
					divisionView.CurrentSubdivisionImage.SetImageName(subdivisionNumberFileName2);
				}
			}
			else if (this.RankedDivisionDown() || this.LeftTopPlacement())
			{
				this.SetCompleteDivisionAndPreviousDivisionBadges(divisionView, currentRank2, currentRank);
				if (!this.LeftTopPlacement())
				{
					string subdivisionNumberFileName3 = this._divisionsBadgeNameBuilder.GetSubdivisionNumberFileName(this._divisions[currentRank.Division], currentRank.Subdivision);
					divisionView.CurrentSubdivisionImage.SetImageName(subdivisionNumberFileName3);
				}
			}
			else if (this.RankedSubdivisionChanged())
			{
				Division division = this._divisions[currentRank2.Division];
				string divisionBadgeBackgroundFileName = this._divisionsBadgeNameBuilder.GetDivisionBadgeBackgroundFileName(division);
				divisionView.AnimatedDivisionImage.SetImageName(divisionBadgeBackgroundFileName);
				Division division2 = this._divisions[currentRank.Division];
				string subdivisionNumberFileName4 = this._divisionsBadgeNameBuilder.GetSubdivisionNumberFileName(division2, currentRank.Subdivision);
				divisionView.PreviousSubdivisionImage.SetImageName(subdivisionNumberFileName4);
				string subdivisionNumberFileName5 = this._divisionsBadgeNameBuilder.GetSubdivisionNumberFileName(division, currentRank2.Subdivision);
				divisionView.CurrentSubdivisionImage.SetImageName(subdivisionNumberFileName5);
			}
			else
			{
				this.SetCompleteDivisionBadge(divisionView, currentRank2);
			}
		}

		private void InitializeTopPlacementPositionLabels(ICompetitiveMatchResultRankedDivisionView divisionView)
		{
			CompetitiveRank currentRank = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank;
			CompetitiveRank currentRank2 = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			if (this.TopPlacementUp())
			{
				divisionView.TopPlacementLabel.Text = currentRank2.TopPlacementPosition.Value.ToString();
				divisionView.PreviousTopPlacementLabel.Text = currentRank.TopPlacementPosition.Value.ToString();
			}
			else if (this.TopPlacementDown())
			{
				divisionView.TopPlacementLabel.Text = currentRank2.TopPlacementPosition.Value.ToString();
				divisionView.PreviousTopPlacementLabel.Text = currentRank.TopPlacementPosition.Value.ToString();
			}
			else if (this.LeftTopPlacement())
			{
				divisionView.PreviousTopPlacementLabel.Text = currentRank.TopPlacementPosition.Value.ToString();
			}
			else if (this.IsTopPlacementInvolved())
			{
				divisionView.TopPlacementLabel.Text = currentRank2.TopPlacementPosition.Value.ToString();
			}
		}

		private void InitializeScores()
		{
			if (this.IsOnLastSubdivisionOfLastDivision())
			{
				this.InitializeScoreWithoutProgress();
			}
			else
			{
				Subdivision nextSubdivision = this.GetNextSubdivision();
				this._view.RankedView.NextScoreLabel.Text = nextSubdivision.StartingScore.ToString();
				this.InitializeScoreWithProgress();
			}
		}

		private void SetCompleteDivisionAndPreviousDivisionBadges(ICompetitiveMatchResultRankedDivisionView divisionView, CompetitiveRank staticImageRank, CompetitiveRank animatedImageRank)
		{
			Division division = this._divisions[staticImageRank.Division];
			divisionView.StaticDivisionImage.SetImageName(this._divisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(division, staticImageRank.Subdivision, 700));
			this.SetCompleteDivisionBadge(divisionView, animatedImageRank);
		}

		private void SetCompleteDivisionBadge(ICompetitiveMatchResultRankedDivisionView divisionView, CompetitiveRank rank)
		{
			string imageName;
			if (this.IsTopPlacementInvolved())
			{
				imageName = this._divisionsBadgeNameBuilder.GetTopDivisionBadgeFileName(700);
			}
			else
			{
				Division division = this._divisions[rank.Division];
				imageName = this._divisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(division, rank.Subdivision, 700);
			}
			divisionView.AnimatedDivisionImage.SetImageName(imageName);
		}

		private void InitializeRankChangeDescription()
		{
			this._view.RankedView.RankChangeDescriptionLabel.Text = this.GetRankChangeDescription();
		}

		private void InitializeDivisionName()
		{
			CompetitiveRank currentRank = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			string divisionWithSubdivisionNameTranslated = this._divisionsBadgeNameBuilder.GetDivisionWithSubdivisionNameTranslated(currentRank);
			this._view.RankedView.DivisionNameLabel.Text = divisionWithSubdivisionNameTranslated;
		}

		private void InitializeDivisionGlowColor()
		{
			CompetitiveRank currentRank = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			int num;
			if (currentRank.TopPlacementPosition != null)
			{
				num = this._view.RankedView.DivisionsGlowColors.Length - 1;
			}
			else
			{
				num = currentRank.Division;
			}
			Color color = this._view.RankedView.DivisionsGlowColors[num];
			this._view.RankedView.DivisionGlowImage.Color = color;
		}

		private string GetRankChangeDescription()
		{
			CompetitiveRank currentRank = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			if (this.LeftCalibration())
			{
				return this.GetTranslationWithDivisionAndSubdivision("RANKING_CLASSIFICATION", currentRank);
			}
			if (this.EnteredTopPlacement())
			{
				return this.GetTranslationWithDivisionAndTopPlacementPosition("RANKING_HEAVYMETAL_ENTER", currentRank);
			}
			if (this.RankedDivisionUp())
			{
				return this.GetTranslationWithDivisionAndSubdivision("RANKING_UP_DIVISION", currentRank);
			}
			if (this.RankedDivisionDown() || this.LeftTopPlacement())
			{
				return this.GetTranslationWithDivisionAndSubdivision("RANKING_DOWN_DIVISION", currentRank);
			}
			if (this.RankedSubdivisionUp())
			{
				return this.GetTranslationWithSubdivision("RANKING_UP_SUBDIVISION", currentRank);
			}
			if (this.RankedSubdivisionDown())
			{
				return this.GetTranslationWithSubdivision("RANKING_DOWN_SUBDIVISION", currentRank);
			}
			if (this.TopPlacementUp())
			{
				return this.GetTranslationWithTopPlacementPosition("RANKING_UP_POSITION", currentRank);
			}
			if (this.TopPlacementDown())
			{
				return this.GetTranslationWithTopPlacementPosition("RANKING_DOWN_POSITION", currentRank);
			}
			return string.Empty;
		}

		private string GetTranslationWithDivisionAndSubdivision(string translationDraft, CompetitiveRank rank)
		{
			string divisionNameTranslated = this._divisionsBadgeNameBuilder.GetDivisionNameTranslated(rank);
			string subdivisionNumberTranslated = this._divisionsBadgeNameBuilder.GetSubdivisionNumberTranslated(rank);
			return this._translation.GetFormatted(translationDraft, TranslationContext.Ranked, new object[]
			{
				divisionNameTranslated,
				subdivisionNumberTranslated
			});
		}

		private string GetTranslationWithSubdivision(string translationDraft, CompetitiveRank rank)
		{
			string subdivisionNumberTranslated = this._divisionsBadgeNameBuilder.GetSubdivisionNumberTranslated(rank);
			return this._translation.GetFormatted(translationDraft, TranslationContext.Ranked, new object[]
			{
				subdivisionNumberTranslated
			});
		}

		private string GetTranslationWithTopPlacementPosition(string translationDraft, CompetitiveRank rank)
		{
			string text = rank.TopPlacementPosition.Value.ToString();
			return this._translation.GetFormatted(translationDraft, TranslationContext.Ranked, new object[]
			{
				text
			});
		}

		private string GetTranslationWithDivisionAndTopPlacementPosition(string translationDraft, CompetitiveRank rank)
		{
			string divisionNameTranslated = this._divisionsBadgeNameBuilder.GetDivisionNameTranslated(rank);
			string text = rank.TopPlacementPosition.Value.ToString();
			return this._translation.GetFormatted(translationDraft, TranslationContext.Ranked, new object[]
			{
				divisionNameTranslated,
				text
			});
		}

		private void InitializeChangedScore()
		{
			if (this.LeftCalibration())
			{
				ActivatableExtensions.Deactivate(this._view.RankedView.ScoreChangeGroup);
				return;
			}
			int score = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Score;
			int score2 = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Score;
			int num = score2 - score;
			int num2 = Mathf.Abs(num);
			if (this.IsOnLastSubdivisionOfLastDivision())
			{
				this._view.RankedView.ProgresslessChangedScoreLabel.Text = num2.ToString();
			}
			else
			{
				this._view.RankedView.ChangedScoreLabel.Text = num2.ToString();
			}
			if (num >= 0)
			{
				if (this.IsOnLastSubdivisionOfLastDivision())
				{
					ActivatableExtensions.Activate(this._view.RankedView.ProgresslessPositiveScoreChangeActivatable);
				}
				else
				{
					ActivatableExtensions.Activate(this._view.RankedView.PositiveScoreChangeActivatable);
				}
			}
			else if (this.IsOnLastSubdivisionOfLastDivision())
			{
				ActivatableExtensions.Activate(this._view.RankedView.ProgresslessNegativeScoreChangeActivatable);
			}
			else
			{
				ActivatableExtensions.Activate(this._view.RankedView.NegativeScoreChangeActivatable);
			}
		}

		private void InitializeScoreWithoutProgress()
		{
			int score;
			if (this.WillAnimateScoreFilling())
			{
				score = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Score;
			}
			else
			{
				score = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Score;
			}
			this._view.RankedView.ProgresslessCurrentScoreLabel.Text = score.ToString();
			ActivatableExtensions.Deactivate(this._view.RankedView.ScoreProgressGroup);
			ActivatableExtensions.Activate(this._view.RankedView.ProgresslessScoreGroup);
		}

		private void InitializeScoreWithProgress()
		{
			CompetitiveRank currentRank = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			Division division = this._divisions[currentRank.Division];
			Subdivision nextSubdivision = this.GetNextSubdivision();
			Subdivision subdivision = division.Subdivisions[currentRank.Subdivision];
			int num = nextSubdivision.StartingScore - subdivision.StartingScore;
			int score;
			if (this.WillAnimateScoreFilling())
			{
				score = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Score;
			}
			else
			{
				score = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Score;
			}
			this._view.RankedView.CurrentScoreLabel.Text = score.ToString();
			this._view.RankedView.ScoreProgressBar.FillPercent = (float)(score - subdivision.StartingScore) / (float)num;
		}

		private Subdivision GetNextSubdivision()
		{
			CompetitiveRank currentRank = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			Division division = this._divisions[currentRank.Division];
			if (currentRank.Subdivision == division.Subdivisions.Length - 1)
			{
				return this._divisions[currentRank.Division + 1].Subdivisions[0];
			}
			return division.Subdivisions[currentRank.Subdivision + 1];
		}

		private IObservable<Unit> PlayRankChangeAnimation(ICompetitiveMatchResultRankedDivisionView divisionView, IObservable<Unit> changeAnimation)
		{
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.First<Unit>(this._view.RankedView.DivisionOpenTrigger), (Unit _) => changeAnimation), delegate(Unit _)
			{
				ObservableExtensions.Subscribe<Unit>(divisionView.IdleAnimation.Play());
			}), delegate(Unit _)
			{
				ObservableExtensions.Subscribe<Unit>(this.GetGlowAnimation().Play());
			}));
			return this._view.RankedView.TransitionAnimation.Play();
		}

		private IAnimation GetGlowAnimation()
		{
			if (this.FinishedAtTopPlacement())
			{
				return this._view.RankedView.TopPlacementDivisionView.GlowAnimation;
			}
			CompetitiveRank currentRank = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			ICompetitiveMatchResultRankedDivisionView competitiveMatchResultRankedDivisionView = this._view.RankedView.DivisionViews.ElementAt(currentRank.Division);
			return competitiveMatchResultRankedDivisionView.GlowAnimation;
		}

		private IObservable<Unit> PlayUnchangedRankAnimation(ICompetitiveMatchResultRankedDivisionView divisionView)
		{
			ObservableExtensions.Subscribe<Unit>(divisionView.IdleAnimation.Play());
			ObservableExtensions.Subscribe<Unit>(this.GetGlowAnimation().Play());
			return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(this._view.RankedView.ShowAnimation.Play(), delegate(Unit _)
			{
				this.PlayFillingAudio();
			}), (Unit _) => this.PlayScoreFillingAnimation());
		}

		private void PlayFillingAudio()
		{
			int num = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Score - this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Score;
			if (num > 0)
			{
				this._view.RankedView.ScoreFillUpAudio.PlayOnce();
			}
			else
			{
				this._view.RankedView.ScoreFillDownAudio.PlayOnce();
			}
		}

		private IObservable<Unit> PlayScoreFillingAnimation()
		{
			return Observable.AsUnitObservable<float>(Observable.TakeWhile<float>(Observable.Do<float>(Observable.Select<float, float>(Observable.Scan<long, float>(Observable.EveryUpdate(), 0f, (float timePassed, long _) => timePassed + Time.deltaTime), (float timePassed) => timePassed / this._view.RankedView.ScoreFillTimeSeconds), new Action<float>(this.UpdateScoreFilling)), (float percent) => percent < 1f));
		}

		private void UpdateScoreFilling(float percent)
		{
			int score = this._playerCompetitiveProgress.InitialState.Rank.CurrentRank.Score;
			int score2 = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank.Score;
			int num = score2 - score;
			float num2 = this._view.RankedView.ScoreFillAnimationCurve.Evaluate(percent);
			float num3 = (float)num * num2;
			int num4 = score + Mathf.CeilToInt(num3);
			if (this.IsOnLastSubdivisionOfLastDivision())
			{
				this._view.RankedView.ProgresslessCurrentScoreLabel.Text = num4.ToString();
				return;
			}
			CompetitiveRank currentRank = this._playerCompetitiveProgress.FinalState.Rank.CurrentRank;
			Division division = this._divisions[currentRank.Division];
			Subdivision subdivision = division.Subdivisions[currentRank.Subdivision];
			Subdivision nextSubdivision = this.GetNextSubdivision();
			int num5 = nextSubdivision.StartingScore - subdivision.StartingScore;
			this._view.RankedView.CurrentScoreLabel.Text = num4.ToString();
			this._view.RankedView.ScoreProgressBar.FillPercent = ((float)(score - subdivision.StartingScore) + num3) / (float)num5;
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
			});
		}

		public IObservable<Unit> Dispose()
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private const string LeftCalibrationDraft = "RANKING_CLASSIFICATION";

		private const string DivisionUpDraft = "RANKING_UP_DIVISION";

		private const string DivisionDownDraft = "RANKING_DOWN_DIVISION";

		private const string SubdivisionUpDraft = "RANKING_UP_SUBDIVISION";

		private const string SubdivisionDownDraft = "RANKING_DOWN_SUBDIVISION";

		private const string TopPlacementDivisionEnterDraft = "RANKING_HEAVYMETAL_ENTER";

		private const string TopPlacementUpDraft = "RANKING_UP_POSITION";

		private const string TopPlacementDownDraft = "RANKING_DOWN_POSITION";

		private readonly IViewProvider _viewProvider;

		private readonly IWaitAndGetMyPlayerCompetitiveStateProgress _waitAndGetMyPlayerCompetitiveStateProgress;

		private readonly IGetCurrentOrNextCompetitiveSeason _getCurrentOrNextCompetitiveSeason;

		private readonly IGetCompetitiveDivisions _getCompetitiveDivisions;

		private readonly ILocalizeKey _translation;

		private readonly ICompetitiveDivisionsBadgeNameBuilder _divisionsBadgeNameBuilder;

		private readonly Subject<Unit> _hideSubject = new Subject<Unit>();

		private ICompetitiveMatchResultView _view;

		private PlayerCompetitiveProgress _playerCompetitiveProgress;

		private Division[] _divisions;
	}
}
