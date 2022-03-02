using System;
using System.Collections.Generic;
using HeavyMetalMachines.ExpirableStorage;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public class PlayersStateStorage : IPlayersStateStorage
	{
		public PlayersStateStorage()
		{
			this.PlayersCompetitiveStateDictionary = new Dictionary<long, Expirable<PlayerCompetitiveState>>(8);
		}

		public Dictionary<long, Expirable<PlayerCompetitiveState>> PlayersCompetitiveStateDictionary { get; private set; }
	}
}
