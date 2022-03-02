using System;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public interface ISpectatorCameraBehaviour
	{
		Transform CurrentTargetTransform { get; }

		bool IsOrbitalMode { get; }

		bool IsSkyViewMode { get; }

		bool IsFixedCamMode { get; }

		void ToggleOrbitalOrSkyViewMode();

		void SetTarget(Transform target, bool snap);

		void SetFixedCamera(Transform target);
	}
}
