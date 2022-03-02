using System;
using System.Diagnostics;

namespace HeavyMetalMachines.Announcer
{
	public class FakeAnnouncerService : IAnnouncerService
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<QueuedAnnouncerLog> ListenToEvent;

		public void RaiseEvent()
		{
		}
	}
}
