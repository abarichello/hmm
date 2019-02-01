using System;

namespace HeavyMetalMachines.Combat
{
	public enum AttributeBuffKind
	{
		None,
		HPMax,
		HPRegen,
		HPPureDamageBuff,
		HPPureArmor,
		EPMax = 6,
		EPRegen,
		ObsoleteSpeedMax,
		AngSpeed,
		HPLightDamageBuff,
		HPLightArmor,
		FireRate,
		HPHeavyDamageBuff,
		HPHeavyArmor,
		ScrapBonus,
		CooldownReduction,
		CrowdControlReduction,
		Power,
		HpPartialRegen,
		EpPartialRegen,
		DriftMod,
		ForwardAcceleration,
		SupressTargetTag,
		Drag,
		PushForce,
		PushReceived,
		BackwardAcceleration,
		BrakeAcceleration,
		GripExtraFwdAcceleration,
		GripExtraBackAcceleration,
		SceneryBounciness,
		TurningRadius,
		DriftDrag,
		LateralFriction,
		MaxAngularPush,
		ForcedAngularPush,
		MaxAngularSpeed,
		Mass,
		HPRepairArmor
	}
}
