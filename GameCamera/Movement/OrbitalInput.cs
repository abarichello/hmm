using System;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Spectator;
using Hoplon.Input.Business;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera.Movement
{
	public class OrbitalInput : IOrbitalInput
	{
		public OrbitalInput(OrbitalParameters parameters, OrbitalMovement movement, IControllerInputActionPoller controllerInputActionPoller, ISpectatorCameraConfigProvider cameraConfigProvider)
		{
			this._parameters = parameters;
			this._movement = movement;
			this._controllerInputActionPoller = controllerInputActionPoller;
			this._cameraConfigProvider = cameraConfigProvider;
			this.Reset();
		}

		public void Reset()
		{
			this._distSpeed = 0f;
			this._angleSpeed = 0f;
			this._sideAngleSpeed = 0f;
		}

		public void LateUpdate()
		{
			this.UpdateSpinLock();
			this.UpdateCurrentDistance();
			this.UpdateCurrentAngle();
			this.UpdateCurrentSideAngle();
			this._movement.UpdateRotationSpeed(this._distSpeed, this._angleSpeed, this._sideAngleSpeed);
		}

		private void UpdateSpinLock()
		{
			if (Input.GetKeyDown(this._parameters.ChangeLock.GetUnityKeyCode()))
			{
				this._movement.ToggleSpinLock();
			}
		}

		private void UpdateCurrentDistance()
		{
			bool up = Input.GetKey(this._parameters.ZoomOut.GetUnityKeyCode()) || this._controllerInputActionPoller.GetButton(this._cameraConfigProvider.ZoomOutGamepadAction);
			bool down = Input.GetKey(this._parameters.ZoomIn.GetUnityKeyCode()) || this._controllerInputActionPoller.GetButton(this._cameraConfigProvider.ZoomInGamepadAction);
			OrbitalInput.UpdateValue(up, down, this._parameters.ZoomAcceleration, this._parameters.ZoomSpeed, Time.unscaledDeltaTime, ref this._distSpeed);
		}

		private void UpdateCurrentAngle()
		{
			bool up = Input.GetKey(this._parameters.TurnUp.GetUnityKeyCode()) || this._controllerInputActionPoller.GetAxis(this._cameraConfigProvider.MoveOrbitalCameraVerticallGamepadAxisAction) > 0.4f;
			bool down = Input.GetKey(this._parameters.TurnDown.GetUnityKeyCode()) || this._controllerInputActionPoller.GetAxis(this._cameraConfigProvider.MoveOrbitalCameraVerticallGamepadAxisAction) < -0.4f;
			OrbitalInput.UpdateValue(up, down, this._parameters.TurnAcceleration, this._parameters.TurnSpeed, Time.unscaledDeltaTime, ref this._angleSpeed);
		}

		private void UpdateCurrentSideAngle()
		{
			bool up = Input.GetKey(this._parameters.TurnLeft.GetUnityKeyCode()) || this._controllerInputActionPoller.GetAxis(this._cameraConfigProvider.MoveOrbitalCameraHorizontalGamepadAxisAction) < -0.4f;
			bool down = Input.GetKey(this._parameters.TurnRight.GetUnityKeyCode()) || this._controllerInputActionPoller.GetAxis(this._cameraConfigProvider.MoveOrbitalCameraHorizontalGamepadAxisAction) > 0.4f;
			OrbitalInput.UpdateValue(up, down, this._parameters.TurnAcceleration, this._parameters.TurnSpeed, Time.unscaledDeltaTime, ref this._sideAngleSpeed);
		}

		private static void UpdateValue(bool up, bool down, float acceleration, float maxSpeed, float deltaTime, ref float currentSpeed)
		{
			if (up && !down)
			{
				currentSpeed += acceleration * deltaTime;
				if (currentSpeed > maxSpeed)
				{
					currentSpeed = maxSpeed;
				}
			}
			else if (down && !up)
			{
				currentSpeed -= acceleration * deltaTime;
				if (currentSpeed < -maxSpeed)
				{
					currentSpeed = -maxSpeed;
				}
			}
			else
			{
				currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, acceleration * deltaTime);
			}
		}

		private const float AxisThreshold = 0.4f;

		private readonly OrbitalMovement _movement;

		private readonly OrbitalParameters _parameters;

		private readonly IControllerInputActionPoller _controllerInputActionPoller;

		private ISpectatorCameraConfigProvider _cameraConfigProvider;

		private float _distSpeed;

		private float _sideAngleSpeed;

		private float _angleSpeed;
	}
}
