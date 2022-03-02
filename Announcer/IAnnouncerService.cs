using System;

namespace HeavyMetalMachines.Announcer
{
	public interface IAnnouncerService
	{
		event Action<QueuedAnnouncerLog> ListenToEvent;
	}
}
