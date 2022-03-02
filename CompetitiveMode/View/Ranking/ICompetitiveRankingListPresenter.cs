using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public interface ICompetitiveRankingListPresenter
	{
		IObservable<Unit> Initialize();

		IObservable<Unit> Show();

		IObservable<Unit> Hide();

		void Dispose();

		Func<int, IObservable<PlayerCompetitiveRankingPosition[]>> GetRankings { get; set; }

		int RankingPlayersCount { get; set; }

		IObservable<Unit> OnListRefreshed { get; }
	}
}
