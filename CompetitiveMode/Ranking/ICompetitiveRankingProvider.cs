using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Ranking
{
	public interface ICompetitiveRankingProvider
	{
		IObservable<PlayerCompetitiveRankingPosition[]> GetGlobalRanking(long seasonId, int numberOfPlayers);

		IObservable<PlayerCompetitiveRankingPosition[]> GetPlayersRanking(long seasonId, int numberOfPlayers, long[] playersIds);
	}
}
