using System;

namespace HeavyMetalMachines.Playback
{
	public static class FrameKindLegacyEx
	{
		public static FrameKind Convert(this KeyFrameType type)
		{
			switch (type)
			{
			case KeyFrameType.CombatStates:
				return FrameKind.CombatStates;
			case KeyFrameType.TransformStates:
				return FrameKind.TransformStates;
			case KeyFrameType.ModifierEvent:
				return FrameKind.ModifierEvent;
			case KeyFrameType.CollisionEvent:
				return FrameKind.CollisionEvent;
			case KeyFrameType.ManagerEvent:
				return FrameKind.ManagerEvent;
			case KeyFrameType.GadgetLevel:
				return FrameKind.GadgetLevel;
			case KeyFrameType.BombInstance:
				return FrameKind.BombInstance;
			case KeyFrameType.BombDetonation:
				return FrameKind.BombDetonation;
			case KeyFrameType.CombatFeedbacks:
				return FrameKind.CombatFeedbacks;
			case KeyFrameType.GadgetEvent:
				return FrameKind.GadgetEvent;
			default:
				return FrameKind.None;
			}
		}

		public static FrameKind Convert(this StateType type)
		{
			switch (type)
			{
			case StateType.PlayerStats:
				return FrameKind.PlayerStats;
			case StateType.Players:
				return FrameKind.Players;
			case StateType.Scoreboard:
				return FrameKind.Scoreboard;
			case StateType.Counselor:
				return FrameKind.Counselor;
			case StateType.Teams:
				return FrameKind.Teams;
			}
			return FrameKind.None;
		}

		public static KeyFrameType ToKeyFrameType(this FrameKind kind)
		{
			switch (kind)
			{
			case FrameKind.CombatStates:
				return KeyFrameType.CombatStates;
			case FrameKind.TransformStates:
				return KeyFrameType.TransformStates;
			case FrameKind.ModifierEvent:
				return KeyFrameType.ModifierEvent;
			case FrameKind.CollisionEvent:
				return KeyFrameType.CollisionEvent;
			case FrameKind.ManagerEvent:
				return KeyFrameType.ManagerEvent;
			case FrameKind.GadgetLevel:
				return KeyFrameType.GadgetLevel;
			case FrameKind.BombInstance:
				return KeyFrameType.BombInstance;
			case FrameKind.BombDetonation:
				return KeyFrameType.BombDetonation;
			case FrameKind.CombatFeedbacks:
				return KeyFrameType.CombatFeedbacks;
			case FrameKind.GadgetEvent:
				return KeyFrameType.GadgetEvent;
			default:
				return KeyFrameType.None;
			}
		}

		public static StateType ToStateType(this FrameKind kind)
		{
			switch (kind)
			{
			case FrameKind.PlayerStats:
				return StateType.PlayerStats;
			case FrameKind.Scoreboard:
				return StateType.Scoreboard;
			case FrameKind.Players:
				return StateType.Players;
			case FrameKind.Teams:
				return StateType.Teams;
			case FrameKind.Counselor:
				return StateType.Counselor;
			default:
				return StateType.None;
			}
		}

		public static bool IsKeyFrameType(this FrameKind kind)
		{
			switch (kind)
			{
			case FrameKind.PlayerStats:
			case FrameKind.Scoreboard:
			case FrameKind.Players:
			case FrameKind.Teams:
			case FrameKind.Counselor:
				return false;
			default:
				return true;
			}
		}

		public static bool IsStateType(this FrameKind kind)
		{
			return !kind.IsKeyFrameType();
		}
	}
}
