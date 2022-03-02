using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public interface ICompetitiveQueuePeriodsView
	{
		ICanvas MainCanvas { get; }

		IButton BackButton { get; }

		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		IEnumerable<ICompetitiveQueueDayView> QueueDays { get; }

		IDropdown<string> RegionDropdown { get; }

		IAnimation ValuesChangeInAnimation { get; }

		IAnimation ValuesChangeOutAnimation { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }
	}
}
