using System;

namespace HeavyMetalMachines.Combat
{
	public static class GadgetNatureKindEx
	{
		public static bool HasFlag(this GadgetNatureKind kind, GadgetNatureKind other)
		{
			return (kind & other) == other;
		}
	}
}
