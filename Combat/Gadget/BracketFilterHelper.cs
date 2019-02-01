using System;

namespace HeavyMetalMachines.Combat.Gadget
{
	public static class BracketFilterHelper
	{
		public static bool HasFlag(this BracketFilter kind, BracketFilter other)
		{
			return (kind & other) == other;
		}
	}
}
