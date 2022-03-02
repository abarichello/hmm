using System;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.CompetitiveMode.Seasons;

namespace HeavyMetalMachines.CompetitiveMode.Prizes
{
	public class CompetitivePrizesCollection
	{
		public bool HasCollected;

		public CollectedPrize[] CollectedPrizes;

		public CompetitiveRank PrizeRank;

		public CompetitiveSeason Season;
	}
}
