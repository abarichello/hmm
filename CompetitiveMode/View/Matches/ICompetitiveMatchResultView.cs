using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public interface ICompetitiveMatchResultView
	{
		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		ICompetitiveMatchResultCalibrationView CalibrationView { get; }

		ICompetitiveMatchResultRankedView RankedView { get; }
	}
}
