using System;
using System.Collections.Generic;
using HeavyMetalMachines.ExpirableStorage;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public interface IQueueConfigurationStorage
	{
		IDictionary<QueueConfigurationKey, Expirable<QueueConfiguration>> QueueConfigurations { get; }

		bool TryGet(string queueName, string regionName, out Expirable<QueueConfiguration> queueConfiguration);
	}
}
