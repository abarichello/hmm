using System;
using HeavyMetalMachines.CompetitiveMode.View.Matches;

namespace HeavyMetalMachines.CompetitiveMode.Seasons
{
	public class SkipSwordfishSeasonsStorage : ISeasonsStorage
	{
		public SkipSwordfishSeasonsStorage()
		{
			FakeCompetitiveSeasonsProvider fakeCompetitiveSeasonsProvider = new FakeCompetitiveSeasonsProvider();
			this.CurrentSeason = fakeCompetitiveSeasonsProvider.GetMockedCurrentSeason();
		}

		public CompetitiveSeason CurrentSeason { get; set; }

		public CompetitiveSeason NextSeason { get; set; }
	}
}
