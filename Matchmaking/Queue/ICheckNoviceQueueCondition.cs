using System;

namespace HeavyMetalMachines.Matchmaking.Queue
{
	public interface ICheckNoviceQueueCondition
	{
		bool ShouldGoToNoviceQueue();
	}
}
