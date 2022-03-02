using System;
using System.Linq;
using HeavyMetalMachines.CompetitiveMode.Divisions;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Matchmaking.Configuration;
using HeavyMetalMachines.Matchmaking.Queue;
using HeavyMetalMachines.Presenting;
using Hoplon.Localization;
using Hoplon.Localization.TranslationTable;
using Hoplon.Time;
using Hoplon.ToggleableFeatures;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public class CompetitiveQueueJoinPresenter : ICompetitiveQueueJoinPresenter, IDisposable
	{
		public CompetitiveQueueJoinPresenter(IViewProvider viewProvider, IGetPlayerCompetitiveState getPlayerCompetitiveState, IGetThenObservePlayerCompetitiveJoinability getThenObservePlayerCompetitiveJoinability, ICurrentTime currentTime, ICompetitiveDivisionsBadgeNameBuilder competitiveDivisionsBadgeNameBuilder, IGetCompetitiveDivisions getCompetitiveDivisions, ISearchCompetitiveMatch searchCompetitiveMatch, IMatchmakingMatchConfirmation matchmakingMatchConfirmation, IIsFeatureToggled isFeatureToggled, ILocalizeKey translation, ILocalizeDateTime localizeDateTime, ICountdownDisplayGenerator countdownDisplayGenerator)
		{
			this._viewProvider = viewProvider;
			this._getPlayerCompetitiveState = getPlayerCompetitiveState;
			this._getThenObservePlayerCompetitiveJoinability = getThenObservePlayerCompetitiveJoinability;
			this._currentTime = currentTime;
			this._competitiveDivisionsBadgeNameBuilder = competitiveDivisionsBadgeNameBuilder;
			this._getCompetitiveDivisions = getCompetitiveDivisions;
			this._searchCompetitiveMatch = searchCompetitiveMatch;
			this._matchmakingMatchConfirmation = matchmakingMatchConfirmation;
			this._isFeatureToggled = isFeatureToggled;
			this._translation = translation;
			this._localizeDateTime = localizeDateTime;
			this._countdownDisplayGenerator = countdownDisplayGenerator;
		}

		public string Context { get; set; }

		public void Initialize()
		{
			this._view = this._viewProvider.Provide<ICompetitiveQueueJoinView>(this.ViewProviderContext);
			this.DeactivateAllGroups();
			this.InitializeView();
		}

		public void Enable()
		{
			this.InitializeJoinButton();
		}

		public void Disable()
		{
			this._joinButtonDisposable.Dispose();
		}

		private void InitializeView()
		{
			this.InitializeJoinabilityObservation();
			this.InitializeJoinButton();
		}

		private void InitializeJoinabilityObservation()
		{
			this._joinabilityChangeDisposable = ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<CompetitiveQueueJoinabilityResult, Unit>(this._getThenObservePlayerCompetitiveJoinability.GetThenObserve(), (CompetitiveQueueJoinabilityResult joinabilityResult) => this.UpdateView(joinabilityResult)));
		}

		private void InitializeJoinButton()
		{
			this._joinButtonDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._view.JoinButton.OnClick(), delegate(Unit _)
			{
				this._searchCompetitiveMatch.Search(this._matchmakingMatchConfirmation);
			}));
		}

		private IObservable<Unit> UpdateView(CompetitiveQueueJoinabilityResult joinabilityResult)
		{
			IObservable<Unit> observable = Observable.Do<Unit>(this._view.HideGroupAnimation.Play(), delegate(Unit _)
			{
				this.DeactivateAllGroups();
			});
			if (joinabilityResult.CanJoin)
			{
				observable = Observable.AsUnitObservable<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(Observable.ContinueWith<Unit, PlayerCompetitiveState>(observable, this._getPlayerCompetitiveState.GetMine()), new Action<PlayerCompetitiveState>(this.InitializeAsCalibratingOrRanked)));
			}
			else if (joinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.ModeIsLocked))
			{
				observable = Observable.AsUnitObservable<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(Observable.ContinueWith<Unit, PlayerCompetitiveState>(observable, this._getPlayerCompetitiveState.GetMine()), new Action<PlayerCompetitiveState>(this.InitializeAsLocked)));
			}
			else if (joinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.QueueIsNotOpen))
			{
				observable = Observable.AsUnitObservable<Unit>(Observable.Do<Unit>(observable, delegate(Unit _)
				{
					this.InitializeAsWaitingForQueueToOpen(joinabilityResult.NextQueuePeriod.Value);
				}));
			}
			else if (joinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.PlayerIsBannedFromQueue))
			{
				observable = Observable.AsUnitObservable<Unit>(Observable.Do<Unit>(observable, delegate(Unit _)
				{
					this.InitializeAsBanned(joinabilityResult);
				}));
			}
			else
			{
				observable = Observable.Do<Unit>(observable, delegate(Unit _)
				{
					this.InitializeWithUnjoinableReasonMessage(joinabilityResult);
				});
			}
			return Observable.ContinueWith<Unit, Unit>(observable, (Unit _) => this._view.ShowGroupAnimation.Play());
		}

		private void InitializeAsCalibratingOrRanked(PlayerCompetitiveState playerCompetitiveState)
		{
			if (playerCompetitiveState.Status == 1)
			{
				this.InitializeAsCalibrating(playerCompetitiveState);
			}
			else
			{
				this.InitializeAsRanked(playerCompetitiveState);
			}
		}

		private void InitializeAsLocked(PlayerCompetitiveState playerCompetitiveState)
		{
			int totalMatchesPlayed = playerCompetitiveState.Requirements.TotalMatchesPlayed;
			int totalRequiredMatches = playerCompetitiveState.Requirements.TotalRequiredMatches;
			this._view.MatchesPlayedToUnlockLabel.Text = totalMatchesPlayed.ToString();
			this._view.TotalMatchesToUnlockLabel.Text = string.Format("/ {0}", totalRequiredMatches.ToString());
			ActivatableExtensions.Activate(this._view.ModeLockedGroup);
			this.TryToSetUnjoinableIndicatorState(true);
		}

		private void InitializeAsBanned(CompetitiveQueueJoinabilityResult joinabilityResult)
		{
			DateTime? banEndTime = joinabilityResult.BanEndTime;
			DateTime dateTime = (banEndTime == null) ? DateTime.MaxValue : banEndTime.Value;
			this._view.BanTimerLabel.Text = this._countdownDisplayGenerator.GenerateFromNowTo(dateTime);
			ActivatableExtensions.Activate(this._view.BanGroup);
			this.TryToSetUnjoinableIndicatorState(true);
		}

		private void InitializeAsWaitingForQueueToOpen(QueuePeriod nextPeriod)
		{
			DateTime openDateTime = nextPeriod.OpenDateTimeUtc.ToLocalTime();
			this.TryToSetNextQueuePeriodDates(openDateTime);
			this.TryToSetWaitingNextQueuePeriodGroupState(true);
			this.TryToSetUnjoinableIndicatorState(true);
		}

		private void InitializeAsCalibrating(PlayerCompetitiveState playerCompetitiveState)
		{
			int totalMatchesPlayed = playerCompetitiveState.CalibrationState.TotalMatchesPlayed;
			int totalRequiredMatches = playerCompetitiveState.CalibrationState.TotalRequiredMatches;
			this._view.MatchesPlayedToCalibrateLabel.Text = totalMatchesPlayed.ToString();
			this._view.TotalMatchesToCalibrateLabel.Text = string.Format("/ {0}", totalRequiredMatches.ToString());
			this._view.JoinButton.IsInteractable = true;
			ActivatableExtensions.Activate(this._view.CalibrationGroup);
		}

		private void InitializeAsRanked(PlayerCompetitiveState playerCompetitiveState)
		{
			CompetitiveRank currentRank = playerCompetitiveState.Rank.CurrentRank;
			if (currentRank.TopPlacementPosition != null)
			{
				this._view.ScoreLabel.IsActive = false;
				ActivatableExtensions.Activate(this._view.ScoreWithTopPlacementGroup);
				this._view.ScoreWithTopPlacementLabel.Text = currentRank.Score.ToString();
				this._view.TopPlacementLabel.Text = currentRank.TopPlacementPosition.Value.ToString();
			}
			else
			{
				this._view.ScoreLabel.IsActive = true;
				ActivatableExtensions.Deactivate(this._view.ScoreWithTopPlacementGroup);
				this._view.ScoreLabel.Text = currentRank.Score.ToString();
			}
			this.SetDivisionImage(currentRank);
			this._view.JoinButton.IsInteractable = true;
			ActivatableExtensions.Activate(this._view.RankedGroup);
		}

		private void SetDivisionImage(CompetitiveRank rank)
		{
			Division[] array = this._getCompetitiveDivisions.Get();
			Division division = array[rank.Division];
			string imageName;
			if (rank.TopPlacementPosition != null)
			{
				imageName = this._competitiveDivisionsBadgeNameBuilder.GetTopDivisionBadgeFileName(100);
			}
			else
			{
				imageName = this._competitiveDivisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(division, rank.Subdivision, 100);
			}
			this._view.DivisionImage.SetImageName(imageName);
		}

		private void InitializeWithUnjoinableReasonMessage(CompetitiveQueueJoinabilityResult joinabilityResult)
		{
			ActivatableExtensions.Activate(this._view.UnjoinableQueueGroup);
			string text = null;
			if (joinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.GroupMemberHasNotUnlockedCompetitive))
			{
				text = "RANKED_FEEDBACK_GROUP_MEMBER";
			}
			else if (joinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.PlayerIsNotLeaderOfGroup))
			{
				text = "RANKED_FEEDBACK_GROUP_LEADER";
			}
			else if (joinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.GroupMembersCountAboveLimit))
			{
				text = "RANKED_BIG_GROUP_FEEDBACK";
			}
			else if (joinabilityResult.Reasons.Contains(CompetitiveQueueUnjoinabilityReason.GroupMemberIsNotInMainMenu))
			{
				text = "RANKED_FEEDBACK_GROUP_MEMBER_MAINMENU";
			}
			else
			{
				ActivatableExtensions.Deactivate(this._view.UnjoinableQueueGroup);
			}
			if (text != null)
			{
				this._view.UnjoinableQueueMessageLabel.Text = this._translation.Get(text, TranslationContext.Ranked);
			}
			this.TryToSetUnjoinableIndicatorState(true);
		}

		private void DeactivateAllGroups()
		{
			this.TryToSetUnjoinableIndicatorState(false);
			this.TryToSetWaitingNextQueuePeriodGroupState(false);
			this._view.JoinButton.IsInteractable = false;
			ActivatableExtensions.Deactivate(this._view.CalibrationGroup);
			ActivatableExtensions.Deactivate(this._view.RankedGroup);
			ActivatableExtensions.Deactivate(this._view.ModeLockedGroup);
			ActivatableExtensions.Deactivate(this._view.UnjoinableQueueGroup);
			ActivatableExtensions.Deactivate(this._view.BanGroup);
		}

		private void TryToSetUnjoinableIndicatorState(bool activate)
		{
			if (!this._view.UnjoinableIndicator.HasValue)
			{
				return;
			}
			if (activate)
			{
				ActivatableExtensions.Activate(this._view.UnjoinableIndicator);
			}
			else
			{
				ActivatableExtensions.Deactivate(this._view.UnjoinableIndicator);
			}
		}

		private void TryToSetWaitingNextQueuePeriodGroupState(bool activate)
		{
			if (!this._view.WaitingNextQueuePeriodGroup.HasValue)
			{
				return;
			}
			if (activate)
			{
				ActivatableExtensions.Activate(this._view.WaitingNextQueuePeriodGroup);
			}
			else
			{
				ActivatableExtensions.Deactivate(this._view.WaitingNextQueuePeriodGroup);
			}
		}

		private void TryToSetNextQueuePeriodDates(DateTime openDateTime)
		{
			if (!this._view.NextQueuePeriodOpenDateLabel.HasValue || !this._view.NextQueuePeriodOpenTimeLabel.HasValue)
			{
				return;
			}
			this._view.NextQueuePeriodOpenDateLabel.Text = LocalizationExtensions.GetShortDateString(this._localizeDateTime, openDateTime);
			this._view.NextQueuePeriodOpenTimeLabel.Text = LocalizationExtensions.GetShortTimeString(this._localizeDateTime, openDateTime);
		}

		public void Dispose()
		{
			if (this._joinButtonDisposable != null)
			{
				this._joinButtonDisposable.Dispose();
				this._joinButtonDisposable = null;
			}
			if (this._joinabilityChangeDisposable != null)
			{
				this._joinabilityChangeDisposable.Dispose();
				this._joinabilityChangeDisposable = null;
			}
		}

		public string ViewProviderContext { get; set; }

		private readonly IViewProvider _viewProvider;

		private readonly IGetPlayerCompetitiveState _getPlayerCompetitiveState;

		private readonly IGetThenObservePlayerCompetitiveJoinability _getThenObservePlayerCompetitiveJoinability;

		private readonly ICurrentTime _currentTime;

		private readonly ICompetitiveDivisionsBadgeNameBuilder _competitiveDivisionsBadgeNameBuilder;

		private readonly IGetCompetitiveDivisions _getCompetitiveDivisions;

		private readonly ISearchCompetitiveMatch _searchCompetitiveMatch;

		private readonly IMatchmakingMatchConfirmation _matchmakingMatchConfirmation;

		private readonly IIsFeatureToggled _isFeatureToggled;

		private readonly ILocalizeKey _translation;

		private readonly ILocalizeDateTime _localizeDateTime;

		private readonly ICountdownDisplayGenerator _countdownDisplayGenerator;

		private ICompetitiveQueueJoinView _view;

		private IDisposable _joinabilityChangeDisposable;

		private IDisposable _joinButtonDisposable;
	}
}
