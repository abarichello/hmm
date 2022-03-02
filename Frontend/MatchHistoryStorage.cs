using System;
using HeavyMetalMachines.DataTransferObjects.Player;

namespace HeavyMetalMachines.Frontend
{
	public class MatchHistoryStorage : IMatchHistoryStorage
	{
		public int MatchesPlayed { get; private set; }

		public void SetMatchHistory(MainMenuData data)
		{
			this.MatchesPlayed = data.MatchesPlayed;
		}
	}
}
