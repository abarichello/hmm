using System;

namespace HeavyMetalMachines.Spectator
{
	public interface IGetSpectatorQueueFilter
	{
		CrossplayPlatformQueueSettings Get();
	}
}
