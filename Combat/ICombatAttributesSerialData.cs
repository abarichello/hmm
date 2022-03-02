using System;
using System.Collections.Generic;
using HeavyMetalMachines.Playback.Snapshot;

namespace HeavyMetalMachines.Combat
{
	public interface ICombatAttributesSerialData : IBaseStreamSerialData<ICombatAttributesSerialData>
	{
		float HPMax { get; }

		float HPMaxPct { get; }

		float HPRegen { get; }

		float HPRegenPct { get; }

		float HPPartialRegenPct { get; }

		float HPPureDamage { get; }

		float HPPureDamagePct { get; }

		float HPPureArmor { get; }

		float HPPureArmorPct { get; }

		float EPMax { get; }

		float EPMaxPct { get; }

		float EPRegen { get; }

		float EPRegenPct { get; }

		float EPPartialRegenPct { get; }

		float AccelerationModPct { get; }

		float AccelerationMod { get; }

		float BackAccelModPct { get; }

		float BackAccelMod { get; }

		float BrakeAccelModPct { get; }

		float BrakeAccelMod { get; }

		float GripExtraFwdAccelModPct { get; }

		float GripExtraFwdAccelMod { get; }

		float GripExtraBackAccelModPct { get; }

		float GripExtraBackAccelMod { get; }

		float DragMod { get; }

		float DragModPct { get; }

		float DriftDragMod { get; }

		float DriftDragModPct { get; }

		float LateralFriction { get; }

		float LateralFrictionPct { get; }

		float HPLightDamage { get; }

		float HPLightDamagePct { get; }

		float HPLightArmor { get; }

		float HPLightArmorPct { get; }

		float HPHeavyDamage { get; }

		float HPHeavyDamagePct { get; }

		float HPHeavyArmor { get; }

		float HPHeavyArmorPct { get; }

		float HPRepairArmor { get; }

		float HPRepairArmorPct { get; }

		float FireRate { get; }

		float FireRatePct { get; }

		int ScrapBonus { get; }

		float ScrapBonusPct { get; }

		float PowerPct { get; }

		float Mass { get; }

		float MassPct { get; }

		float PushForcePct { get; }

		float PushReceivedPct { get; }

		float SceneryBouncinessPct { get; }

		float TurningRadiusPct { get; }

		float TurningRadius { get; }

		float MaxAngularPushPct { get; }

		float MaxAngularPush { get; }

		float ForcedAngularPush { get; }

		float MaxAngularSpeed { get; }

		float MaxAngularSpeedPct { get; }

		float CooldownReductionGadget0 { get; }

		float CooldownReductionGadget1 { get; }

		float CooldownReductionGadget2 { get; }

		float CooldownReductionGadgetB { get; }

		float CooldownReductionGadget0Pct { get; }

		float CooldownReductionGadget1Pct { get; }

		float CooldownReductionGadget2Pct { get; }

		float CooldownReductionGadgetBPct { get; }

		float CrowdControlReduction { get; }

		StatusKind CurrentStatus { get; }

		bool ForceInvincible { get; }

		HashSet<int> Status { get; }
	}
}
