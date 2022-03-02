using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public interface ICompetitiveMatchResultRankedDivisionView
	{
		IActivatable Group { get; }

		IAnimation DivisionUpAnimation { get; }

		IAnimation DivisionDownAnimation { get; }

		IAnimation SubdivisionUpAnimation { get; }

		IAnimation SubdivisionDownAnimation { get; }

		IAnimation IdleAnimation { get; }

		IAnimation GlowAnimation { get; }

		IDynamicImage AnimatedDivisionImage { get; }

		IDynamicImage StaticDivisionImage { get; }

		IDynamicImage CurrentSubdivisionImage { get; }

		IDynamicImage PreviousSubdivisionImage { get; }

		ILabel PreviousTopPlacementLabel { get; }

		ILabel TopPlacementLabel { get; }
	}
}
