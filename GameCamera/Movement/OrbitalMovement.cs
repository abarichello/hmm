using System;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera.Movement
{
	public class OrbitalMovement : ICameraMovement
	{
		public OrbitalMovement(OrbitalParameters parameters)
		{
			this._parameters = parameters;
			this.Reset();
		}

		public void Reset()
		{
			this._currentDistance = (this._parameters.MaxDistance + this._parameters.MinDistance) / 2f;
			this._currentAngle = (this._parameters.MaxAngle + this._parameters.MinAngle) / 2f;
			this._currentSideAngle = 0f;
			this._spinFreeFromTarget = this._parameters.UnlockSpin;
			this._fixSideAngle = false;
		}

		public void UpdateRotationSpeed(float distanceSpeed, float angleSpeed, float sideAngleSpeed)
		{
			this._currentDistanceSpeed = distanceSpeed;
			this._currentAngleSpeed = angleSpeed;
			this._currentSideAngleSpeed = sideAngleSpeed;
		}

		public void ToggleSpinLock()
		{
			this._spinFreeFromTarget = !this._spinFreeFromTarget;
			this._fixSideAngle = true;
		}

		public CameraState Update(IGameCameraState camera, CameraState current, float deltaTime)
		{
			this.UpdateAngles(deltaTime);
			CameraState result = new CameraState
			{
				NearPlane = this._parameters.NearPlane,
				FarPlane = this._parameters.FarPlane,
				Fov = this._parameters.Fov,
				StartFog = this._parameters.FogStart,
				EndFog = this._parameters.FogEnd
			};
			Transform currentTargetTransform = camera.CurrentTargetTransform;
			this.CheckAndFixSideAngle(currentTargetTransform);
			Vector3 vector = (!this._spinFreeFromTarget) ? currentTargetTransform.forward : Vector3.forward;
			Vector3 vector2 = Quaternion.Euler(0f, this._currentSideAngle, 0f) * vector;
			float num = this._currentAngle * 0.017453292f;
			Vector3 vector3 = vector2 * -1f * this._currentDistance * Mathf.Cos(num) + Vector3.up * this._currentDistance * Mathf.Sin(num);
			result.Position = vector3 + currentTargetTransform.position;
			result.Rotation = Quaternion.LookRotation(-vector3, Vector3.up);
			return result;
		}

		private void UpdateAngles(float deltaTime)
		{
			this._currentDistance += this._currentDistanceSpeed * deltaTime;
			this._currentAngle += this._currentAngleSpeed * deltaTime;
			this._currentSideAngle += this._currentSideAngleSpeed * deltaTime;
			this._currentDistance = Mathf.Clamp(this._currentDistance, this._parameters.MinDistance, this._parameters.MaxDistance);
			this._currentAngle = Mathf.Clamp(this._currentAngle, this._parameters.MinAngle, this._parameters.MaxAngle);
			if (this._currentSideAngle > 180f)
			{
				this._currentSideAngle -= 360f;
			}
			else if (this._currentSideAngle < -180f)
			{
				this._currentSideAngle += 360f;
			}
		}

		private void CheckAndFixSideAngle(Transform target)
		{
			if (this._fixSideAngle)
			{
				this._fixSideAngle = false;
				float num = Vector3.Angle(Vector3.forward, target.forward);
				if (Vector3.Cross(Vector3.forward, target.forward).y < 0f)
				{
					num = -num;
				}
				if (this._spinFreeFromTarget)
				{
					this._currentSideAngle += num;
				}
				else
				{
					this._currentSideAngle -= num;
				}
				if (this._currentSideAngle > 180f)
				{
					this._currentSideAngle -= 360f;
				}
				else if (this._currentSideAngle < -180f)
				{
					this._currentSideAngle += 360f;
				}
			}
		}

		private readonly OrbitalParameters _parameters;

		private float _currentSideAngle;

		private float _currentAngle;

		private float _currentDistance;

		private float _currentSideAngleSpeed;

		private float _currentAngleSpeed;

		private float _currentDistanceSpeed;

		private bool _spinFreeFromTarget;

		private bool _fixSideAngle;
	}
}
