using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.GameCamera.Movement;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public class BombDisputeBehaviour : BaseCameraBehaviour
	{
		[Inject]
		private void Init(IBombManager bombManager, SkyViewMovement skyViewMovement, IGameCameraPlayerSetup cameraPlayer)
		{
			this._bombManager = bombManager;
			this._currentMovement = skyViewMovement;
			this._cameraPlayer = cameraPlayer;
			this._distanceToActivateSqr = (float)(this._bombManager.BombRules.MaxDistanceToActivateZoom * this._bombManager.BombRules.MaxDistanceToActivateZoom);
			this._bombManager.OnSlowMotionToggled += this.TriggerSlowMotion;
			base.SnapToTarget();
		}

		~BombDisputeBehaviour()
		{
			this._bombManager.OnSlowMotionToggled -= this.TriggerSlowMotion;
		}

		public override bool IsActive
		{
			get
			{
				return this._triggered && this._bombManager.SlowMotionEnabled;
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
				return this._bombManager.GetBombTransform();
			}
		}

		public override Vector3 CurrentTargetPosition
		{
			get
			{
				return this._bombManager.GetBombTransform().position;
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

		private void TriggerSlowMotion(bool enable)
		{
			if (!enable || this._cameraPlayer.PlayerCarId == -1)
			{
				this._triggered = false;
				return;
			}
			Transform bombTransform = this._bombManager.GetBombTransform();
			float sqrMagnitude = (bombTransform.position - this._cameraPlayer.PlayerTransform.position).sqrMagnitude;
			this._triggered = (sqrMagnitude < this._distanceToActivateSqr);
		}

		private IBombManager _bombManager;

		private SkyViewMovement _currentMovement;

		private IGameCameraPlayerSetup _cameraPlayer;

		private float _distanceToActivateSqr;

		private bool _triggered;
	}
}
