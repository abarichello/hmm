using System;

namespace HeavyMetalMachines.MatchMakingQueue.Configuration.Exceptions
{
	public class QueueConfigurationNotFoundException : Exception
	{
		public QueueConfigurationNotFoundException(string queueName, string regionName) : base(string.Format("A queue configuration with queue name {0} and region name {1} was not found.", queueName, regionName))
		{
		}
	}
}
