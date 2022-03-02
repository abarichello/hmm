using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.CompetitiveMode.View
{
	public interface ICompetitiveModeView
	{
		ITitle Title { get; }

		ICanvas MainCanvas { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IButton BackButton { get; }

		IButton OpenQueuePeriodsButton { get; }

		IButton OpenDivisionsButton { get; }

		IButton OpenRewardsButton { get; }

		IButton OpenRankingButton { get; }

		ILabel SeasonNameLabel { get; }

		ILabel SeasonStartDateLabel { get; }

		ILabel SeasonStartTimeLabel { get; }

		ILabel SeasonEndDateLabel { get; }

		ILabel SeasonEndTimeLabel { get; }

		ILabel NextQueuePeriodOpenDateLabel { get; }

		ILabel NextQueuePeriodOpenTimeLabel { get; }

		ILabel InformationLabel { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
