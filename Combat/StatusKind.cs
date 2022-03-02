using System;

namespace HeavyMetalMachines.Combat
{
	[Flags]
	public enum StatusKind
	{
		None = 0,
		Immobilized = 1,
		Disarmed = 2,
		Jammed = 4,
		Oiled = 8,
		Invulnerable = 16,
		Unstoppable = 32,
		Blind = 64,
		Phasing = 128,
		ForcedVisible = 256,
		Invisible = 512,
		Revealed = 1024,
		Banished = 2048,
		HpUnhealable = 4096,
		BombTargetTriggerImmunity = 8192,
		Freeze = 16384,
		PassBlocked = 32768,
		Gadget0Disarmed = 65536,
		Gadget1Disarmed = 131072,
		Gadget2Disarmed = 262144,
		PassiveDisarmed = 524288,
		Indestructible = 1048576,
		HpTempBlocked = 2097152,
		Dead = 4194304,
		GrabberBlocked = 8388608,
		DamageDisarmed = 16777216,
		HealingDisarmed = 33554432,
		SpeedBoostDisarmed = 67108864,
		TeleportDisarmed = 134217728,
		NewStatusModifier = 268435456,
		OnDropper = 536870912
	}
}
