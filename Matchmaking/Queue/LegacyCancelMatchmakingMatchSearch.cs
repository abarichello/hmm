using System;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Swordfish;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public class LegacyCancelMatchmakingMatchSearch : ICancelMatchmakingMatchSearch
	{
		public LegacyCancelMatchmakingMatchSearch(SwordfishMatchmaking swordfishMatchmaking)
		{
			this._swordfishMatchmaking = swordfishMatchmaking;
		}

		public void Cancel()
		{
			if (this._swordfishMatchmaking.IsInQueue())
			{
				this._swordfishMatchmaking.StopSearch();
			}
		}

		private readonly SwordfishMatchmaking _swordfishMatchmaking;
	}
}
