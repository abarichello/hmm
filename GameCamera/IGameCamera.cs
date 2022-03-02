using System;
using HeavyMetalMachines.Scene;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	public interface IGameCamera
	{
		CarCameraMode CameraMode { get; }

		[Obsolete]
		bool SetTarget(string context, ICameraTarget target);

		void TriggerTeleport();

		float CurrentFov { get; set; }

		void SetJitter(Vector3 jitter);

		void Shake(float amount);

		void SetCameraZoom(float zoom);

		SkyViewFollowMode SkyViewFollowMouse { get; set; }

		void SnapToTarget();

		void SetPanLock(bool locked);

		void SetFocusTarget(CameraFocusTrigger focus);
	}
}
