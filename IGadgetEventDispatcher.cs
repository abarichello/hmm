using System;
using HeavyMetalMachines.Combat.GadgetScript;

namespace HeavyMetalMachines
{
	public interface IGadgetEventDispatcher
	{
		void SendEvent(IHMMGadgetContext gadgetContext, IHMMEventContext eventContext);

		void SendAllEvents(byte address);
	}
}
