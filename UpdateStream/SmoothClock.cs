using System;
using Hoplon.Time;
using Pocketverse;

namespace HeavyMetalMachines.UpdateStream
{
	public class SmoothClock : IClock
	{
		public SmoothClock(TimelineConfiguration configuration)
		{
			this.config = configuration;
			this.delay = configuration.ClockDelay;
		}

		public double GetTime()
		{
			HMMHub hub = GameHubBehaviour.Hub;
			double num = (double)(hub.GameTime.GetPlaybackTime() + hub.GameTime.RewindedTimeMillis) / 1000.0;
			if (hub.GameTime.RewindedTimeMillis == 0)
			{
				num -= (double)hub.Net.GetPing() / 2000.0;
				num -= this.delay;
			}
			return num;
		}

		private TimelineConfiguration config;

		private double delay;
	}
}
