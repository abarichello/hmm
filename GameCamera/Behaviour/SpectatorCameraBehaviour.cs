using System;
using HeavyMetalMachines.GameCamera.Movement;
using HeavyMetalMachines.Spectator;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public class SpectatorCameraBehaviour : BaseCameraBehaviour, ISpectatorCameraBehaviour
	{
		[Inject]
		private void Init(SpectatorCameraBehaviourParameters parameters, SkyViewMovement skyViewMovement, FixedCameraMovement fixedCameraMovement, OrbitalMovement orbitalMovement, ISpectatorService spectator)
		{
			this._parameters = parameters;
			this._skyViewMovement = skyViewMovement;
			this._fixedCamera = fixedCameraMovement;
			this._orbitalMovement = orbitalMovement;
			this._spectator = spectator;
			this.ChangeMode(CarCameraMode.SkyView);
			base.SnapToTarget();
		}

		public override bool IsActive
		{
			get
			{
				return this._spectator.IsSpectating;
			}
		}

		public override void OnCleanup()
		{
			this._currentTargetTransform = null;
			this._orbitalMovement.Reset();
			this._skyViewMovement.Reset();
			this._fixedCamera.Reset();
			this.ChangeMode(CarCameraMode.SkyView);
		}

		public CarCameraMode CameraMode { get; private set; }

		private void ChangeMode(CarCameraMode mode)
		{
			this.CameraMode = mode;
			if (mode != CarCameraMode.SkyView)
			{
				if (mode != CarCameraMode.Orbital)
				{
					if (mode == CarCameraMode.StageCamera)
					{
						this._currentMovement = this._fixedCamera;
					}
				}
				else
				{
					this._currentMovement = this._orbitalMovement;
				}
			}
			else
			{
				this._currentMovement = this._skyViewMovement;
			}
		}

		public bool IsOrbitalMode
		{
			get
			{
				return this._currentMovement == this._orbitalMovement;
			}
		}

		public bool IsSkyViewMode
		{
			get
			{
				return this._currentMovement == this._skyViewMovement;
			}
		}

		public bool IsFixedCamMode
		{
			get
			{
				return this._currentMovement == this._fixedCamera;
			}
		}

		public void ToggleOrbitalOrSkyViewMode()
		{
			if (this.IsFixedCamMode)
			{
				return;
			}
			if (this.IsSkyViewMode)
			{
				this.ChangeMode(CarCameraMode.Orbital);
				base.InterpolateToTarget();
				return;
			}
			this.ChangeMode(CarCameraMode.SkyView);
			base.InterpolateToTarget();
		}

		public void SetTarget(Transform target, bool snap)
		{
			this._currentTargetTransform = target;
			if (snap || this.IsFixedCamMode)
			{
				if (this.IsFixedCamMode)
				{
					this.ChangeMode(CarCameraMode.SkyView);
				}
				base.SnapToTarget();
			}
			else
			{
				base.InterpolateToTarget();
			}
		}

		public void SetFixedCamera(Transform target)
		{
			if (!this.IsFixedCamMode)
			{
				this.ChangeMode(CarCameraMode.StageCamera);
			}
			this._currentTargetTransform = target;
			base.SnapToTarget();
		}

		public override ICameraMovement CurrentMovement
		{
			get
			{
				return this._currentMovement;
			}
		}

		public override Transform CurrentTargetTransform
		{
			get
			{
				return this._currentTargetTransform;
			}
		}

		public override Vector3 CurrentTargetPosition
		{
			get
			{
				return this.CurrentTargetTransform.position;
			}
		}

		public override bool LockPan
		{
			get
			{
				return false;
			}
		}

		public override bool FollowTarget
		{
			get
			{
				return true;
			}
		}

		public override bool SmoothTeleport
		{
			get
			{
				return false;
			}
		}

		private SpectatorCameraBehaviourParameters _parameters;

		private SkyViewMovement _skyViewMovement;

		private FixedCameraMovement _fixedCamera;

		private OrbitalMovement _orbitalMovement;

		private ISpectatorService _spectator;

		private ICameraMovement _currentMovement;

		private Transform _currentTargetTransform;
	}
}
