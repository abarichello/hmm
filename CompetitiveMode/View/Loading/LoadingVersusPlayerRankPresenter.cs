using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Presenting;
using Hoplon.ToggleableFeatures;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Loading
{
	public class LoadingVersusPlayerRankPresenter : ILoadingVersusPlayerRankPresenter
	{
		public LoadingVersusPlayerRankPresenter(IIsFeatureToggled isFeatureToggled, IGetPlayerCompetitiveState getPlayerCompetitiveState, ICompetitiveDivisionsBadgeNameBuilder badgeNameBuilder)
		{
			LoadingVersusPlayerRankPresenter.AssertParametersAreValid(isFeatureToggled, getPlayerCompetitiveState, badgeNameBuilder);
			this._isFeatureToggled = isFeatureToggled;
			this._getPlayerCompetitiveState = getPlayerCompetitiveState;
			this._badgeNameBuilder = badgeNameBuilder;
		}

		private static void AssertParametersAreValid(IIsFeatureToggled isFeatureToggled, IGetPlayerCompetitiveState getPlayerCompetitiveState, ICompetitiveDivisionsBadgeNameBuilder badgeNameBuilder)
		{
		}

		public IObservable<Unit> LoadRank(long playerId, ILoadingVersusPlayerRankView view)
		{
			this._view = view;
			ActivatableExtensions.Activate(this._view.RankGroup);
			return Observable.AsUnitObservable<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(this._getPlayerCompetitiveState.GetFromPlayerId(playerId), new Action<PlayerCompetitiveState>(this.UpdatePlayerRank)));
		}

		private void UpdatePlayerRank(PlayerCompetitiveState state)
		{
			if (state.Status != 2)
			{
				this._view.RankGroup.SetActive(false);
				return;
			}
			string subdivisionBadgeFileName = this._badgeNameBuilder.GetSubdivisionBadgeFileName(state.Rank.CurrentRank, 100);
			this._view.RankDynamicImage.SetImageName(subdivisionBadgeFileName);
		}

		private ILoadingVersusPlayerRankView _view;

		private readonly IIsFeatureToggled _isFeatureToggled;

		private readonly IGetPlayerCompetitiveState _getPlayerCompetitiveState;

		private readonly ICompetitiveDivisionsBadgeNameBuilder _badgeNameBuilder;
	}
}
