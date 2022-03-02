using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.CompetitiveMode.View.Matchmaking
{
	public interface ICompetitiveQueueJoinView
	{
		IButton JoinButton { get; }

		IActivatable UnjoinableIndicator { get; }

		IAnimation ShowGroupAnimation { get; }

		IAnimation HideGroupAnimation { get; }

		IActivatable ModeLockedGroup { get; }

		ILabel MatchesPlayedToUnlockLabel { get; }

		ILabel TotalMatchesToUnlockLabel { get; }

		IActivatable WaitingNextQueuePeriodGroup { get; }

		ILabel NextQueuePeriodOpenDateLabel { get; }

		ILabel NextQueuePeriodOpenTimeLabel { get; }

		IActivatable UnjoinableQueueGroup { get; }

		ILabel UnjoinableQueueMessageLabel { get; }

		IActivatable CalibrationGroup { get; }

		ILabel MatchesPlayedToCalibrateLabel { get; }

		ILabel TotalMatchesToCalibrateLabel { get; }

		IActivatable BanGroup { get; }

		ILabel BanTimerLabel { get; }

		IActivatable RankedGroup { get; }

		ILabel ScoreLabel { get; }

		IDynamicImage DivisionImage { get; }

		IActivatable ScoreWithTopPlacementGroup { get; }

		ILabel TopPlacementLabel { get; }

		ILabel ScoreWithTopPlacementLabel { get; }
	}
}
