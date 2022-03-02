using System;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public interface IGetOrFetchQueueConfiguration
	{
		IObservable<QueueConfiguration> GetOrFetch(string queueName, string regionName);
	}
}
