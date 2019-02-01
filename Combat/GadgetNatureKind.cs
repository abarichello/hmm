using System;

namespace HeavyMetalMachines.Combat
{
	[Flags]
	public enum GadgetNatureKind
	{
		Damage = 1,
		Healing = 2,
		SpeedBoost = 4,
		Teleport = 8
	}
}
