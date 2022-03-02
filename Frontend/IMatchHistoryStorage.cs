using System;
using HeavyMetalMachines.DataTransferObjects.Player;

namespace HeavyMetalMachines.Frontend
{
	public interface IMatchHistoryStorage
	{
		int MatchesPlayed { get; }

		void SetMatchHistory(MainMenuData data);
	}
}
