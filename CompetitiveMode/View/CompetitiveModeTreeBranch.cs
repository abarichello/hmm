using System;
using HeavyMetalMachines.Presenting.Navigation;

namespace HeavyMetalMachines.CompetitiveMode.View
{
	public class CompetitiveModeTreeBranch
	{
		public IPresenterNode DivisionsNode { get; set; }

		public IPresenterNode RewardsNode { get; set; }

		public IPresenterNode RankingNode { get; set; }

		public IPresenterNode QueuePeriodsNode { get; set; }
	}
}
