using System;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	[Obsolete]
	public interface ICameraTarget
	{
		CarCameraMode Mode { get; }

		Transform TargetTransform { get; }

		bool Snap { get; }

		bool Follow { get; }

		bool SmoothTeleport { get; }

		Func<bool> Condition { get; }
	}
}
