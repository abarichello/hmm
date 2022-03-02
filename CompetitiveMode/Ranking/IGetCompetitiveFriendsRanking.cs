using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Ranking
{
	public interface IGetCompetitiveFriendsRanking
	{
		IObservable<PlayerCompetitiveRankingPosition[]> Get(int numberOfPlayers);
	}
}
