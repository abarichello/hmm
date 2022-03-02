using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Profile
{
	public interface IPlayerProfileView
	{
		IActivatable MainGroup { get; }

		IDynamicImage CurrentRankDynamicImage { get; }

		ILabel CurrentRankLabel { get; }

		ILabel CurrentScoreLabel { get; }

		IActivatable CurrentTopPlacementPositionGroup { get; }

		ILabel CurrentTopPlacementPositionLabel { get; }

		IDynamicImage TooltipCurrentRankDynamicImage { get; }

		ILabel TooltipCurrentRankLabel { get; }

		IDynamicImage TooltipTopRankDynamicImage { get; }

		ILabel TooltipTopRankLabel { get; }

		IGrid CurrentRankGrid { get; }
	}
}
