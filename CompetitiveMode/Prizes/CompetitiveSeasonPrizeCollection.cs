using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Seasons;
using HeavyMetalMachines.Store.Business;

namespace HeavyMetalMachines.CompetitiveMode.Prizes
{
	public class CompetitiveSeasonPrizeCollection
	{
		public CompetitiveRank? PrizeRank;

		public Item[] CollectedItems;

		public CompetitiveSeason Season;
	}
}
