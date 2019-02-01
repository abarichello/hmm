using System;

namespace HeavyMetalMachines.Combat
{
	public enum EffectKind
	{
		None,
		HPPureDamage = 11,
		HPPureDamageNL,
		HPRepair,
		EPDmg,
		EPRepair,
		Purge = 17,
		CooldownRepair = 19,
		HPLightDamage,
		HPHeavyDamage,
		Impulse,
		Dispel,
		Boost,
		OverheatRepair,
		AddCharge,
		HPGodDamage,
		ChargeRepair,
		HPTemp,
		ModifyParameter
	}
}
