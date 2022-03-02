using System;

namespace HeavyMetalMachines.Event
{
	public interface IEventManagerDispatcher
	{
		void Send(EventData e);

		void SendFullFrame(byte to);
	}
}
