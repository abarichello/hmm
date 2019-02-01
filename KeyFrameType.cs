using System;

namespace HeavyMetalMachines
{
	public enum KeyFrameType : byte
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
		BombTrackTrigger
	}
}
