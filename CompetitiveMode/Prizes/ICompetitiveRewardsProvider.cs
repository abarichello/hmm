using System;

namespace HeavyMetalMachines.CompetitiveMode.Prizes
{
	public interface ICompetitiveRewardsProvider
	{
		CompetitiveReward[] GetRewards(Guid[] itemTypeIds);
	}
}
