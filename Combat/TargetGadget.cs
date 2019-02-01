using System;

namespace HeavyMetalMachines.Combat
{
	[Flags]
	public enum TargetGadget : byte
	{
		None = 0,
		Gadget0 = 1,
		Gadget1 = 2,
		Gadget2 = 4,
		GadgetBoost = 8,
		Gadgets01 = 3,
		Gadgets12 = 6,
		All = 15
	}
}
