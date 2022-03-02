using System;
using HeavyMetalMachines.Combat.Gadget;

namespace HeavyMetalMachines.Render
{
	public interface IActivatableGadgetFeedback
	{
		bool IsActive { get; set; }

		GadgetSlot Slot { get; }
	}
}
