using System;
using HeavyMetalMachines.Tournaments;
using HeavyMetalMachines.Tournaments.API;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishTournamentSeasonProvider : ITournamentSeasonProvider
	{
		public IObservable<TournamentSeason> GetCurrentSeason()
		{
			return Observable.Return<TournamentSeason>(new TournamentSeason
			{
				Id = 1L,
				Number = 4,
				StartDate = DateTime.Now,
				EndDate = DateTime.Now.AddMonths(2)
			});
		}
	}
}
