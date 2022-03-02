using System;
using HeavyMetalMachines.Combat.Gadget;

namespace HeavyMetalMachines.QuickChat
{
	public interface ISendGadgetInputCommand
	{
		void Send(GadgetSlot slot);
	}
}
