using System;
using HeavyMetalMachines.Combat.Gadget;

namespace HeavyMetalMachines
{
	public interface ICurrentPlayerController
	{
		void AddGadgetCommand(GadgetSlot slot);
	}
}
