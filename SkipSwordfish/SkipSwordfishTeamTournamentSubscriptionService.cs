using System;
using HeavyMetalMachines.Tournaments.Infra;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishTeamTournamentSubscriptionService : ITeamTournamentSubscriptionService
	{
		public IObservable<Unit> Subscribe(Guid teamId, long tournamentId)
		{
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> Unsubscribe(Guid teamId, long tournamentId)
		{
			return Observable.ReturnUnit();
		}
	}
}
