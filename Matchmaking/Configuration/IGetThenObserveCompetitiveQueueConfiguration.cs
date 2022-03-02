using System;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public interface IGetThenObserveCompetitiveQueueConfiguration
	{
		IObservable<QueueConfiguration> GetThenObserve();
	}
}
