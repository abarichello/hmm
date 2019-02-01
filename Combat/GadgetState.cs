using System;

namespace HeavyMetalMachines.Combat
{
	public enum GadgetState : byte
	{
		Toggled,
		Waiting,
		Cooldown,
		NotActive,
		None,
		Ready,
		CoolingAfterOverheat
	}
}
