using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines
{
	public interface IMatchPlayers
	{
		List<PlayerData> GetPlayersAndBotsByTeam(TeamKind team);
	}
}
