using System;
using HeavyMetalMachines.CompetitiveMode;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Customization.Business;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.ToggleableFeatures;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Profile
{
	public class PlayerProfilePresenter : IPlayerProfilePresenter
	{
		public PlayerProfilePresenter(IViewProvider viewProvider, IGetPlayerCompetitiveState getPlayerCompetitiveState, IIsFeatureToggled isFeatureToggled, ICompetitiveDivisionsBadgeNameBuilder competitiveDivisionsBadgeNameBuilder, IObservePlayerAvatarChanged observePlayerAvatarChanged)
		{
			this._viewProvider = viewProvider;
			this._getPlayerCompetitiveState = getPlayerCompetitiveState;
			this._isFeatureToggled = isFeatureToggled;
			this._competitiveDivisionsBadgeNameBuilder = competitiveDivisionsBadgeNameBuilder;
			this._observePlayerAvatarChanged = observePlayerAvatarChanged;
		}

		private bool IsProfileRefactorEnabled
		{
			get
			{
				return this._isFeatureToggled.Check(Features.ProfileRefactor);
			}
		}

		public IObservable<Unit> Initialize()
		{
			if (this.IsProfileRefactorEnabled)
			{
				return Observable.ReturnUnit();
			}
			this._view = this._viewProvider.Provide<IPlayerProfileView>(null);
			this._view.MainGroup.SetActive(false);
			IObservable<Unit> observable = Observable.AsUnitObservable<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(this._getPlayerCompetitiveState.GetMine(), new Action<PlayerCompetitiveState>(this.InitializeView)));
			IObservable<Unit> observable2 = Observable.Do<Unit>(this._observePlayerAvatarChanged.Observe(), delegate(Unit _)
			{
				Debug.Log("changed");
			});
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				observable,
				observable2
			});
		}

		private void InitializeView(PlayerCompetitiveState state)
		{
			if (this.IsProfileRefactorEnabled)
			{
				return;
			}
			if (state.Status != 2)
			{
				return;
			}
			this.SetCurrentRank(state);
			this.SetHighestRank(state);
			this._view.CurrentRankGrid.Reposition();
			this._view.MainGroup.SetActive(true);
		}

		private void SetCurrentRank(PlayerCompetitiveState state)
		{
			CompetitiveRank currentRank = state.Rank.CurrentRank;
			this._view.CurrentScoreLabel.Text = currentRank.Score.ToString();
			if (state.Rank.CurrentRank.TopPlacementPosition != null)
			{
				ActivatableExtensions.Activate(this._view.CurrentTopPlacementPositionGroup);
				this._view.CurrentTopPlacementPositionLabel.Text = currentRank.TopPlacementPosition.Value.ToString();
			}
			else
			{
				ActivatableExtensions.Deactivate(this._view.CurrentTopPlacementPositionGroup);
			}
			string divisionWithSubdivisionNameTranslated = this._competitiveDivisionsBadgeNameBuilder.GetDivisionWithSubdivisionNameTranslated(currentRank);
			this._view.CurrentRankLabel.Text = divisionWithSubdivisionNameTranslated;
			string subdivisionBadgeFileName = this._competitiveDivisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(currentRank, 200);
			this._view.CurrentRankDynamicImage.SetImageName(subdivisionBadgeFileName);
		}

		private void SetHighestRank(PlayerCompetitiveState state)
		{
			this._view.TooltipTopRankLabel.Text = this._competitiveDivisionsBadgeNameBuilder.GetDivisionWithSubdivisionNameTranslated(state.Rank.HighestRank);
			string subdivisionBadgeFileName = this._competitiveDivisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(state.Rank.HighestRank, 100);
			this._view.TooltipTopRankDynamicImage.SetImageName(subdivisionBadgeFileName);
		}

		private readonly IViewProvider _viewProvider;

		private readonly IGetPlayerCompetitiveState _getPlayerCompetitiveState;

		private readonly IIsFeatureToggled _isFeatureToggled;

		private readonly ICompetitiveDivisionsBadgeNameBuilder _competitiveDivisionsBadgeNameBuilder;

		private readonly IObservePlayerAvatarChanged _observePlayerAvatarChanged;

		private IPlayerProfileView _view;
	}
}
