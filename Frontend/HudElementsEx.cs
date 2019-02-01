using System;

namespace HeavyMetalMachines.Frontend
{
	internal static class HudElementsEx
	{
		public static bool HasFlag(this GameGui.HudElement kind, GameGui.HudElement other)
		{
			return (kind & other) == other;
		}
	}
}
