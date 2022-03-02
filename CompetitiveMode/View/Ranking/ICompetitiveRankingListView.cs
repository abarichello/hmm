using System;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public interface ICompetitiveRankingListView
	{
		IScroller<CompetitiveRankingPlayerScrollerData> RankingScroller { get; }

		IActivatable LoadingActivatable { get; }

		IButton RefreshButton { get; }

		IAnimation FadeInAnimation { get; }

		IAnimation FadeOutAnimation { get; }

		ILabel LastUpdatedLabel { get; }

		IActivatable EmptyRankingGroup { get; }

		IObservable<Unit> AnimateFadeOutScrollerItems();

		void DisableRankingScroll();

		void EnableRankingScroll();
	}
}
