using System;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera.Movement
{
	public interface IGameCameraState
	{
		Transform CurrentTargetTransform { get; }

		Vector3 CurrentTargetPosition { get; }

		bool LockPan { get; }

		bool FollowTarget { get; }

		bool SmoothTeleport { get; }
	}
}
