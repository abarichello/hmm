using System;
using HeavyMetalMachines.CompetitiveMode.Prizes;
using HeavyMetalMachines.Store.Business;
using UniRx;

namespace HeavyMetalMachines.CompetitiveMode.Infra.Fake
{
	public class SkipSwordfishCompetitivePrizesService : ICompetitivePrizesService
	{
		public IObservable<CompetitiveSeasonPrizeCollection> CollectPlayerPendingCompetitivePrizes(long playerId)
		{
			return Observable.Return<CompetitiveSeasonPrizeCollection>(new CompetitiveSeasonPrizeCollection
			{
				CollectedItems = new Item[0]
			});
		}
	}
}
