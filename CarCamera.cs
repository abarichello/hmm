using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.PostProcessing;
using HeavyMetalMachines.Scene;
using HeavyMetalMachines.Spectator;
using Hoplon.Input.Business;
using Hoplon.Math;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines
{
	[RequireComponent(typeof(Camera))]
	public class CarCamera : GameHubBehaviour, ICleanupListener
	{
		public Transform CameraTargetTransform
		{
			get
			{
				return this._cameraTargetTransform;
			}
		}

		private bool IsPlayerPreSpawning()
		{
			return GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery && this.MyPlayerCombatData != null && this.MyPlayerCombatData.Combat != null && (this.MyPlayerCombatData.Combat.SpawnController.State == SpawnStateKind.PreSpawned || this.MyPlayerCombatData.Combat.SpawnController.State == SpawnStateKind.Respawning);
		}

		public Vector3 CurrentTargetPosition
		{
			get
			{
				return (!this._followTarget) ? this._targetInitialPosition : this.CameraTargetTransform.position;
			}
		}

		public Transform CameraTransform
		{
			get
			{
				return this._cameraTransform;
			}
		}

		public CarCameraMode CameraMode
		{
			get
			{
				return this._cameraMode;
			}
		}

		private void OnEnable()
		{
			this._interpolationValue = 1f;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.OnPlayerUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning += this.OnPlayerRespawning;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.OnPlayerUnspawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning -= this.OnPlayerRespawning;
		}

		private void Awake()
		{
			if (CarCamera.SingletonInstanceId == -1)
			{
				CarCamera.Singleton = this;
				CarCamera.SingletonInstanceId = base.GetInstanceID();
				this._cameraTransform = base.transform;
				this.CameraInversionAngleY = this.CameraInversionTeamAAngleY;
				if (this.CameraTargetTransform != null)
				{
					CarCamera.CameraTargetData target = new CarCamera.CameraTargetData
					{
						Target = this.CameraTargetTransform,
						Mode = CarCameraMode.SkyView,
						Snap = true,
						Follow = true,
						SmoothTeleport = true
					};
					this.SetTarget(target);
				}
				this.Camera = base.GetComponent<Camera>();
				this.CameraInstanceId = this.Camera.GetInstanceID();
				this._targetStack = new CarCameraStack<CarCamera.CameraTargetData>(16);
				this._raceStartCursorLockController = new PlayerRaceStartCursorLockController(this._config);
				this._skyViewCurrentZoom = this.SkyViewDefaultZoom;
				this.SkyViewSpectatorCloseZoom = this._config.GetFloatValue(ConfigAccess.SpectatorZoomClose);
				this.SkyViewSpectatorNearZoom = this._config.GetFloatValue(ConfigAccess.SpectatorZoomNear);
				this.SkyViewSpectatorFarZoom = this._config.GetFloatValue(ConfigAccess.SpectatorZoomFar);
			}
			else
			{
				CarCamera.Log.Error("Singleton is already initialized");
				base.enabled = false;
				Object.DestroyImmediate(this);
			}
		}

		private void OnDestroy()
		{
			if (CarCamera.SingletonInstanceId == base.GetInstanceID())
			{
				CarCamera.Singleton = null;
				CarCamera.SingletonInstanceId = -1;
			}
		}

		private void OnPhaseChange(BombScoreboardState state)
		{
			if (this._spectatorService.IsSpectating)
			{
				return;
			}
			if (state == BombScoreboardState.BombDelivery)
			{
				this.SetTarget("Player", () => GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery, this.MyPlayerTransform, CarCameraMode.SkyView, false, true, true);
			}
		}

		private void OnPlayerUnspawn(PlayerEvent data)
		{
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			if (currentPlayerData != null && currentPlayerData.GetPlayerCarObjectId() == data.TargetId)
			{
				this.SetTarget("Respawn", delegate()
				{
					BombManager bombManager = GameHubBehaviour.Hub.BombManager;
					return bombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery && !this.MyPlayerCombatData.IsAlive() && bombManager.BombMovement.gameObject.activeSelf;
				}, GameHubBehaviour.Hub.BombManager.BombMovement.Combat.transform, CarCameraMode.SkyView, false, true, false);
				int delayTime = data.EventTime + (int)(1000f * GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena().KillCamWaitTimeSeconds);
				this.SetTarget("RespawnDelay", delegate()
				{
					BombManager bombManager = GameHubBehaviour.Hub.BombManager;
					return bombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery && !this.MyPlayerCombatData.IsAlive() && bombManager.BombMovement.gameObject.activeSelf && GameHubBehaviour.Hub.GameTime.GetPlaybackTime() < delayTime;
				}, this.MyPlayerTransform, CarCameraMode.SkyView, false, false, false);
			}
		}

		private void OnPlayerRespawning(PlayerEvent data)
		{
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			if (currentPlayerData != null && currentPlayerData.GetPlayerCarObjectId() == data.TargetId)
			{
				this.SetTarget("Respawning", delegate()
				{
					BombManager bombManager = GameHubBehaviour.Hub.BombManager;
					return bombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery && !this.MyPlayerCombatData.IsAlive();
				}, this.MyPlayerTransform, CarCameraMode.SkyView, false, true, false);
			}
		}

		private bool SetMode(CarCameraMode mode)
		{
			bool result = this._cameraMode != mode;
			this._cameraMode = mode;
			return result;
		}

		public void OnCleanup(CleanupMessage msg)
		{
			CarCamera.Log.Debug("OnCleanup. Setting camera follow target to null.");
			this._cameraTargetTransform = null;
			this.LockPan = false;
			this._targetStack.Clear();
		}

		public bool SetTarget(string identifier, Func<bool> condition, Transform targetTransform, CarCameraMode mode = CarCameraMode.SkyView, bool snap = true, bool follow = true, bool smoothTeleport = false)
		{
			CarCamera.CameraTargetData cameraTargetData = new CarCamera.CameraTargetData(mode, targetTransform, snap, follow, smoothTeleport);
			if (this._targetStack.Push(identifier, condition, cameraTargetData))
			{
				this.SetTarget(cameraTargetData);
				return true;
			}
			return false;
		}

		private void SetTarget(CarCamera.CameraTargetData data)
		{
			bool flag = this.SetMode(data.Mode);
			this._cameraTargetTransform = data.Target;
			this._followTarget = data.Follow;
			this._interpolationValue = ((!data.Snap && !flag) ? 0f : 2f);
			this._smoothTeleport = data.SmoothTeleport;
			this._descriptorCurrent = this._descriptorTarget;
			this._targetInitialPosition = ((!(this._cameraTargetTransform != null)) ? Vector3.zero : this._cameraTargetTransform.position);
		}

		public void ForceSnappingToTarget()
		{
			this._interpolationValue = 1f;
		}

		private void Update()
		{
			if (!GameHubBehaviour.Hub)
			{
				return;
			}
			this._raceStartCursorLockController.FreeCursorLockIfTimeout();
			if (this.MyPlayerCombatData == null)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.AllowCameraMovement, false))
			{
				return;
			}
			float axis = Input.GetAxis("Mouse ScrollWheel");
			if (!Mathf.Approximately(axis, 0f) && this.CameraDistance > 50f)
			{
				this.CameraDistance -= axis * this.SmoothDeltaTime() * this.CameraScrollStrength;
				if (this.CameraDistance < 50f)
				{
					this.CameraDistance = 50f;
				}
			}
			if (Input.GetKey(105))
			{
				this.CameraInversionAngle = Mathf.Max(5f, this.CameraInversionAngle - this.SmoothDeltaTime() * 30f);
			}
			if (Input.GetKey(107))
			{
				this.CameraInversionAngle = Mathf.Min(85f, this.CameraInversionAngle + this.SmoothDeltaTime() * 30f);
			}
			if (Input.GetKey(108))
			{
				this.CameraInversionAngleY = Mathf.Max(0f, this.CameraInversionAngleY - this.SmoothDeltaTime() * 30f);
			}
			if (Input.GetKey(106))
			{
				this.CameraInversionAngleY = Mathf.Min(360f, this.CameraInversionAngleY + this.SmoothDeltaTime() * 30f);
			}
			if (Input.GetKeyDown(99))
			{
				this.CameraInversionIsInverted = !this.CameraInversionIsInverted;
			}
		}

		private void LateUpdate()
		{
			if (GameHubBehaviour.Hub.State.Current is MainMenu)
			{
				return;
			}
			if (this._targetStack.ShouldUpdate())
			{
				CarCamera.CameraTargetData target = this._targetStack.Peek();
				this.SetTarget(target);
			}
			if (this.CameraTargetTransform != null)
			{
				CarCameraMode cameraMode = this._cameraMode;
				if (cameraMode != CarCameraMode.SkyView)
				{
					if (cameraMode == CarCameraMode.StageCamera)
					{
						this.CameraModeLateUpdate();
					}
				}
				else
				{
					this.SkyViewLateUpdate();
				}
				if (this._interpolationValue < 1f)
				{
					this._interpolationValue = Mathf.SmoothDamp(this._interpolationValue, 1f, ref this._interpolationSpeed, 0.5f, float.PositiveInfinity, this.DeltaTime());
					this._interpolationValue = Mathf.Clamp01(this._interpolationValue);
					this._descriptorTarget = CarCamera.CameraDescriptor.Lerp(this._descriptorCurrent, this._descriptorTarget, this._interpolationValue);
					if (Mathf.Approximately(this._interpolationValue, 1f))
					{
						this._interpolationValue = 2f;
					}
				}
				this.Camera.nearClipPlane = this._descriptorTarget.nearPlane;
				this.Camera.farClipPlane = this._descriptorTarget.farPlane;
				if (!this._spectatorService.IsSpectating)
				{
					this._currentDynamicZoom = Mathf.Lerp(this._currentDynamicZoom, this.CameraDynamicZoom, Time.unscaledDeltaTime * 2f);
					this.Camera.fieldOfView = this._descriptorTarget.fov * (1f - this._currentDynamicZoom);
				}
				else
				{
					this.Camera.fieldOfView = this._descriptorTarget.fov;
				}
				RenderSettings.fogStartDistance = this._descriptorTarget.startFog;
				RenderSettings.fogEndDistance = this._descriptorTarget.endFog;
				this._cameraTransform.position = this._descriptorTarget.position;
				this._cameraTransform.rotation = this._descriptorTarget.rotation;
			}
			this.EffectsLateUpdate();
		}

		public bool CameraInversionIsInverted
		{
			get
			{
				return this._cameraInversionIsInverted;
			}
			set
			{
				this._cameraInversionIsInverted = value;
				this.CameraInversionAngleY = ((!value) ? this.CameraInversionTeamBAngleY : this.CameraInversionTeamAAngleY);
			}
		}

		private void EffectsLateUpdate()
		{
			if (this._effectsShakeValue > 0f)
			{
				this._cameraTransform.position += Random.onUnitSphere * Mathf.Pow(this._effectsShakeValue, 2f) * this.SmoothDeltaTime() * 60f;
				this._effectsShakeValue -= this.SmoothDeltaTime() * 2f;
			}
		}

		public void Shake(float ammount)
		{
			this._effectsShakeValue = ammount;
		}

		public float SkyViewTargetZoom
		{
			get
			{
				if (this._spectatorService.IsSpectating)
				{
					CameraZoomLevel currentZoomLevel = this._spectatorService.GetCurrentZoomLevel();
					switch (currentZoomLevel + 1)
					{
					case CameraZoomLevel.Game:
						return this.SkyViewSpectatorFarZoom;
					case CameraZoomLevel.Near:
						return this.SkyViewDefaultZoom;
					case CameraZoomLevel.Close:
						return this.SkyViewSpectatorNearZoom;
					case (CameraZoomLevel)3:
						return this.SkyViewSpectatorCloseZoom;
					}
				}
				if (this.IsPlayerPreSpawning())
				{
					return this.SkyViewPreSpawnZoom;
				}
				return this.SkyViewDefaultZoom;
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
					this.SkyViewLoosingFocus = true;
				}
			}
		}

		private void SkyViewLateUpdate()
		{
			this._descriptorTarget.nearPlane = this.skyViewCamera.nearPlane;
			this._descriptorTarget.farPlane = this.skyViewCamera.farPlane;
			this._descriptorTarget.fov = this.skyViewCamera.fov;
			this._descriptorTarget.startFog = this.skyViewCamera.fogStart;
			this._descriptorTarget.endFog = this.skyViewCamera.fogEnd;
			this._skyViewCameraDistance = this.CameraDistance;
			Vector3 currentTargetPosition = this.CurrentTargetPosition;
			if (this.SkyViewFocusTarget != null && !this.SkyViewLoosingFocus)
			{
				float num = this.FocusCurve.Evaluate(1f - (this.SkyViewFocusTarget.position - currentTargetPosition).magnitude / this.SkyViewFocusTarget.size);
				if (num > 1f)
				{
					num = 1f;
				}
				else if (num < 0f)
				{
					num = 0f;
				}
				this._skyViewCameraDistance *= 1f + num * (this.SkyViewFocusTarget.extraViewAreaMultiplier - 1f);
			}
			else
			{
				this.SkyViewFocusTarget = null;
				this.SkyViewLoosingFocus = false;
			}
			if (this.Teleported && this._smoothTeleport && !this._smoothing)
			{
				this._initialCameraTargetPosition = currentTargetPosition;
				this._smoothing = true;
				this.Teleported = false;
				this._movementPct = 0f;
				this._smoothDampVelocity = this._cameraTargetTransform.GetComponent<CombatObject>().Movement.SpeedZ * this._initialVelocityPct;
			}
			if (this._smoothing)
			{
				this._movementPct = Mathf.SmoothDamp(this._movementPct, 1f, ref this._smoothDampVelocity, this._smoothDampDuration, float.PositiveInfinity, this.DeltaTime());
				this._movementPct = Mathf.Clamp01(this._movementPct);
				this._lastCameraTargetPosition = Vector3.Lerp(this._initialCameraTargetPosition, currentTargetPosition, this._movementPct);
				if (Mathf.Approximately(this._movementPct, 1f))
				{
					this._smoothing = false;
				}
			}
			else
			{
				this._lastCameraTargetPosition = currentTargetPosition;
			}
			this.SkyViewLookAt(this._lastCameraTargetPosition + this._SkyViewOffsetJitter);
		}

		private void CameraModeLateUpdate()
		{
			this._descriptorTarget.nearPlane = this._spectatorCamera.nearPlane;
			this._descriptorTarget.farPlane = this._spectatorCamera.farPlane;
			this._descriptorTarget.fov = this._spectatorCamera.fov;
			this._descriptorTarget.startFog = this._spectatorCamera.fogStart;
			this._descriptorTarget.endFog = this._spectatorCamera.fogEnd;
			this._descriptorTarget.position = this.CameraTargetTransform.position;
			this._descriptorTarget.rotation = this.CameraTargetTransform.rotation;
		}

		private void SkyViewLookAt(Vector3 position)
		{
			Quaternion quaternion = Quaternion.Euler(0f, this.CameraInversionAngleY - 90f, 0f);
			Vector3 vector = quaternion * Vector3.back;
			Vector3 vector2 = quaternion * Vector3.left;
			Vector2 vector3 = Vector2.zero;
			bool flag = this._spectatorService.IsSpectating && (Input.GetKey(324) || !Application.isFocused);
			this.CameraPanDistance = this._inputCameraPanLoad.GetCurrentInfraValue(CameraPanCode.MaxDistance);
			if (!this.LockPan && !flag)
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
							vector3 *= this.CameraPanDistance;
							this._joyTargetOffset = vector3;
						}
						vector3 = this._joyTargetOffset;
					}
				}
				else
				{
					vector3 = Input.mousePosition;
					float num = (float)Screen.height * 0.5f * this.screenArenaPercent;
					vector3.x -= (float)Screen.width * 0.5f;
					vector3.y -= (float)Screen.height * 0.5f;
					float num2 = vector3.magnitude;
					if (Mathf.Approximately(num2, 0f))
					{
						return;
					}
					Vector2 vector4 = vector3 / num2;
					this.deadZonePercent = this._inputCameraPanLoad.GetCurrentInfraValue(CameraPanCode.Deadzone);
					float num3 = num * this.deadZonePercent;
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
						vector3.y *= this.CameraAngleCompensationPanMultiplier;
					}
					vector3 *= this.CameraPanDistance;
				}
			}
			this.smoothSpeed = this._inputCameraPanLoad.GetCurrentInfraValue(CameraPanCode.Sensitivity);
			Vector3 vector5 = vector2 * vector3.x + vector * vector3.y;
			if (this.IsPlayerPreSpawning())
			{
				this._currentLookPosition = Vector3.Lerp(this._currentLookPosition, vector5 + position, this.SmoothDeltaTime() * this.smoothSpeed);
				this._currentCameraPositionOffset = this._currentLookPosition - position;
			}
			else
			{
				this._currentCameraPositionOffset = Vector3.Lerp(this._currentCameraPositionOffset, vector5, this.SmoothDeltaTime() * this.smoothSpeed);
				this._currentLookPosition = this._currentCameraPositionOffset + position;
			}
			this._currentCameraPositionOffset.y = 0f;
			Quaternion quaternion2 = Quaternion.Euler(0f, this.CameraInversionAngleY, this.CameraInversionAngle);
			this._skyViewCurrentZoom = Mathf.Lerp(this._skyViewCurrentZoom, this.SkyViewTargetZoom, this.SmoothDeltaTime() * this.smoothSpeed);
			Vector3 vector6 = ((!this._followTarget) ? this._targetInitialPosition : this._lastCameraTargetPosition) + quaternion2 * (Vector3.up * this._skyViewCameraDistance * this._skyViewCurrentZoom);
			this._descriptorTarget.position = vector6 + this._currentCameraPositionOffset;
			this._descriptorTarget.rotation.SetLookRotation((position - vector6).normalized, Vector3.up);
		}

		private float SmoothDeltaTime()
		{
			if (Time.timeScale > 0f)
			{
				return Time.smoothDeltaTime;
			}
			return Time.unscaledDeltaTime;
		}

		private float DeltaTime()
		{
			if (Time.timeScale > 0f)
			{
				return Time.deltaTime;
			}
			return Time.unscaledDeltaTime;
		}

		[Obsolete]
		public static bool operator ==(CarCamera a, CarCamera b)
		{
			CarCamera.Log.Fatal("DO NOT USE == OPERATOR OF CARCAMERA!");
			return false;
		}

		[Obsolete]
		public static bool operator !=(CarCamera a, CarCamera b)
		{
			CarCamera.Log.Fatal("DO NOT USE != OPERATOR OF CARCAMERA!");
			return true;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CarCamera));

		public static CarCamera Singleton;

		public static int SingletonInstanceId = -1;

		[Header("Camera object")]
		private Transform _cameraTargetTransform;

		public float CameraScrollStrength = 600f;

		public float CameraDistance = 55f;

		public float CameraDistanceToRespawn = 85f;

		private Transform _cameraTransform;

		private CarCameraMode _cameraMode;

		private PlayerRaceStartCursorLockController _raceStartCursorLockController;

		public Camera Camera;

		public int CameraInstanceId = -1;

		public float CameraDynamicZoom;

		private float _currentDynamicZoom;

		public bool Teleported;

		[Header("Post processing")]
		public MainPostProcessing postProcessing;

		private CarCamera.CameraDescriptor _descriptorCurrent;

		private CarCamera.CameraDescriptor _descriptorTarget;

		private float _interpolationValue;

		private float _interpolationSpeed;

		[Range(0f, 10f)]
		public float smoothSpeed = 1f;

		[Header("Player")]
		public Transform MyPlayerTransform;

		public CombatData MyPlayerCombatData;

		private CarCameraStack<CarCamera.CameraTargetData> _targetStack;

		public float CameraInversionAngle = 30f;

		public float CameraInversionAngleY = 315f;

		public float CameraInversionTeamAAngleY = 135f;

		public float CameraInversionTeamBAngleY = 315f;

		public bool _cameraInversionIsInverted;

		private float _effectsShakeValue;

		[SerializeField]
		private CarCamera.SkyCameraConfig _spectatorCamera = new CarCamera.SkyCameraConfig();

		[Header("Skyview")]
		[SerializeField]
		public CarCamera.SkyCameraConfig skyViewCamera = new CarCamera.SkyCameraConfig();

		public float SkyViewPreSpawnZoom = 4.4f;

		public float SkyViewDefaultZoom = 2.4f;

		public float SkyViewSpectatorCloseZoom = 0.8f;

		public float SkyViewSpectatorNearZoom = 1.2f;

		public float SkyViewSpectatorFarZoom = 4.8f;

		private float _skyViewCurrentZoom;

		[InjectOnClient]
		private IControllerInputActionPoller _inputActionPoller;

		[InjectOnClient]
		private IInputCameraPanLoad _inputCameraPanLoad;

		[InjectOnClient]
		private IInputJoystickCursorLoad _inputJoystickCursorLoad;

		[InjectOnClient]
		private ISpectatorService _spectatorService;

		[InjectOnClient]
		private IConfigLoader _config;

		private bool _followTarget;

		private Vector3 _targetInitialPosition;

		private bool _smoothTeleport;

		public SkyViewFollowMode SkyViewFollowMouse = SkyViewFollowMode.Mouse;

		private CameraFocusTrigger _skyViewFocusTarget;

		public bool SkyViewLoosingFocus;

		public AnimationCurve FocusCurve;

		public Vector3 _SkyViewOffsetJitter = Vector3.zero;

		private float _skyViewCameraDistance;

		private Vector3 _initialCameraTargetPosition;

		private Vector3 _lastCameraTargetPosition;

		private float _movementPct;

		private float _smoothDampVelocity;

		[SerializeField]
		private float _smoothDampDuration = 0.5f;

		[SerializeField]
		private float _initialVelocityPct = 0.01f;

		private bool _smoothing;

		public bool LockPan;

		public float CameraPanDistance = 15f;

		[Range(1f, 2f)]
		public float CameraAngleCompensationPanMultiplier = 1.35f;

		private Vector3 _currentLookPosition = Vector3.zero;

		private Vector3 _currentCameraPositionOffset = Vector3.zero;

		[Range(0f, 1f)]
		public float screenArenaPercent = 1f;

		[Range(0f, 1f)]
		public float deadZonePercent = 0.5f;

		public float zoneArea = 4f;

		public float timeFactor = 0.5f;

		public float mouseSensibility = 1f;

		private Vector2 _joyTargetOffset;

		[Serializable]
		private struct CameraDescriptor
		{
			public static CarCamera.CameraDescriptor Lerp(CarCamera.CameraDescriptor source, CarCamera.CameraDescriptor target, float value)
			{
				return new CarCamera.CameraDescriptor
				{
					position = Vector3.Lerp(source.position, target.position, value),
					rotation = Quaternion.Lerp(source.rotation, target.rotation, value),
					farPlane = source.farPlane * (1f - value) + target.farPlane * value,
					nearPlane = source.nearPlane * (1f - value) + target.nearPlane * value,
					fov = source.fov * (1f - value) + target.fov * value,
					startFog = source.startFog * (1f - value) + target.startFog * value,
					endFog = source.endFog * (1f - value) + target.endFog * value
				};
			}

			public Vector3 position;

			public Quaternion rotation;

			public float fov;

			public float startFog;

			public float endFog;

			public float nearPlane;

			public float farPlane;
		}

		[Serializable]
		public class SkyCameraConfig
		{
			public float fov = 60f;

			public float nearPlane = 0.1f;

			public float farPlane = 500f;

			public float fogStart = 70f;

			public float fogEnd = 80f;

			public LayerMask collisionMask;
		}

		private struct CameraTargetData
		{
			public CameraTargetData(CarCameraMode mode, Transform target, bool snap, bool follow, bool smoothTeleport)
			{
				this.Mode = mode;
				this.Target = target;
				this.Snap = snap;
				this.Follow = follow;
				this.SmoothTeleport = smoothTeleport;
			}

			public CarCameraMode Mode;

			public Transform Target;

			public bool Snap;

			public bool Follow;

			public bool SmoothTeleport;
		}
	}
}
