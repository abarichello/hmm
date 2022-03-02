using System;
using HeavyMetalMachines.Tournaments;
using HeavyMetalMachines.Tournaments.Infra;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishTournamentTiersProvider : ITournamentTiersProvider
	{
		public IObservable<TournamentTier[]> Get()
		{
			return Observable.Return<TournamentTier[]>(new TournamentTier[]
			{
				new TournamentTier
				{
					Id = 1L,
					Name = "Beginner",
					QueuName = "Beginner_Queue"
				},
				new TournamentTier
				{
					Id = 2L,
					Name = "Pro",
					QueuName = "Pro_Queue"
				}
			});
		}
	}
}
