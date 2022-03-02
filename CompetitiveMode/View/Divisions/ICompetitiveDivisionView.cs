using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Divisions
{
	public interface ICompetitiveDivisionView
	{
		IToggle Toggle { get; }

		ILabel ScoreRangesLabel { get; }

		IActivatable PlayerDivisionIndicator { get; }

		IAnimation Animation { get; }
	}
}
