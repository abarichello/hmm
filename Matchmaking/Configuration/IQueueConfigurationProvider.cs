using System;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public interface IQueueConfigurationProvider
	{
		IObservable<QueueConfiguration> Get(string queueName, string regionName);
	}
}
