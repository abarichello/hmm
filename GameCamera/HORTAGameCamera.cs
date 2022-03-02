using System;
using HeavyMetalMachines.GameCamera.Movement;
using HeavyMetalMachines.Scene;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera
{
	public class HORTAGameCamera : IGameCamera
	{
		public bool SetTarget(string context, ICameraTarget target)
		{
			return false;
		}

		public void TriggerTeleport()
		{
			this._skyViewMovement.Teleported = true;
		}

		public float CurrentFov
		{
			get
			{
				return this._skyViewMovement.Parameters.Fov;
			}
			set
			{
				this._skyViewMovement.Parameters.Fov = value;
			}
		}

		public void SetJitter(Vector3 jitter)
		{
			this._skyViewMovement.SetJitter(jitter);
		}

		public void Shake(float amount)
		{
			this._camera.Shake(amount);
		}

		public void SetCameraZoom(float zoom)
		{
			this._skyViewMovement.CameraDynamicZoom = zoom;
		}

		public SkyViewFollowMode SkyViewFollowMouse
		{
			get
			{
				return this._skyViewMovement.SkyViewFollowMouse;
			}
			set
			{
				this._skyViewMovement.SkyViewFollowMouse = value;
			}
		}

		public void SnapToTarget()
		{
			this._camera.ForceSnappingToTarget();
		}

		public void SetPanLock(bool locked)
		{
			this._camera.LockPan = locked;
		}

		public void SetFocusTarget(CameraFocusTrigger focus)
		{
			this._skyViewMovement.SkyViewFocusTarget = focus;
		}

		public CarCameraMode CameraMode
		{
			get
			{
				return this._camera.CameraMode;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HORTAGameCamera));

		[Inject]
		private HORTACamera _camera;

		[Inject]
		private SkyViewMovement _skyViewMovement;
	}
}
