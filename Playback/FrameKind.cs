using System;

namespace HeavyMetalMachines.Playback
{
	public enum FrameKind : byte
	{
		None,
		CombatStates,
		TransformStates,
		ModifierEvent,
		CollisionEvent,
		ManagerEvent,
		GadgetLevel,
		BombInstance,
		BombDetonation,
		CombatFeedbacks,
		GadgetEvent,
		PlayerStats,
		Scoreboard,
		Players,
		Teams,
		Counselor
	}
}
