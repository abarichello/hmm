using System;
using HeavyMetalMachines.Durango;
using HeavyMetalMachines.Orbis;

namespace HeavyMetalMachines.HostingPlatforms
{
	public class GetHostPlatform : IGetHostPlatform
	{
		public HostPlatform GetCurrent()
		{
			if (Platform.Current is OrbisPlatform)
			{
				return 1;
			}
			if (Platform.Current is DurangoPlatform)
			{
				return 2;
			}
			return 0;
		}
	}
}
