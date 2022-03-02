using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public interface ICompetitiveRankingView
	{
		ICanvas MainCanvas { get; }

		ITitle Title { get; }

		IToggle ShowGlobalToggle { get; }

		IToggle ShowFriendsToggle { get; }

		ILabel ShowGlobalToggleLabel { get; }

		ILabel ShowFriendsToggleLabel { get; }

		IButton BackButton { get; }

		IAnimation FadeInAnimation { get; }

		IAnimation FadeOutAnimation { get; }

		IActivatable CalibratingStateGroup { get; }

		ILabel CalibrationTotalMatchesLabel { get; }

		ILabel CalibrationMatchesPlayedLabel { get; }

		IActivatable DivisionGroup { get; }

		ILabel DivisionNameLabel { get; }

		ILabel RankScoreLabel { get; }

		ILabel TopPositionLabel { get; }

		IEnumerable<IRankBadgeComponents> RanksBadgesComponents { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
