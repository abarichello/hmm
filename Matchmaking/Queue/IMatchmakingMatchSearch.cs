using System;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public interface IMatchmakingMatchSearch
	{
		void Search(IMatchmakingMatchConfirmation matchConfirmation);
	}
}
