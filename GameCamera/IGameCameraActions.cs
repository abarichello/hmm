using System;
using HeavyMetalMachines.Scene;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	public interface IGameCameraActions
	{
		void OverrideDynamicZoom(float zoom);

		void OverrideFoV(float value);

		void RestoreFoV();

		void TriggerTeleport();

		void SetJitter(Vector3 jitter);

		void Shake(float amount);

		void ForceSnapToTarget();

		void LockPan();

		void UnlockPan();

		void SetFocusTarget(CameraFocusTrigger focus);

		void RemoveFocusTarget();
	}
}
