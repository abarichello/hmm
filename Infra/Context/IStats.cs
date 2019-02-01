using System;

namespace HeavyMetalMachines.Infra.Context
{
	public interface IStats
	{
		IPlayerStats GetPlayerStats(int player);

		IMatchStats MatchStats { get; }
	}
}
