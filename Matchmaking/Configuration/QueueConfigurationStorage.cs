using System;
using System.Collections.Generic;
using HeavyMetalMachines.ExpirableStorage;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public class QueueConfigurationStorage : IQueueConfigurationStorage
	{
		public QueueConfigurationStorage()
		{
			this.QueueConfigurations = new Dictionary<QueueConfigurationKey, Expirable<QueueConfiguration>>();
		}

		public IDictionary<QueueConfigurationKey, Expirable<QueueConfiguration>> QueueConfigurations { get; private set; }

		public bool TryGet(string queueName, string regionName, out Expirable<QueueConfiguration> queueConfiguration)
		{
			QueueConfigurationStorage.AssertValidParameters(queueName, regionName);
			QueueConfigurationKey key = new QueueConfigurationKey(queueName, regionName);
			return this.QueueConfigurations.TryGetValue(key, out queueConfiguration);
		}

		private static void AssertValidParameters(string queueName, string regionName)
		{
		}
	}
}
