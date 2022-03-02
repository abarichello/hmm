using System;
using HeavyMetalMachines.HostingPlatforms;

namespace HeavyMetalMachines.Spectator
{
	[Serializable]
	public struct CrossplayPlatformQueueKey
	{
		public CrossplayPlatformQueueKey(HostPlatform platform, bool isCrossplayEnabled)
		{
			this.Platform = platform;
			this.IsCrossplayEnabled = isCrossplayEnabled;
		}

		public HostPlatform Platform;

		public bool IsCrossplayEnabled;
	}
}
