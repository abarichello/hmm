using System;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public enum CompetitiveQueueUnjoinabilityReason
	{
		ModeIsLocked,
		PlayerIsNotLeaderOfGroup,
		GroupMemberHasNotUnlockedCompetitive,
		GroupMembersCountAboveLimit,
		QueueIsNotOpen,
		PlayerIsAlreadyInQueue,
		GroupMemberIsNotInMainMenu,
		PlayerIsBannedFromQueue
	}
}
