using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Loading
{
	public interface ILoadingVersusPlayerRankView
	{
		IDynamicImage RankDynamicImage { get; }

		IActivatable RankGroup { get; }
	}
}
