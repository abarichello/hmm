using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.PostProcessing;
using HeavyMetalMachines.Scene;
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

		public float CurrentCameraDistance
		{
			get
			{
				if (this.ShouldZoomOut() && GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.BombDelivery)
				{
					return this.CameraDistanceToRespawn;
				}
				return this.CameraDistance;
			}
		}

		private bool ShouldZoomOut()
		{
			return this.MyPlayerCombatData != null && this.MyPlayerCombatData.Combat != null && (this.MyPlayerCombatData.Combat.SpawnController.State == SpawnController.StateType.PreSpawned || this.MyPlayerCombatData.Combat.SpawnController.State == SpawnController.StateType.Respawning);
		}

		public Vector3 CurrentTargetPosition
		{
			get
			{
				if (this._followTarget)
				{
					return this.CameraTargetTransform.position;
				}
				return this._targetInitialPosition;
			}
		}

		public Transform CameraTransform
		{
			get
			{
				return this._cameraTransform;
			}
		}

		public CarCamera.CarCameraMode CameraMode
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
					this.SetTarget(this.CameraTargetTransform, true, true, true);
				}
				this.Camera = base.GetComponent<Camera>();
				this.CameraInstanceId = this.Camera.GetInstanceID();
				this._targetStack = new CarCameraStack<CarCamera.CameraTargetData>(16);
				this._raceStartCursorLockController = new PlayerRaceStartCursorLockController();
				return;
			}
			CarCamera.Log.Error("Singleton is already initialized");
			base.enabled = false;
			UnityEngine.Object.DestroyImmediate(this);
		}

		private void OnDestroy()
		{
			if (CarCamera.SingletonInstanceId == base.GetInstanceID())
			{
				CarCamera.Singleton = null;
				CarCamera.SingletonInstanceId = -1;
			}
		}

		private void OnPhaseChange(BombScoreBoard.State state)
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			if (state == BombScoreBoard.State.BombDelivery)
			{
				this.SetTarget("Player", () => GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery, this.MyPlayerTransform, false, true, true);
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
					return bombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery && !this.MyPlayerCombatData.IsAlive() && bombManager.BombMovement.gameObject.activeSelf;
				}, GameHubBehaviour.Hub.BombManager.BombMovement.Combat.transform, false, true, false);
				int delayTime = checked(data.EventTime + (int)(unchecked(1000f * GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex].KillCamWaitTimeSeconds)));
				this.SetTarget("RespawnDelay", delegate()
				{
					BombManager bombManager = GameHubBehaviour.Hub.BombManager;
					return bombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery && !this.MyPlayerCombatData.IsAlive() && bombManager.BombMovement.gameObject.activeSelf && GameHubBehaviour.Hub.GameTime.GetPlaybackTime() < delayTime;
				}, this.MyPlayerTransform, false, false, false);
			}
		}

		private void OnPlayerRespawning(PlayerEvent data)
		{
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			if (currentPlayerData != null && currentPlayerData.GetPlayerCarObjectId() == data.TargetId)
			{
				this.SetTarget("Respawning", () => GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.BombDelivery && !this.MyPlayerCombatData.IsAlive(), this.MyPlayerTransform, false, true, false);
			}
		}

		public void SetMode(CarCamera.CarCameraMode mode)
		{
			if (mode != CarCamera.CarCameraMode.SkyView)
			{
				if (mode == CarCamera.CarCameraMode.ScreenShot)
				{
					this._cameraMode = CarCamera.CarCameraMode.ScreenShot;
					PlayerController.LockedInputs = true;
					this._screenShotTargetMouseRotationAngle = new Vector3(180f, 0f, 0f);
					return;
				}
			}
			else
			{
				this._cameraMode = CarCamera.CarCameraMode.SkyView;
				PlayerController.LockedInputs = false;
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this._cameraTargetTransform = null;
			this.isIdleMode = false;
			this.LockPan = false;
			this._targetStack.Clear();
		}

		public void SetGameEndMode()
		{
			this.Game.SetGameEndMode();
		}

		public bool SetTarget(string identifier, Func<bool> condition, Transform targetTransform, bool snap = true, bool follow = true, bool smoothTeleport = false)
		{
			if (this._targetStack.Push(identifier, condition, new CarCamera.CameraTargetData(targetTransform, snap, follow, smoothTeleport)))
			{
				this.SetTarget(targetTransform, snap, follow, smoothTeleport);
				return true;
			}
			return false;
		}

		private void SetTarget(Transform targetTransform, bool snap, bool follow, bool smoothTeleport)
		{
			this._cameraTargetTransform = targetTransform;
			this._followTarget = follow;
			this._interpolationValue = ((!snap) ? 0f : 1f);
			this._smoothTeleport = smoothTeleport;
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
				this.CameraDistance -= axis * Time.smoothDeltaTime * this.CameraScrollStrength;
				if (this.CameraDistance < 50f)
				{
					this.CameraDistance = 50f;
				}
			}
			if (Input.GetKey(KeyCode.I))
			{
				this.CameraInversionAngle = Mathf.Max(5f, this.CameraInversionAngle - Time.smoothDeltaTime * 30f);
			}
			if (Input.GetKey(KeyCode.K))
			{
				this.CameraInversionAngle = Mathf.Min(85f, this.CameraInversionAngle + Time.smoothDeltaTime * 30f);
			}
			if (Input.GetKey(KeyCode.L))
			{
				this.CameraInversionAngleY = Mathf.Max(0f, this.CameraInversionAngleY - Time.smoothDeltaTime * 30f);
			}
			if (Input.GetKey(KeyCode.J))
			{
				this.CameraInversionAngleY = Mathf.Min(360f, this.CameraInversionAngleY + Time.smoothDeltaTime * 30f);
			}
			if (Input.GetKeyDown(KeyCode.C))
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
				CarCamera.CameraTargetData cameraTargetData = this._targetStack.Peek();
				this.SetTarget(cameraTargetData.Target, cameraTargetData.Snap, cameraTargetData.Follow, cameraTargetData.SmoothTeleport);
			}
			if (this.CameraTargetTransform != null)
			{
				CarCamera.CarCameraMode carCameraMode = this._cameraMode;
				if (carCameraMode != CarCamera.CarCameraMode.SkyView)
				{
					if (carCameraMode == CarCamera.CarCameraMode.ScreenShot)
					{
						this.ScreenShotLateUpdate();
					}
				}
				else
				{
					this.SkyViewLateUpdate();
				}
				if (this._interpolationValue <= 1f)
				{
					if (Time.timeScale > 0f)
					{
						this._interpolationValue = Mathf.SmoothDamp(this._interpolationValue, 1f, ref this._interpolationSpeed, 0.5f);
					}
					this._interpolationValue = Mathf.Clamp01(this._interpolationValue);
					this._descriptorTarget = CarCamera.CameraDescriptor.Lerp(this._descriptorCurrent, this._descriptorTarget, this._interpolationValue);
					if (Mathf.Approximately(this._interpolationValue, 1f))
					{
						this._interpolationValue = 2f;
					}
				}
				this.Camera.nearClipPlane = this._descriptorTarget.nearPlane;
				this.Camera.farClipPlane = this._descriptorTarget.farPlane;
				if (!SpectatorController.IsSpectating)
				{
					this._currentDynamicZoom = Mathf.Lerp(this._currentDynamicZoom, this.CameraDynamicZoom, Time.unscaledDeltaTime * 2f);
					this.Camera.fieldOfView = this._descriptorTarget.fov * (1f - this._currentDynamicZoom);
				}
				RenderSettings.fogStartDistance = this._descriptorTarget.startFog;
				RenderSettings.fogEndDistance = this._descriptorTarget.endFog;
				this._cameraTransform.position = this._descriptorTarget.position;
				this._cameraTransform.rotation = this._descriptorTarget.rotation;
			}
			this.EffectsLateUpdate();
			this.CheatCamera();
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
				this._cameraTransform.position += UnityEngine.Random.onUnitSphere * Mathf.Pow(this._effectsShakeValue, 2f) * Time.smoothDeltaTime * 60f;
				this._effectsShakeValue -= Time.smoothDeltaTime * 2f;
			}
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible();
			}
		}

		public void Shake(float ammount, float range)
		{
			Identifiable characterInstance = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance;
			if (characterInstance != null)
			{
				float ammount2 = ammount * Mathf.Clamp01((range - Vector3.Distance(base.transform.position, characterInstance.transform.position)) / (range * 0.2f));
				this.Shake(ammount2);
			}
		}

		public void ShakeForPosition(Vector3 position)
		{
			Vector3 b = (!(this.CameraTargetTransform == null)) ? this.CameraTargetTransform.position : this.Camera.ScreenToWorldPoint(0.5f * new Vector2((float)Screen.width, (float)Screen.height));
			float time = Vector3.Distance(position, b);
			this.Shake(Mathf.Clamp01(this.shakeFalloff.Evaluate(time)));
		}

		public void Shake(float ammount)
		{
			this._effectsShakeValue = ammount;
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
					return;
				}
				this.SkyViewLoosingFocus = true;
			}
		}

		private void SkyViewLateUpdate()
		{
			this._descriptorTarget.nearPlane = this.skyViewCamera.nearPlane;
			this._descriptorTarget.farPlane = this.skyViewCamera.farPlane;
			this._descriptorTarget.fov = this.skyViewCamera.fov;
			this._descriptorTarget.startFog = this.skyViewCamera.fogStart;
			this._descriptorTarget.endFog = this.skyViewCamera.fogEnd;
			this._skyViewCameraDistance = this.CurrentCameraDistance;
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
				if (Time.deltaTime > 0f)
				{
					this._movementPct = Mathf.SmoothDamp(this._movementPct, 1f, ref this._smoothDampVelocity, this._smoothDampDuration);
				}
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
			this.SkyViewLookAt(this._lastCameraTargetPosition + this._skyViewCurrentOffset + this._SkyViewOffsetJitter);
		}

		private float CurrentCameraPanDistance
		{
			get
			{
				if (this.ShouldZoomOut() && GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreBoard.State.BombDelivery)
				{
					return this.CameraPanDistanceToRespawn;
				}
				return this.CameraPanDistance;
			}
		}

		private void SkyViewLookAt(Vector3 position)
		{
			this.targetCameraPosition = position;
			Vector3 a = this.currentCameraPosition - position;
			float num = a.magnitude;
			float magnitude = (this.deadZoneReference - position).magnitude;
			if (num > this.zoneArea && SpectatorController.IsSpectating)
			{
				this.currentCameraPosition += a / (this.zoneArea / num);
			}
			if (magnitude < this.zoneArea)
			{
				if (!this.isIdleMode)
				{
					this.deadZoneReference = position;
				}
				this.isIdleMode = true;
				num = 0f;
			}
			else if (this.isIdleMode)
			{
				this.isIdleMode = false;
			}
			this.deadZoneReference = Vector3.MoveTowards(this.deadZoneReference, position, Time.smoothDeltaTime * ((!this.isIdleMode) ? this.DeadZoneReferenceVelocity : this.DeadZoneReferenceVelocityWhenIdleMode));
			float num2 = Vector3.Distance(this.deadZoneReference, position);
			if (num2 > this.zoneArea * 2f)
			{
				this.deadZoneReference = Vector3.MoveTowards(this.deadZoneReference, position, num2 - this.zoneArea * 2f);
			}
			this.followSpeed = Mathf.Lerp(this.followSpeed, num, Time.smoothDeltaTime * this.timeFactor);
			Quaternion rotation = Quaternion.Euler(0f, this.CameraInversionAngleY - 90f, 0f);
			Vector3 a2 = rotation * Vector3.back;
			Vector3 a3 = rotation * Vector3.left;
			Vector3 vector = this.Camera.WorldToViewportPoint(this._lastCameraTargetPosition);
			vector.x -= 0.5f;
			vector.y -= 0.5f;
			if (vector.x < -this.deadScreenArea)
			{
				this.currentCameraPosition += this._lastCameraTargetPosition - this.Camera.ViewportToWorldPoint(new Vector3(-this.deadScreenArea + 0.5f, vector.y + 0.5f, vector.z));
			}
			if (vector.x > this.deadScreenArea)
			{
				this.currentCameraPosition += this._lastCameraTargetPosition - this.Camera.ViewportToWorldPoint(new Vector3(this.deadScreenArea + 0.5f, vector.y + 0.5f, vector.z));
			}
			if (vector.y < -this.deadScreenArea)
			{
				this.currentCameraPosition += this._lastCameraTargetPosition - this.Camera.ViewportToWorldPoint(new Vector3(vector.x + 0.5f, -this.deadScreenArea + 0.5f, vector.z));
			}
			if (vector.y > this.deadScreenArea)
			{
				this.currentCameraPosition += this._lastCameraTargetPosition - this.Camera.ViewportToWorldPoint(new Vector3(vector.x + 0.5f, this.deadScreenArea + 0.5f, vector.z));
			}
			Vector2 vector2 = Vector2.zero;
			if (!this.LockPan)
			{
				CarCamera.SkyViewFollowMode skyViewFollowMouse = this.SkyViewFollowMouse;
				if (skyViewFollowMouse != CarCamera.SkyViewFollowMode.Mouse)
				{
					if (skyViewFollowMouse != CarCamera.SkyViewFollowMode.JoyAxis)
					{
						if (skyViewFollowMouse != CarCamera.SkyViewFollowMode.None)
						{
						}
					}
					else
					{
						vector2.x = Mathf.Pow(Input.GetAxis("Joy1 Axis 4"), 3f);
						vector2.y = Mathf.Pow(-Input.GetAxis("Joy1 Axis 5"), 3f);
						vector2 = Vector3.ClampMagnitude(vector2, 1f);
						vector2 *= this.CurrentCameraPanDistance;
					}
				}
				else if (this.cameraMode == CarCamera.CameraTestMode.Default)
				{
					vector2 = Input.mousePosition;
					float num3 = (float)Screen.height * 0.5f * this.screenArenaPercent;
					vector2.x -= (float)Screen.width * 0.5f;
					vector2.y -= (float)Screen.height * 0.5f;
					float num4 = vector2.magnitude;
					if (Mathf.Approximately(num4, 0f))
					{
						return;
					}
					Vector2 a4 = vector2 / num4;
					float num5 = num3 * this.deadZonePercent;
					if (num4 < num5)
					{
						num4 = num5;
					}
					if (num4 > num3)
					{
						num4 = num3;
					}
					num4 = (num4 - num5) / (num3 - num5);
					vector2 = a4 * num4;
					if (vector2.y < 0f)
					{
						vector2.y *= this.CameraAngleCompensationPanMultiplier;
					}
					vector2 *= this.CurrentCameraPanDistance;
				}
				else
				{
					Vector3 vector3 = this.CameraTargetTransform.forward;
					if (this.cameraMode == CarCamera.CameraTestMode.FollowBomb)
					{
						BombVisualController instance = BombVisualController.GetInstance(false);
						if (instance)
						{
							vector3 = (instance.transform.position - this.CurrentTargetPosition).normalized;
						}
					}
					vector3 *= this.CurrentCameraPanDistance;
					vector2 = Quaternion.Euler(0f, 0f, -45f) * new Vector2(vector3.x, -vector3.z);
				}
			}
			if (SpectatorController.IsSpectating && Input.GetKey(KeyCode.Mouse1))
			{
				vector2 = Vector3.zero;
			}
			this.currentCameraPositionOffset = Vector3.Lerp(this.currentCameraPositionOffset, a3 * vector2.x + a2 * vector2.y, Time.smoothDeltaTime * this.smoothSpeed);
			this.currentCameraPositionOffset.y = 0f;
			this.currentCameraPosition.y = position.y;
			this.currentCameraPosition = Vector3.MoveTowards(this.currentCameraPosition, position, this.followSpeed * Mathf.Clamp01(num / 10f));
			Quaternion rotation2 = Quaternion.Euler(0f, this.CameraInversionAngleY, this.CameraInversionAngle);
			Vector3 vector4 = ((!this._followTarget) ? this._targetInitialPosition : this._lastCameraTargetPosition) + rotation2 * (Vector3.up * this._skyViewCameraDistance * this.SkyViewCurrentZoom);
			this._descriptorTarget.position = vector4 + this.currentCameraPositionOffset;
			this._descriptorTarget.rotation.SetLookRotation((position - vector4).normalized, Vector3.up);
		}

		private void ScreenShotSetTarget(Transform transf)
		{
			this._screenShotRenderers = transf.GetComponentsInChildren<Renderer>(true);
			bool flag = true;
			checked
			{
				for (int i = 0; i < this._screenShotRenderers.Length; i++)
				{
					Renderer renderer = this._screenShotRenderers[i];
					if (renderer is SkinnedMeshRenderer || renderer is MeshRenderer)
					{
						if (flag)
						{
							flag = false;
							this._screenShotTargetObjectBounds = renderer.bounds;
						}
						else
						{
							this._screenShotTargetObjectBounds.Encapsulate(renderer.bounds);
						}
					}
				}
				this._screenShotTargetObjectBounds.center = this._screenShotTargetObjectBounds.center - this.CameraTargetTransform.position;
			}
		}

		private void ScreenShotLateUpdate()
		{
			Vector3 vector;
			float d;
			if (this._screenShotRenderers != null)
			{
				vector = this._screenShotTargetObjectBounds.center + this.CameraTargetTransform.position;
				d = this._screenShotTargetObjectBounds.size.magnitude;
			}
			else
			{
				vector = this.CameraTargetTransform.position;
				d = 1f;
			}
			this._descriptorTarget.nearPlane = this.screenShotCamera.nearPlane;
			this._descriptorTarget.farPlane = this.screenShotCamera.farPlane;
			this._descriptorTarget.fov = this.screenShotCamera.fov;
			this._descriptorTarget.startFog = this.screenShotCamera.fogStart;
			this._descriptorTarget.endFog = this.screenShotCamera.fogEnd;
			Vector3 b = new Vector3(Input.GetAxis("Mouse X") * 3f, -Input.GetAxis("Mouse Y") * 3f, 0f);
			if (!GameHubBehaviour.Hub.GuiScripts.Esc.IsWindowVisible())
			{
				this._screenShotTargetMouseRotationAngle += b;
			}
			if (this._screenShotTargetMouseRotationAngle.y < 10f)
			{
				this._screenShotTargetMouseRotationAngle.y = 10f;
			}
			if (this._screenShotTargetMouseRotationAngle.y > 80f)
			{
				this._screenShotTargetMouseRotationAngle.y = 80f;
			}
			Vector3 vector2 = Vector3.zero;
			this._screenShotCurrentMouseRotationAngle = Quaternion.Slerp(this._screenShotCurrentMouseRotationAngle, this.CameraTargetTransform.transform.rotation * Quaternion.Euler(-this._screenShotTargetMouseRotationAngle.y, this._screenShotTargetMouseRotationAngle.x, this._screenShotTargetMouseRotationAngle.z), Time.smoothDeltaTime * 7f);
			vector2 = this._screenShotCurrentMouseRotationAngle * Vector3.forward * d * this.ScreenShotZoom;
			vector2 += vector;
			Vector3 vector3 = vector2 - vector;
			RaycastHit raycastHit;
			if (Physics.Raycast(vector, vector3.normalized, out raycastHit, vector3.magnitude, this.screenShotCamera.collisionMask))
			{
				vector2 = raycastHit.point;
			}
			this._descriptorTarget.position = vector2;
			this._descriptorTarget.rotation.SetLookRotation((vector - vector2).normalized, Vector3.up);
		}

		[Obsolete]
		public static bool operator ==(CarCamera a, CarCamera b)
		{
			CarCamera.Log.FatalFormat("DO NOT USE == OPERATOR OF CARCAMERA!", new object[0]);
			return false;
		}

		[Obsolete]
		public static bool operator !=(CarCamera a, CarCamera b)
		{
			CarCamera.Log.FatalFormat("DO NOT USE != OPERATOR OF CARCAMERA!", new object[0]);
			return true;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawLine(this.currentCameraPosition, this.targetCameraPosition);
			Gizmos.DrawSphere(this.targetCameraPosition, 0.5f);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(this.deadZoneReference, this.targetCameraPosition);
			Gizmos.DrawSphere(this.deadZoneReference, 0.5f);
		}

		private void CheatCamera()
		{
			if (Input.GetKey(KeyCode.KeypadPlus))
			{
				this.CameraDistance -= 1f;
			}
			if (Input.GetKey(KeyCode.KeypadMinus))
			{
				this.CameraDistance += 1f;
			}
			if (Input.GetKey(KeyCode.KeypadDivide))
			{
				this.CameraDistance = 55f;
			}
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

		private CarCamera.CarCameraMode _cameraMode;

		private IRaceStartCursorLockController _raceStartCursorLockController;

		public Camera Camera;

		public int CameraInstanceId = -1;

		public float CameraDynamicZoom;

		private float _currentDynamicZoom;

		public bool Teleported;

		[Header("Post processing")]
		public MainPostProcessing postProcessing;

		[Header("Game State")]
		public Game Game;

		private CarCamera.CameraDescriptor _descriptorCurrent;

		private CarCamera.CameraDescriptor _descriptorTarget;

		[Header("Animation")]
		public AnimationCurve shakeFalloff;

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

		[Header("Skyview")]
		[SerializeField]
		public CarCamera.SkyCameraConfig skyViewCamera = new CarCamera.SkyCameraConfig();

		public float SkyViewZoomFactor;

		public float SkyViewCurrentZoom = 2.4f;

		public float SkyViewMaxFollowDistance = 20f;

		public float SkyViewMouseDeadZoneRadius = 10f;

		public float SkyViewSpeedMax = 30f;

		private bool _followTarget;

		private Vector3 _targetInitialPosition;

		private bool _smoothTeleport;

		public CarCamera.SkyViewFollowMode SkyViewFollowMouse = CarCamera.SkyViewFollowMode.Mouse;

		public float SkyViewFollowMouseFactor = 1f;

		public float SkyViewFollowMouseDelay = 1.5f;

		private CameraFocusTrigger _skyViewFocusTarget;

		public bool SkyViewLoosingFocus;

		public AnimationCurve FocusCurve;

		public Vector3 _SkyViewOffsetJitter = Vector3.zero;

		private Vector3 _skyViewCurrentOffset = Vector3.zero;

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

		public float CameraPanDistanceToRespawn = 50f;

		[Range(1f, 2f)]
		public float CameraAngleCompensationPanMultiplier = 1.35f;

		private Vector3 currentCameraPosition = Vector3.zero;

		private Vector3 currentCameraPositionOffset = Vector3.zero;

		private Vector3 targetCameraPosition = Vector3.zero;

		private Vector3 deadZoneReference = Vector3.zero;

		private float followSpeed;

		[Range(0f, 1f)]
		public float screenArenaPercent = 1f;

		[Range(0f, 1f)]
		public float deadZonePercent = 0.5f;

		public float zoneArea = 4f;

		public float timeFactor = 0.5f;

		public float DeadZoneReferenceVelocity = 50f;

		public float DeadZoneReferenceVelocityWhenIdleMode = 50f;

		public float deadScreenArea = 0.25f;

		public float mouseSensibility = 1f;

		public bool isIdleMode;

		public CarCamera.CameraTestMode cameraMode;

		[Header("Screen Shot Camera")]
		[SerializeField]
		public CarCamera.ScreenShotCameraConfig screenShotCamera = new CarCamera.ScreenShotCameraConfig();

		public float ScreenShotZoom = 1f;

		private float _screenShotZoomVelocity;

		private Quaternion _screenShotCurrentMouseRotationAngle;

		private Vector3 _screenShotTargetMouseRotationAngle;

		private Bounds _screenShotTargetObjectBounds;

		private Renderer[] _screenShotRenderers;

		public enum CarCameraMode
		{
			SkyView,
			ScreenShot
		}

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
		public class ScreenShotCameraConfig
		{
			public float fov = 60f;

			public float nearPlane = 0.1f;

			public float farPlane = 100f;

			public float fogStart = 70f;

			public float fogEnd = 80f;

			public float bordersSensitivityDepth;

			public LayerMask collisionMask;
		}

		[Serializable]
		public class SkyCameraConfig
		{
			public float fov = 60f;

			public float nearPlane = 0.1f;

			public float farPlane = 500f;

			public float fogStart = 70f;

			public float fogEnd = 80f;

			public float bordersSensitivityDepth;

			public LayerMask collisionMask;
		}

		private struct CameraTargetData
		{
			public CameraTargetData(Transform target, bool snap, bool follow, bool smoothTeleport)
			{
				this.Target = target;
				this.Snap = snap;
				this.Follow = follow;
				this.SmoothTeleport = smoothTeleport;
			}

			public Transform Target;

			public bool Snap;

			public bool Follow;

			public bool SmoothTeleport;
		}

		public enum SkyViewFollowMode
		{
			None,
			Mouse,
			JoyAxis
		}

		public enum CameraTestMode
		{
			Default,
			CarForward,
			FollowBomb
		}
	}
}
