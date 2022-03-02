using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public interface ICompetitiveQueueDayView
	{
		ILabel DateLabel { get; }

		ILabel DayOfWeekLabel { get; }

		IImage BackgroundImage { get; }

		IActivatable EmptyPeriodsIndicator { get; }

		Color ActiveBackgroundColor { get; }

		Color ActiveDateColor { get; }

		Color ActiveDayOfWeekColor { get; }

		Color EmptyBackgroundColor { get; }

		Color EmptyDateColor { get; }

		Color EmptyDayOfWeekColor { get; }

		ICompetitiveQueueDayPeriodView CreateAndAddPeriod();

		void ClearPeriods();
	}
}
