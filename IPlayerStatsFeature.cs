using System;
using HeavyMetalMachines.Bank;

namespace HeavyMetalMachines
{
	public interface IPlayerStatsFeature
	{
		int BlueTeamDeaths { get; set; }

		int RedTeamDeaths { get; set; }

		IPlayerStatsSerialData GetStats(int objectId);
	}
}
