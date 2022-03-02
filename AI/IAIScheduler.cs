using System;

namespace HeavyMetalMachines.AI
{
	public interface IAIScheduler
	{
		void AddTask(IAITask task);

		void RemoveTask(IAITask task);
	}
}
