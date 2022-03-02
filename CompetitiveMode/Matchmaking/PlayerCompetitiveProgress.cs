using System;
using HeavyMetalMachines.CompetitiveMode.Players;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class PlayerCompetitiveProgress
	{
		public PlayerCompetitiveProgress()
		{
		}

		public PlayerCompetitiveProgress(PlayerCompetitiveState initialState, PlayerCompetitiveState finalState)
		{
			this.InitialState = initialState;
			this.FinalState = finalState;
		}

		public PlayerCompetitiveState InitialState;

		public PlayerCompetitiveState FinalState;
	}
}
