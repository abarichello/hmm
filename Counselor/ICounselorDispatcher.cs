using System;

namespace HeavyMetalMachines.Counselor
{
	public interface ICounselorDispatcher
	{
		void Send(byte targetPlayerAddress, int configIndex, bool isActive);
	}
}
