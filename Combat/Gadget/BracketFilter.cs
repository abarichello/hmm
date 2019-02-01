using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Flags]
	public enum BracketFilter
	{
		None = 0,
		PrioritizePlayers = 1,
		OnlyClosest = 2,
		IgnoreCaster = 4,
		OnlyPlayers = 8
	}
}
