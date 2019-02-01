using System;

namespace HeavyMetalMachines.Combat
{
	public static class PerkUsageKindUtil
	{
		public static bool HasFlag(this PerkUsageKind kind, PerkUsageKind other)
		{
			return (kind & other) == other;
		}
	}
}
