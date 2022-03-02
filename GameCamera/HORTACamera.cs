using System;
using HeavyMetalMachines.GameCamera.Movement;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.GameCamera
{
	[RequireComponent(typeof(Camera))]
	public class HORTACamera : GameHubBehaviour, ICleanupListener, IGameCameraState
	{
		public Transform CurrentTargetTransform
		{
			get
			{
				return this._currentTargetTransform;
			}
		}

		public bool FollowTarget
		{
			get
			{
				return this._followTarget;
			}
		}

		public bool SmoothTeleport
		{
			get
			{
				return this._smoothTeleport;
			}
		}

		public Vector3 CurrentTargetPosition
		{
			get
			{
				return (!this._followTarget) ? this._targetInitialPosition : this.CurrentTargetTransform.position;
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
		}

		private void Awake()
		{
			this._cameraTransform = base.transform;
			if (this.CurrentTargetTransform != null)
			{
				HORTACamera.CameraTargetData target = new HORTACamera.CameraTargetData
				{
					Target = this.CurrentTargetTransform,
					Mode = CarCameraMode.SkyView,
					Snap = true,
					Follow = true,
					SmoothTeleport = true
				};
				this.SetTarget(target);
			}
			this.Camera = base.GetComponent<Camera>();
			this._targetStack = new CarCameraStack<HORTACamera.CameraTargetData>(16);
			this._raceStartCursorLockController = new PlayerRaceStartCursorLockController(this._config);
		}

		public void ChangeMode(CarCameraMode mode)
		{
			if (mode == this._cameraMode)
			{
				return;
			}
			this.SetMode(mode);
			this._interpolationValue = 0f;
			this._descriptorCurrent = this._descriptorTarget;
		}

		private bool SetMode(CarCameraMode mode)
		{
			bool result = this._cameraMode != mode;
			this._cameraMode = mode;
			return result;
		}

		public void OnCleanup(CleanupMessage msg)
		{
			HORTACamera.Log.Debug("OnCleanup. Setting camera follow target to null.");
			this._currentTargetTransform = null;
			this.LockPan = false;
			this._targetStack.Clear();
		}

		public bool SetTarget(string identifier, Func<bool> condition, Transform targetTransform, CarCameraMode mode = CarCameraMode.SkyView, bool snap = true, bool follow = true, bool smoothTeleport = false)
		{
			HORTACamera.CameraTargetData cameraTargetData = new HORTACamera.CameraTargetData(mode, targetTransform, snap, follow, smoothTeleport);
			if (this._targetStack.Push(identifier, condition, cameraTargetData))
			{
				this.SetTarget(cameraTargetData);
				return true;
			}
			return false;
		}

		private void SetTarget(HORTACamera.CameraTargetData data)
		{
			bool flag = this.SetMode(data.Mode);
			this._currentTargetTransform = data.Target;
			this._followTarget = data.Follow;
			this._interpolationValue = ((!data.Snap && !flag) ? 0f : 2f);
			this._smoothTeleport = data.SmoothTeleport;
			this._descriptorCurrent = this._descriptorTarget;
			this._targetInitialPosition = ((!(this._currentTargetTransform != null)) ? Vector3.zero : this._currentTargetTransform.position);
		}

		public void ForceSnappingToTarget()
		{
			this._interpolationValue = 1f;
		}

		private void Update()
		{
			this._raceStartCursorLockController.FreeCursorLockIfTimeout();
		}

		private void LateUpdate()
		{
			this.EffectsLateUpdate();
		}

		private void EffectsLateUpdate()
		{
			if (this._effectsShakeValue > 0f)
			{
				this._cameraTransform.position += Random.onUnitSphere * Mathf.Pow(this._effectsShakeValue, 2f) * this.DeltaTime() * 60f;
				this._effectsShakeValue -= this.DeltaTime() * 2f;
			}
		}

		public void Shake(float ammount)
		{
			this._effectsShakeValue = ammount;
		}

		public bool LockPan { get; set; }

		private float DeltaTime()
		{
			return Time.unscaledDeltaTime;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTACamera));

		[Header("Camera object")]
		private Transform _currentTargetTransform;

		private Transform _cameraTransform;

		private CarCameraMode _cameraMode;

		private PlayerRaceStartCursorLockController _raceStartCursorLockController;

		public Camera Camera;

		private CameraState _descriptorCurrent;

		private CameraState _descriptorTarget;

		private float _interpolationValue;

		private float _interpolationSpeed;

		private CarCameraStack<HORTACamera.CameraTargetData> _targetStack;

		private float _effectsShakeValue;

		[Inject]
		private IConfigLoader _config;

		[Inject]
		private IGameCameraInversion _cameraInversion;

		private bool _followTarget;

		private Vector3 _targetInitialPosition;

		private bool _smoothTeleport;

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
