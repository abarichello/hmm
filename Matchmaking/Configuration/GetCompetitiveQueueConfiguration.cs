using System;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public class GetCompetitiveQueueConfiguration : IGetCompetitiveQueueConfiguration
	{
		public GetCompetitiveQueueConfiguration(IGetOrFetchQueueConfiguration getOrFetchQueueConfiguration)
		{
			this._getOrFetchQueueConfiguration = getOrFetchQueueConfiguration;
		}

		public IObservable<QueueConfiguration> GetForRegion(string regionName)
		{
			return this._getOrFetchQueueConfiguration.GetOrFetch("Ranked", regionName);
		}

		private readonly IGetOrFetchQueueConfiguration _getOrFetchQueueConfiguration;
	}
}
