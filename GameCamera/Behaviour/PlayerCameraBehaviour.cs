using System;
using HeavyMetalMachines.GameCamera.Movement;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public class PlayerCameraBehaviour : BaseCameraBehaviour
	{
		[Inject]
		private void Init(SkyViewMovement skyViewMovement, IGameCameraPlayerSetup cameraPlayer)
		{
			this._currentMovement = skyViewMovement;
			this._cameraPlayer = cameraPlayer;
			base.SnapToTarget();
		}

		public override bool IsActive
		{
			get
			{
				return true;
			}
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
				return this._cameraPlayer.PlayerTransform;
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

		private ICameraMovement _currentMovement;

		private IGameCameraPlayerSetup _cameraPlayer;
	}
}
