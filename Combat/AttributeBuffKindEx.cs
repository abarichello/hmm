using System;

namespace HeavyMetalMachines.Combat
{
	public static class AttributeBuffKindEx
	{
		public static bool IsPurgeable(this AttributeBuffKind att, float amount)
		{
			return att.IsCrowdControl(-amount);
		}

		public static bool IsDispellable(this AttributeBuffKind att, float amount)
		{
			return att.IsCrowdControl(amount);
		}

		public static bool IsCrowdControl(this AttributeBuffKind att, float amount)
		{
			switch (att)
			{
			case AttributeBuffKind.HPPureDamageBuff:
			case AttributeBuffKind.HPPureArmor:
			case AttributeBuffKind.ObsoleteSpeedMax:
			case AttributeBuffKind.HPLightDamageBuff:
			case AttributeBuffKind.HPLightArmor:
			case AttributeBuffKind.HPHeavyDamageBuff:
			case AttributeBuffKind.HPHeavyArmor:
			case AttributeBuffKind.CooldownReduction:
				return amount < 0f;
			}
			return false;
		}
	}
}
