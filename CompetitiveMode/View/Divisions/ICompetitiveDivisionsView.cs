using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.CompetitiveMode.View.Divisions
{
	public interface ICompetitiveDivisionsView
	{
		ICanvas MainCanvas { get; }

		IButton BackButton { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		ILabel CalibrationMatchesPlayedIncompleteLabel { get; }

		ILabel CalibrationMatchesPlayedCompletedLabel { get; }

		ILabel CalibrationTotalMatchesLabel { get; }

		ILabel CalibrationDescriptionLabel { get; }

		ILabel SubdivisionsDescriptionLabel { get; }

		ILabel DivisionTopRankingDescriptionLabel { get; }

		IEnumerable<ICompetitiveDivisionView> DivisionViews { get; }

		IEnumerable<ICompetitiveSubdivisionView> SubdivisionViews { get; }

		ICompetitiveDivisionView TopPlacementDivisionView { get; }

		IActivatable SubdivisionsDescriptionActivatable { get; }

		IActivatable TopPlacementDescriptionActivatable { get; }

		IActivatable TopPlacementPositionActivatable { get; }

		ILabel TopPlacementDescriptionLabel { get; }

		ILabel TopPlacementPositionLabel { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		void SetTitleInfo(string title, string subTitle, string description);
	}
}
