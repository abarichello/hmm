using System;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Prizes
{
	public interface ICompetitivePrizesService
	{
		IObservable<CompetitiveSeasonPrizeCollection> CollectPlayerPendingCompetitivePrizes(long playerId);
	}
}
