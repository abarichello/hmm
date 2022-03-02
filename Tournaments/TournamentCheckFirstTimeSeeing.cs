using System;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Tournaments.API;
using HeavyMetalMachines.Tournaments.Infra;

namespace HeavyMetalMachines.Tournaments
{
	public class TournamentCheckFirstTimeSeeing : ITournamentCheckFirstTimeSeeing
	{
		public TournamentCheckFirstTimeSeeing(ITournamentCheckFirstTimeSeeingService checkFirstTimeSeeingService, IGetLocalPlayer getLocalPlayer)
		{
			this._getLocalPlayer = getLocalPlayer;
		}

		public bool HasSeenTournamentInfo()
		{
			Player player = (Player)this._getLocalPlayer.Get();
			return player.Bag.HasSeenTournamentInfo;
		}

		private readonly IGetLocalPlayer _getLocalPlayer;
	}
}
