using System;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public interface IGetCompetitiveQueueConfiguration
	{
		IObservable<QueueConfiguration> GetForRegion(string regionName);
	}
}
