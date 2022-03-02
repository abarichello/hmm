using System;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public interface IAddOrUpdateQueueConfiguration
	{
		void AddOrUpdate(QueueConfiguration queueConfiguration);
	}
}
