using System;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public class SeasonsStorage : ISeasonsStorage
	{
		public CompetitiveSeason CurrentSeason { get; set; }

		public CompetitiveSeason NextSeason { get; set; }
	}
}
