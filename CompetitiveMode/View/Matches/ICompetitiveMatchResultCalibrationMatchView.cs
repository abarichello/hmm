using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public interface ICompetitiveMatchResultCalibrationMatchView
	{
		IActivatable MatchWonGroup { get; }

		IActivatable MatchLostGroup { get; }

		IAnimation RaiseAnimation { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IAlpha WonAlpha { get; }

		IAlpha LostAlpha { get; }

		void SetWonImage();

		void SetLostImage();
	}
}
