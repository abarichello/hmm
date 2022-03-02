using System;
using HeavyMetalMachines.Matchmaking.Queue;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public interface ISearchCompetitiveMatch
	{
		void Search(IMatchmakingMatchConfirmation matchConfirmation);
	}
}
