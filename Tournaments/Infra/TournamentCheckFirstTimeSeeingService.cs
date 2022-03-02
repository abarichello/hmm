using System;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.Tournaments.Infra
{
	public class TournamentCheckFirstTimeSeeingService : ITournamentCheckFirstTimeSeeingService
	{
		public IObservable<Unit> MarkTournamentAsSeen()
		{
			return Observable.AsUnitObservable<NetResult>(TournamentCustomWS.MarkTournamentFirstSeen());
		}
	}
}
