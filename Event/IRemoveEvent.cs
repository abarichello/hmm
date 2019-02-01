using System;

namespace HeavyMetalMachines.Event
{
	public interface IRemoveEvent
	{
		int TargetEventId { get; }
	}
}
