using System;
using HeavyMetalMachines.Tournaments.Infra;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishTournamentCheckFirstTimeSeeingService : ITournamentCheckFirstTimeSeeingService
	{
		public IObservable<Unit> MarkTournamentAsSeen()
		{
			return Observable.ReturnUnit();
		}
	}
}
