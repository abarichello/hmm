using System;

namespace HeavyMetalMachines
{
	public interface IStatsDispatcher
	{
		void SendUpdate();

		void SendFullUpdate(byte to);
	}
}
