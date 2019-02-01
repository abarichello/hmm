using System;

namespace HeavyMetalMachines.Event
{
	public interface IEventContent
	{
		int EventTime { get; set; }

		bool ShouldBuffer();

		EventScopeKind GetKind();
	}
}
