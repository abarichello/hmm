using System;

namespace HeavyMetalMachines.Combat
{
	public static class StatusKindEx
	{
		public static bool IsPurgeable(this StatusKind stat)
		{
			return stat == StatusKind.Invulnerable || stat == StatusKind.Unstoppable || stat == StatusKind.Phasing || stat == StatusKind.Invisible;
		}

		public static bool IsDispellable(this StatusKind stat)
		{
			return stat.IsCrowdControl();
		}

		public static bool IsCrowdControl(this StatusKind stat)
		{
			switch (stat)
			{
			case StatusKind.Immobilized:
			case StatusKind.Disarmed:
			case StatusKind.Jammed:
				break;
			default:
				if (stat != StatusKind.HpUnhealable)
				{
					return false;
				}
				break;
			}
			return true;
		}

		public static bool HasFlag(this StatusKind kind, StatusKind other)
		{
			return (kind & other) == other;
		}
	}
}
