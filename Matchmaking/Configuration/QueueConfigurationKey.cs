using System;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public class QueueConfigurationKey
	{
		public QueueConfigurationKey(string queueName, string regionName)
		{
			this.QueueName = queueName;
			this.RegionName = regionName;
		}

		public override bool Equals(object obj)
		{
			QueueConfigurationKey queueConfigurationKey = (QueueConfigurationKey)obj;
			return this.QueueName.Equals(queueConfigurationKey.QueueName) && this.RegionName.Equals(queueConfigurationKey.RegionName);
		}

		public override int GetHashCode()
		{
			return (this.QueueName + this.RegionName).GetHashCode();
		}

		public readonly string QueueName;

		public readonly string RegionName;
	}
}
