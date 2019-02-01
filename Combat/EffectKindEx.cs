using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public static class EffectKindEx
	{
		public static void WriteEffectKind(this BitStream stream, int effect)
		{
			stream.WriteBits(5, effect);
		}

		public static void WriteEffectKind(this BitStream stream, EffectKind effect)
		{
			stream.WriteBits(5, (int)effect);
		}

		public static EffectKind ReadEffectKind(this BitStream stream)
		{
			return (EffectKind)stream.ReadBits(5);
		}

		public static int ReadEffectKindAsInt(this BitStream stream)
		{
			return stream.ReadBits(5);
		}

		public static bool IsPurgeable(this EffectKind effect, float amount)
		{
			switch (effect)
			{
			case EffectKind.HPRepair:
			case EffectKind.EPRepair:
				break;
			default:
				if (effect != EffectKind.CooldownRepair && effect != EffectKind.ModifyParameter)
				{
					return false;
				}
				break;
			}
			return amount > 0f;
		}

		public static bool IsDispellable(this EffectKind effect, float lifeTime)
		{
			return effect.IsDamage() && lifeTime > 0f;
		}

		public static bool IsDamage(this EffectKind effect)
		{
			switch (effect)
			{
			case EffectKind.HPPureDamage:
			case EffectKind.HPPureDamageNL:
			case EffectKind.EPDmg:
				break;
			default:
				if (effect != EffectKind.HPLightDamage && effect != EffectKind.HPHeavyDamage && effect != EffectKind.HPGodDamage)
				{
					return false;
				}
				break;
			}
			return true;
		}

		public static bool IsHPDamage(this EffectKind effect)
		{
			return effect == EffectKind.HPPureDamage || effect == EffectKind.HPPureDamageNL || effect == EffectKind.HPLightDamage || effect == EffectKind.HPHeavyDamage || effect == EffectKind.HPGodDamage;
		}

		public static bool IgnoreOnDeath(this EffectKind effect)
		{
			switch (effect)
			{
			case EffectKind.HPPureDamage:
			case EffectKind.HPPureDamageNL:
			case EffectKind.HPRepair:
			case EffectKind.EPDmg:
			case EffectKind.EPRepair:
			case EffectKind.HPLightDamage:
			case EffectKind.HPHeavyDamage:
				break;
			default:
				if (effect != EffectKind.HPGodDamage)
				{
					return false;
				}
				break;
			}
			return true;
		}
	}
}
