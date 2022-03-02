using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public interface ICompetitiveSeasonRewardsCollectionPresenter : IPresenter
	{
		bool ShouldShow();
	}
}
