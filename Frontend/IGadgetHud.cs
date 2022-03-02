using System;
using HeavyMetalMachines.Combat.Gadget;

namespace HeavyMetalMachines.Frontend
{
	public interface IGadgetHud
	{
		IGadgetHudElement GetElement(GadgetSlot slot);
	}
}
