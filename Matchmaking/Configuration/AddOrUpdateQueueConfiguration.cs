using System;
using HeavyMetalMachines.ExpirableStorage;
using Hoplon.Time;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public class AddOrUpdateQueueConfiguration : IAddOrUpdateQueueConfiguration
	{
		public AddOrUpdateQueueConfiguration(IQueueConfigurationStorage queueConfigurationStorage, ICurrentTime currentTime)
		{
			this._queueConfigurationStorage = queueConfigurationStorage;
			this._currentTime = currentTime;
		}

		public void AddOrUpdate(QueueConfiguration queueConfiguration)
		{
			AddOrUpdateQueueConfiguration.AssertValidQueueConfiguration(queueConfiguration);
			DateTime expirationDate = this.GetExpirationDate();
			QueueConfigurationKey key = new QueueConfigurationKey(queueConfiguration.QueueName, queueConfiguration.RegionName);
			this._queueConfigurationStorage.QueueConfigurations[key] = new Expirable<QueueConfiguration>(queueConfiguration, expirationDate);
		}

		private DateTime GetExpirationDate()
		{
			return this._currentTime.Now() + AddOrUpdateQueueConfiguration._expirationTime;
		}

		private static void AssertValidQueueConfiguration(QueueConfiguration queueConfiguration)
		{
			if (queueConfiguration == null)
			{
				throw new ArgumentNullException("queueConfiguration");
			}
			if (string.IsNullOrEmpty(queueConfiguration.QueueName))
			{
				throw new ArgumentException("Cannot add a queueConfiguration with null QueueName.", "queueConfiguration");
			}
			if (string.IsNullOrEmpty(queueConfiguration.RegionName))
			{
				throw new ArgumentException("Cannot add a queueConfiguration with null RegionName.", "queueConfiguration");
			}
		}

		private readonly IQueueConfigurationStorage _queueConfigurationStorage;

		private readonly ICurrentTime _currentTime;

		private static readonly TimeSpan _expirationTime = TimeSpan.FromMinutes(5.0);
	}
}
