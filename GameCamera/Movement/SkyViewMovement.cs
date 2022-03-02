using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Scene;
using HeavyMetalMachines.Spectator;
using Hoplon.Input.Business;
using Hoplon.Math;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera.Movement
{
	public class SkyViewMovement : ICameraMovement
	{
		public SkyViewMovement(SkyViewParameters parameters)
		{
			this._parameters = parameters;
			this.Reset();
		}

		public SkyViewParameters Parameters
		{
			get
			{
				return this._parameters;
			}
		}

		public CameraFocusTrigger SkyViewFocusTarget
		{
			get
			{
				return this._skyViewFocusTarget;
			}
			set
			{
				if (value)
				{
					this._skyViewFocusTarget = value;
				}
				else
				{
					this._skyViewLoosingFocus = true;
				}
			}
		}

		private float SkyViewTargetZoom
		{
			get
			{
				if (this._spectatorService.IsSpectating)
				{
					CameraZoomLevel currentZoomLevel = this._spectatorService.GetCurrentZoomLevel();
					switch (currentZoomLevel + 1)
					{
					case CameraZoomLevel.Game:
						return this._parameters.SpectatorFarZoom;
					case CameraZoomLevel.Near:
						return this._parameters.DefaultZoom;
					case CameraZoomLevel.Close:
						return this._parameters.SpectatorNearZoom;
					case (CameraZoomLevel)3:
						return this._parameters.SpectatorCloseZoom;
					}
				}
				if (this.PlayerPreSpawing)
				{
					return this._parameters.PreSpawnZoom;
				}
				return this._parameters.DefaultZoom;
			}
		}

		public void SetJitter(Vector3 jitter)
		{
			this._offsetJitter = jitter;
		}

		public void Reset()
		{
			this._joyTargetOffset = Vector2.zero;
			this._currentCameraPositionOffset = Vector3.zero;
			this._currentLookPosition = Vector3.zero;
			this._initialCameraTargetPosition = Vector3.zero;
			this._offsetJitter = Vector3.zero;
			this._skyViewCurrentZoom = this._parameters.DefaultZoom;
			this._smoothDampVelocity = 0f;
			this._movementPct = 0f;
			this._smoothing = false;
		}

		public CameraState Update(IGameCameraState camera, CameraState current, float deltaTime)
		{
			CameraState result = new CameraState
			{
				NearPlane = this._parameters.NearPlane,
				FarPlane = this._parameters.FarPlane,
				Fov = this._parameters.Fov,
				StartFog = this._parameters.FogStart,
				EndFog = this._parameters.FogEnd,
				Position = current.Position,
				Rotation = current.Rotation
			};
			Vector3 currentTargetPosition = camera.CurrentTargetPosition;
			float cameraDistance = this.GetCameraDistance(currentTargetPosition);
			Vector3 lastCameraTargetPosition = this.GetLastCameraTargetPosition(camera, deltaTime, currentTargetPosition);
			Vector3 vector = lastCameraTargetPosition + this._offsetJitter;
			Vector3 currentOffset = this.GetCurrentOffset(camera);
			this.UpdateZoomAndOffset(deltaTime, currentOffset, vector);
			Quaternion quaternion = Quaternion.Euler(this._gameCameraInversion.InversionAngle);
			Vector3 vector2 = ((!camera.FollowTarget) ? camera.CurrentTargetPosition : lastCameraTargetPosition) + quaternion * (Vector3.up * cameraDistance * this._skyViewCurrentZoom);
			result.Position = vector2 + this._currentCameraPositionOffset;
			result.Rotation.SetLookRotation((vector - vector2).normalized, Vector3.up);
			if (!this._spectatorService.IsSpectating)
			{
				this._currentDynamicZoom = Mathf.Lerp(this._currentDynamicZoom, this.CameraDynamicZoom, deltaTime * 2f);
				result.Fov *= 1f - this._currentDynamicZoom;
			}
			return result;
		}

		private void UpdateZoomAndOffset(float deltaTime, Vector3 currentOffset, Vector3 targetPosition)
		{
			float num = deltaTime * this._inputCameraPanLoad.GetCurrentInfraValue(CameraPanCode.Sensitivity);
			if (this.PlayerPreSpawing)
			{
				this._currentLookPosition = Vector3.Lerp(this._currentLookPosition, currentOffset + targetPosition, num);
				this._currentCameraPositionOffset = this._currentLookPosition - targetPosition;
			}
			else
			{
				this._currentCameraPositionOffset = Vector3.Lerp(this._currentCameraPositionOffset, currentOffset, num);
				this._currentLookPosition = this._currentCameraPositionOffset + targetPosition;
			}
			this._currentCameraPositionOffset.y = 0f;
			this._skyViewCurrentZoom = Mathf.Lerp(this._skyViewCurrentZoom, this.SkyViewTargetZoom, num);
		}

		private Vector3 GetCurrentOffset(IGameCameraState camera)
		{
			Quaternion quaternion = Quaternion.Euler(0f, this._gameCameraInversion.InversionAngle.y - 90f, 0f);
			Vector3 vector = quaternion * Vector3.back;
			Vector3 vector2 = quaternion * Vector3.left;
			Vector2 vector3 = Vector2.zero;
			bool flag = this._spectatorService.IsSpectating && (Input.GetKey(324) || !Application.isFocused);
			float currentInfraValue = this._inputCameraPanLoad.GetCurrentInfraValue(CameraPanCode.MaxDistance);
			if (!camera.LockPan && !flag)
			{
				SkyViewFollowMode skyViewFollowMouse = this.SkyViewFollowMouse;
				if (skyViewFollowMouse != SkyViewFollowMode.Mouse)
				{
					if (skyViewFollowMouse != SkyViewFollowMode.JoyAxis)
					{
						if (skyViewFollowMouse != SkyViewFollowMode.None)
						{
						}
					}
					else
					{
						Vector2 compositeAxis = this._inputActionPoller.GetCompositeAxis(22, 21);
						bool flag2 = compositeAxis.sqrMagnitude > 0.5f;
						if (this._inputJoystickCursorLoad.LoadCursorMode() == JoystickCursorMode.ReturnToCenter || flag2)
						{
							vector3.x = Mathf.Pow(compositeAxis.x, 3f);
							vector3.y = Mathf.Pow(compositeAxis.y, 3f);
							vector3 = Vector3.ClampMagnitude(vector3, 1f);
							vector3 *= currentInfraValue;
							this._joyTargetOffset = vector3;
						}
						vector3 = this._joyTargetOffset;
					}
				}
				else
				{
					vector3 = Input.mousePosition;
					float num = (float)Screen.height * 0.5f * this._parameters.ScreenArenaPercent;
					vector3.x -= (float)Screen.width * 0.5f;
					vector3.y -= (float)Screen.height * 0.5f;
					float num2 = vector3.magnitude;
					if (Mathf.Approximately(num2, 0f))
					{
						Debug.LogError("woot " + num2);
						return Vector2.zero;
					}
					Vector2 vector4 = vector3 / num2;
					float currentInfraValue2 = this._inputCameraPanLoad.GetCurrentInfraValue(CameraPanCode.Deadzone);
					float num3 = num * currentInfraValue2;
					if (num2 < num3)
					{
						num2 = num3;
					}
					if (num2 > num)
					{
						num2 = num;
					}
					num2 = (num2 - num3) / (num - num3);
					vector3 = vector4 * num2;
					if (vector3.y < 0f)
					{
						vector3.y *= this._parameters.CameraAngleCompensationPanMultiplier;
					}
					vector3 *= currentInfraValue;
				}
			}
			return vector2 * vector3.x + vector * vector3.y;
		}

		private Vector3 GetLastCameraTargetPosition(IGameCameraState camera, float deltaTime, Vector3 targetPosition)
		{
			if (this.Teleported && camera.SmoothTeleport && !this._smoothing)
			{
				this._initialCameraTargetPosition = targetPosition;
				this._smoothing = true;
				this.Teleported = false;
				this._movementPct = 0f;
				this._smoothDampVelocity = camera.CurrentTargetTransform.GetComponent<CombatObject>().Movement.SpeedZ * this._parameters.InitialVelocityPct;
			}
			Vector3 result = targetPosition;
			if (this._smoothing)
			{
				this._movementPct = Mathf.SmoothDamp(this._movementPct, 1f, ref this._smoothDampVelocity, this._parameters.SmoothDampDuration, float.PositiveInfinity, deltaTime);
				this._movementPct = Mathf.Clamp01(this._movementPct);
				result = Vector3.Lerp(this._initialCameraTargetPosition, targetPosition, this._movementPct);
				if (Mathf.Approximately(this._movementPct, 1f))
				{
					this._smoothing = false;
				}
			}
			return result;
		}

		private float GetCameraDistance(Vector3 targetPosition)
		{
			float num = this._parameters.CameraDistance;
			if (this.SkyViewFocusTarget != null && !this._skyViewLoosingFocus)
			{
				float num2 = this._parameters.FocusCurve.Evaluate(1f - (this.SkyViewFocusTarget.position - targetPosition).magnitude / this.SkyViewFocusTarget.size);
				if (num2 > 1f)
				{
					num2 = 1f;
				}
				else if (num2 < 0f)
				{
					num2 = 0f;
				}
				num *= 1f + num2 * (this.SkyViewFocusTarget.extraViewAreaMultiplier - 1f);
			}
			else
			{
				this.SkyViewFocusTarget = null;
				this._skyViewLoosingFocus = false;
			}
			return num;
		}

		public bool PlayerPreSpawing { get; set; }

		[Inject]
		private IControllerInputActionPoller _inputActionPoller;

		[Inject]
		private IInputJoystickCursorLoad _inputJoystickCursorLoad;

		[Inject]
		private IInputCameraPanLoad _inputCameraPanLoad;

		[Inject]
		private ISpectatorService _spectatorService;

		[Inject]
		private IGameCameraInversion _gameCameraInversion;

		private readonly SkyViewParameters _parameters;

		public float CameraDynamicZoom;

		private float _currentDynamicZoom;

		public SkyViewFollowMode SkyViewFollowMouse = SkyViewFollowMode.Mouse;

		public bool Teleported;

		private bool _skyViewLoosingFocus;

		private CameraFocusTrigger _skyViewFocusTarget;

		private Vector3 _offsetJitter = Vector3.zero;

		private Vector2 _joyTargetOffset;

		private Vector3 _currentCameraPositionOffset;

		private Vector3 _currentLookPosition;

		private Vector3 _initialCameraTargetPosition;

		private bool _smoothing;

		private float _movementPct;

		private float _smoothDampVelocity;

		private float _skyViewCurrentZoom;
	}
}
