using System;
using System.Collections.Generic;
using HeavyMetalMachines.ExpirableStorage;

namespace HeavyMetalMachines.CompetitiveMode.Players
{
	public interface IPlayersStateStorage
	{
		Dictionary<long, Expirable<PlayerCompetitiveState>> PlayersCompetitiveStateDictionary { get; }
	}
}
