using System;
using Hoplon.Time;

namespace HeavyMetalMachines.Matchmaking.Configuration
{
	public class QueueConfiguration
	{
		public QueuePeriod GetCurrentOrNextPeriod(ICurrentTime currentTime)
		{
			DateTime t = currentTime.NowServerUtc();
			foreach (QueuePeriod result in this.QueuePeriods)
			{
				if (t <= result.CloseDateTimeUtc)
				{
					return result;
				}
			}
			throw new Exception("There is no current or next queue period.");
		}

		public string QueueName;

		public string RegionName;

		public QueuePeriod[] QueuePeriods;

		public QueueConfigCharacterData[] LockedCharacters;

		public QueueConfigArenaData[] AvailableArenas;
	}
}
