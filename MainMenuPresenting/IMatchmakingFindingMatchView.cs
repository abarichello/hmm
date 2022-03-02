using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public interface IMatchmakingFindingMatchView
	{
		IAnimation ShowAnimation { get; }

		IAnimation HideAnimation { get; }

		ILabel ElapsedWaitTimeLabel { get; }

		ILabel AverageWaitTimeLabel { get; }

		IButton CancelSearchButton { get; }
	}
}
