using System;
using HeavyMetalMachines.CompetitiveMode;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using Hoplon.Localization.TranslationTable;
using UniRx;

namespace HeavyMetalMachines.Friends.GUI
{
	public class FriendTooltipRankPresenter : IFriendTooltipRankPresenter
	{
		public FriendTooltipRankPresenter(IViewProvider viewProvider, IGetPlayerCompetitiveState getPlayerCompetitiveState, ICompetitiveDivisionsBadgeNameBuilder competitiveDivisionsBadgeNameBuilder, ILocalizeKey translation)
		{
			this._viewProvider = viewProvider;
			this._getPlayerCompetitiveState = getPlayerCompetitiveState;
			this._competitiveDivisionsBadgeNameBuilder = competitiveDivisionsBadgeNameBuilder;
			this._translation = translation;
		}

		public IObservable<Unit> LoadRank(long playerId)
		{
			this._view = this._viewProvider.Provide<IFriendTooltipRankView>(null);
			return Observable.AsUnitObservable<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(Observable.Do<PlayerCompetitiveState>(Observable.ContinueWith<Unit, PlayerCompetitiveState>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.ShowMainGroupAndLoading();
			}), (Unit _) => this.GetPlayerCompetitiveState(playerId)), new Action<PlayerCompetitiveState>(this.UpdateView)), delegate(PlayerCompetitiveState _)
			{
				this.HideLoading();
			}));
		}

		private void ShowMainGroupAndLoading()
		{
			this._view.MainGroup.SetActive(true);
			this.ToggleLoading(true);
			this._view.RankLabel.Text = this._translation.Get("Loading_loading", TranslationContext.Loading);
		}

		private void HideLoading()
		{
			this.ToggleLoading(false);
		}

		private void ToggleLoading(bool isLoading)
		{
			this._view.LoadingActivatable.SetActive(isLoading);
			this._view.RankImageActivatable.SetActive(!isLoading);
		}

		private IObservable<PlayerCompetitiveState> GetPlayerCompetitiveState(long playerId)
		{
			return this._getPlayerCompetitiveState.GetFromPlayerId(playerId);
		}

		private void UpdateView(PlayerCompetitiveState state)
		{
			if (state.Status != 2)
			{
				this._view.RankLabel.Text = this._translation.Get("NOT_RANKED", TranslationContext.Ranked);
				this._view.RankDynamicImage.SetImageName("ranking_unranked_league");
				return;
			}
			string divisionWithSubdivisionNameTranslated = this._competitiveDivisionsBadgeNameBuilder.GetDivisionWithSubdivisionNameTranslated(state.Rank.CurrentRank);
			string subdivisionBadgeFileName = this._competitiveDivisionsBadgeNameBuilder.GetSubdivisionBadgeFileName(state.Rank.CurrentRank, 30);
			this._view.RankLabel.Text = divisionWithSubdivisionNameTranslated;
			this._view.RankDynamicImage.SetImageName(subdivisionBadgeFileName);
		}

		private readonly IViewProvider _viewProvider;

		private readonly IGetPlayerCompetitiveState _getPlayerCompetitiveState;

		private readonly ICompetitiveDivisionsBadgeNameBuilder _competitiveDivisionsBadgeNameBuilder;

		private readonly ILocalizeKey _translation;

		private IFriendTooltipRankView _view;
	}
}
