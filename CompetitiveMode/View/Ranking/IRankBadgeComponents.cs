using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Ranking
{
	public interface IRankBadgeComponents
	{
		IAnimation BadgeAnimation { get; }

		IActivatable Group { get; }

		IDynamicImage SubleagueDynamicImage { get; }
	}
}
