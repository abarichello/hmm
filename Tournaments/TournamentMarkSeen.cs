using System;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Tournaments.Infra;
using UniRx;

namespace HeavyMetalMachines.Tournaments
{
	public class TournamentMarkSeen : ITournamentMarkSeen
	{
		public TournamentMarkSeen(ITournamentCheckFirstTimeSeeingService tournamentCheckFirstTimeSeeingService, IGetLocalPlayer getLocalPlayer)
		{
			this._tournamentCheckFirstTimeSeeingService = tournamentCheckFirstTimeSeeingService;
			this._getLocalPlayer = getLocalPlayer;
		}

		public IObservable<Unit> MarkAsSeen()
		{
			return Observable.Do<Unit>(this._tournamentCheckFirstTimeSeeingService.MarkTournamentAsSeen(), delegate(Unit _)
			{
				(this._getLocalPlayer.Get() as Player).Bag.HasSeenTournamentInfo = true;
			});
		}

		private readonly ITournamentCheckFirstTimeSeeingService _tournamentCheckFirstTimeSeeingService;

		private readonly IGetLocalPlayer _getLocalPlayer;
	}
}
