using System;
using HeavyMetalMachines.MatchMakingQueue.Configuration.Exceptions;
using HeavyMetalMachines.Regions.Business;
using Hoplon.Logging;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public class GetThenObserveCompetitiveQueueConfiguration : IGetThenObserveCompetitiveQueueConfiguration
	{
		public GetThenObserveCompetitiveQueueConfiguration(IGetThenObserveChosenRegionChanged getThenObserveChosenRegionChanged, IGetOrFetchQueueConfiguration getOrFetchQueueConfiguration, ILogger<GetThenObserveCompetitiveQueueConfiguration> logger)
		{
			this._getThenObserveChosenRegionChanged = getThenObserveChosenRegionChanged;
			this._getOrFetchQueueConfiguration = getOrFetchQueueConfiguration;
			this._logger = logger;
		}

		public IObservable<QueueConfiguration> GetThenObserve()
		{
			return Observable.Catch<QueueConfiguration, QueueConfigurationNotFoundException>(Observable.SelectMany<Region, QueueConfiguration>(this._getThenObserveChosenRegionChanged.GetThenObserve(), (Region region) => this.GetOrFetchQueueConfigurationForRegion(region)), new Func<QueueConfigurationNotFoundException, IObservable<QueueConfiguration>>(this.LogExceptionAndReturnEmpty));
		}

		private IObservable<QueueConfiguration> GetOrFetchQueueConfigurationForRegion(Region region)
		{
			return this._getOrFetchQueueConfiguration.GetOrFetch("Ranked", region.Name);
		}

		private IObservable<QueueConfiguration> LogExceptionAndReturnEmpty(QueueConfigurationNotFoundException exception)
		{
			this._logger.ErrorFormat("Exception while fetching queue configuration from provider: {0}", new object[]
			{
				exception
			});
			return Observable.Empty<QueueConfiguration>();
		}

		private readonly IGetOrFetchQueueConfiguration _getOrFetchQueueConfiguration;

		private readonly IGetThenObserveChosenRegionChanged _getThenObserveChosenRegionChanged;

		private readonly ILogger<GetThenObserveCompetitiveQueueConfiguration> _logger;
	}
}
