using System;

namespace HeavyMetalMachines.Match.Infra
{
	public interface IMatchPlayersProvider
	{
		IMatchPlayers GetMatchPlayers { get; }
	}
}
