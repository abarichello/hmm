using System;
using HeavyMetalMachines.GameCamera.Movement;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera.Behaviour
{
	public abstract class BaseCameraBehaviour : IGameCameraBehaviour, IGameCameraState
	{
		public abstract bool IsActive { get; }

		public abstract ICameraMovement CurrentMovement { get; }

		public virtual void OnEnable()
		{
		}

		public virtual void OnDisable()
		{
		}

		public virtual void BeforeUpdate(ref CameraState state)
		{
		}

		public virtual void AfterUpdate(ref CameraState state)
		{
		}

		public virtual void OnCleanup()
		{
		}

		public void Enable(CameraState currentState)
		{
			this._initialState = currentState;
			this._interpolationValue = 0f;
			this.OnEnable();
		}

		public void Disable()
		{
			this.OnDisable();
		}

		public CameraState Update(IGameCameraEngine engine, CameraState state)
		{
			this.BeforeUpdate(ref state);
			if (this.CurrentTargetTransform != null)
			{
				this._currentState = this.CurrentMovement.Update(this, state, this.DeltaTime());
				if (this._interpolationValue < 1f)
				{
					this._interpolationValue = Mathf.SmoothDamp(this._interpolationValue, 1f, ref this._interpolationSpeed, 0.5f, float.PositiveInfinity, this.DeltaTime());
					this._interpolationValue = Mathf.Clamp01(this._interpolationValue);
					this._currentState = CameraState.Lerp(this._initialState, this._currentState, this._interpolationValue);
					if (Mathf.Approximately(this._interpolationValue, 1f))
					{
						this._interpolationValue = 2f;
					}
				}
				return this._currentState;
			}
			this.AfterUpdate(ref state);
			return this._currentState = state;
		}

		private float DeltaTime()
		{
			return Time.unscaledDeltaTime;
		}

		public void Cleanup()
		{
			this.CurrentMovement.Reset();
			this.SnapToTarget();
			this.OnCleanup();
		}

		protected void InterpolateToTarget()
		{
			this._interpolationValue = 0f;
			this._initialState = this._currentState;
		}

		protected void SnapToTarget()
		{
			this._interpolationValue = 2f;
		}

		public abstract Transform CurrentTargetTransform { get; }

		public abstract Vector3 CurrentTargetPosition { get; }

		public abstract bool LockPan { get; }

		public abstract bool FollowTarget { get; }

		public abstract bool SmoothTeleport { get; }

		private float _interpolationValue;

		private float _interpolationSpeed;

		private CameraState _initialState;

		private CameraState _currentState;
	}
}
