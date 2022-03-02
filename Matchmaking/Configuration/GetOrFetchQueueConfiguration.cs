using System;
using HeavyMetalMachines.ExpirableStorage;
using Hoplon.Reactive;
using Hoplon.Time;
using UniRx;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public class GetOrFetchQueueConfiguration : IGetOrFetchQueueConfiguration
	{
		public GetOrFetchQueueConfiguration(IQueueConfigurationStorage queueConfigurationStorage, IQueueConfigurationProvider queueConfigurationProvider, IAddOrUpdateQueueConfiguration addOrUpdateQueueConfiguration, ICurrentTime currentTime)
		{
			this._queueConfigurationStorage = queueConfigurationStorage;
			this._queueConfigurationProvider = queueConfigurationProvider;
			this._addOrUpdateQueueConfiguration = addOrUpdateQueueConfiguration;
			this._currentTime = currentTime;
		}

		public IObservable<QueueConfiguration> GetOrFetch(string queueName, string regionName)
		{
			GetOrFetchQueueConfiguration.AssertValidParameters(queueName, regionName);
			return ObservableExtensions.IfElse<Expirable<QueueConfiguration>, QueueConfiguration>(Observable.Return<Expirable<QueueConfiguration>>(this.GetFromStorage(queueName, regionName)), new Func<Expirable<QueueConfiguration>, bool>(this.IsNullOrExpired), (Expirable<QueueConfiguration> _) => this.GetFromProviderAndStoresIntoStorage(queueName, regionName), (Expirable<QueueConfiguration> expirableQueueConfiguration) => Observable.Return<QueueConfiguration>(expirableQueueConfiguration.Value));
		}

		private bool IsNullOrExpired(Expirable<QueueConfiguration> expirableQueueConfiguration)
		{
			return expirableQueueConfiguration == null || expirableQueueConfiguration.IsExpired(this._currentTime);
		}

		private Expirable<QueueConfiguration> GetFromStorage(string queueName, string regionName)
		{
			Expirable<QueueConfiguration> result;
			this._queueConfigurationStorage.TryGet(queueName, regionName, out result);
			return result;
		}

		private IObservable<QueueConfiguration> GetFromProviderAndStoresIntoStorage(string queueName, string regionName)
		{
			return Observable.Do<QueueConfiguration>(this._queueConfigurationProvider.Get(queueName, regionName), delegate(QueueConfiguration queueConfiguration)
			{
				this._addOrUpdateQueueConfiguration.AddOrUpdate(queueConfiguration);
			});
		}

		private static void AssertValidParameters(string queueName, string regionName)
		{
		}

		private readonly IQueueConfigurationStorage _queueConfigurationStorage;

		private readonly IQueueConfigurationProvider _queueConfigurationProvider;

		private readonly IAddOrUpdateQueueConfiguration _addOrUpdateQueueConfiguration;

		private readonly ICurrentTime _currentTime;
	}
}
