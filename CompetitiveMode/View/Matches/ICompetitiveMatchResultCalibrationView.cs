using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public interface ICompetitiveMatchResultCalibrationView
	{
		IActivatable Group { get; }

		IButton ContinueButton { get; }

		ILabel SeasonNameLabel { get; }

		ILabel MatchesPlayedLabel { get; }

		ILabel TotalMatchesLabel { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IAnimation TransitionAnimation { get; }

		IEnumerable<ICompetitiveMatchResultCalibrationMatchView> MatchesViews { get; }

		int MillisecondsBetweenAnimationsOfMatches { get; }

		IAnimation ShowButtonAnimation { get; }

		IActivatable LeavingCalibrationAnimationGroup { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
