using System;
using HeavyMetalMachines.GameCamera.Movement;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public class BombScoreCameraBehaviour : BaseCameraBehaviour, IBombScoreCameraBehaviour
	{
		[Inject]
		private void Init(SkyViewMovement skyViewMovement)
		{
			this._skyViewMovement = skyViewMovement;
			this._behaviourRunning = false;
			base.SnapToTarget();
		}

		public override bool IsActive
		{
			get
			{
				return this._behaviourRunning;
			}
		}

		public override void OnCleanup()
		{
			this._behaviourRunning = false;
			this._currentTargetTransform = null;
		}

		public override ICameraMovement CurrentMovement
		{
			get
			{
				return this._skyViewMovement;
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
				return (!this.FollowTarget) ? this._targetPosition : this.CurrentTargetTransform.position;
			}
		}

		public override bool LockPan
		{
			get
			{
				return true;
			}
		}

		public override bool FollowTarget
		{
			get
			{
				return this._followTarget;
			}
		}

		public override bool SmoothTeleport
		{
			get
			{
				return false;
			}
		}

		public void LookAtExplosion(Transform explosionTransform)
		{
			this._followTarget = false;
			this._targetPosition = explosionTransform.position;
			this._behaviourRunning = true;
			this._currentTargetTransform = explosionTransform;
			base.InterpolateToTarget();
		}

		public void FollowBomb(Transform bombTransform)
		{
			this._followTarget = true;
			this._behaviourRunning = true;
			this._currentTargetTransform = bombTransform;
			base.SnapToTarget();
		}

		public void StopBehaviour()
		{
			this._behaviourRunning = false;
			this._currentTargetTransform = null;
		}

		private SkyViewMovement _skyViewMovement;

		private bool _followTarget;

		private bool _behaviourRunning;

		private Transform _currentTargetTransform;

		private Vector3 _targetPosition;
	}
}
