using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using Hoplon.Input;
using Hoplon.Unity.Loading;
using Pocketverse;
using UnityEngine;
using UnityEngine.Serialization;

namespace HeavyMetalMachines.Car
{
	public class CarIndicator : GameHubBehaviour, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		public static void PrecacheCarIndicator()
		{
			Content asset = Loading.Content.GetAsset("car_indicator");
			GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset));
			Content asset2 = Loading.Content.GetAsset("input_arrow");
			GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset2));
			asset2 = Loading.Content.GetAsset("input_arrow_repeat");
			GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset2));
			asset2 = Loading.Content.GetAsset("indicator_mouse_inner_ring");
			GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset2));
			asset2 = Loading.Content.GetAsset("indicator_mouse_outer_ring");
			GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset2));
			asset2 = Loading.Content.GetAsset("drift_indicator_base");
			GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset2));
			asset2 = Loading.Content.GetAsset("aura_glow_base");
			GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset2));
			asset2 = Loading.Content.GetAsset("crosshair");
			GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset2));
			Content asset3 = Loading.Content.GetAsset("PlayerIndicator");
			GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset3));
			for (int i = 0; i < CarIndicatorArrow.CarIndicatorArrowTextureNames.Length; i++)
			{
				string assetName = CarIndicatorArrow.CarIndicatorArrowTextureNames[i];
				asset2 = Loading.Content.GetAsset(assetName);
				GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset2));
			}
			CarIndicator._tintColorPropertyId = Shader.PropertyToID("_TintColor");
			CarIndicator._mainTexPropertyId = Shader.PropertyToID("_MainTex");
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			if (this._data == null)
			{
				this._data = base.GetComponent<CombatData>();
			}
			if (evt.Id != this._data.Combat.Player.PlayerCarId)
			{
				return;
			}
			this._isPlayerBuilt = true;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.ListenToObjectSpawn;
			GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn += this.ListenToPreObjectSpawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning += this.ListenToObjectRespawning;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.ListenToObjectUnspawn;
			GameHubBehaviour.Hub.Options.Game.ShowPlayerIndicatorChanged += this.ListenPlayerIndicatorChanged;
			GameHubBehaviour.Hub.Options.Game.PlayerIndicatorAlphaChanged += this.ListePlayerIndicatorAlphaChange;
			GameHubBehaviour.Hub.GuiScripts.LoadingVersus.OnPosHideLoading += this.ListenOnPosHideLoading;
			LogoTransition.OnAnimationEnd += this.ListenOnLogoAnimationEnd;
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.ListenToObjectSpawn;
			GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn -= this.ListenToPreObjectSpawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning -= this.ListenToObjectRespawning;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.ListenToObjectUnspawn;
			GameHubBehaviour.Hub.Options.Game.ShowPlayerIndicatorChanged -= this.ListenPlayerIndicatorChanged;
			GameHubBehaviour.Hub.Options.Game.PlayerIndicatorAlphaChanged -= this.ListePlayerIndicatorAlphaChange;
			GameHubBehaviour.Hub.GuiScripts.LoadingVersus.OnPosHideLoading -= this.ListenOnPosHideLoading;
			LogoTransition.OnAnimationEnd -= this.ListenOnLogoAnimationEnd;
			this._driftIndicator.Destroy();
		}

		private void ListenOnPosHideLoading()
		{
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.EndGame)
			{
				return;
			}
			bool flag = this.IsBombDeliveryState(GameHubBehaviour.Hub.BombManager.CurrentBombGameState);
			if (flag)
			{
				this._playerIndicator.SetVisibility(true);
			}
			else if (!GameHubBehaviour.Hub.BombManager.AnyTeamHasScored)
			{
				this._playerIndicator.SetVisibilityAnimated(true);
				this.TriggerGridHighlightGadget();
			}
		}

		private void ListenOnLogoAnimationEnd()
		{
			if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.EndGame)
			{
				return;
			}
			this._playerIndicator.SetVisibilityAnimated(true);
			this.TriggerGridHighlightGadget();
		}

		private void TriggerGridHighlightGadget()
		{
			if (!this._playerIndicator.IsBorderEnabled)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance != null)
			{
				PlayerController bitComponent = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<PlayerController>();
				bitComponent.TriggerGridHighlightGadget();
			}
		}

		private void ListenPlayerIndicatorChanged(bool showPlayerIndicator)
		{
			this._playerIndicator.SetBorderEnable(showPlayerIndicator);
			this._driftIndicator.SetDriftIndicatorEnable(!showPlayerIndicator);
		}

		private void ListePlayerIndicatorAlphaChange(float alpha)
		{
			this._playerIndicator.SetBorderAlpha(alpha);
		}

		private void OnPhaseChange(BombScoreboardState state)
		{
			bool visibility = this.IsBombDeliveryState(state);
			this.SetVisibility(visibility);
			this.CheckSetPlayerIndicator(state);
		}

		private bool IsBombDeliveryState(BombScoreboardState state)
		{
			return state == BombScoreboardState.BombDelivery || state == BombScoreboardState.PreBomb || GameHubBehaviour.Hub.Match.LevelIsTutorial();
		}

		private void CheckSetPlayerIndicator(BombScoreboardState state)
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this._playerIndicator.SetVisibility(true);
			}
			else if (state == BombScoreboardState.PreReplay || state == BombScoreboardState.Replay)
			{
				this._playerIndicator.SetVisibility(false);
			}
		}

		public void SetVisibility(bool isVisible)
		{
			isVisible &= (this._data.Combat.SpawnController.State == SpawnStateKind.Spawned);
			if (this._data.Combat.Player.IsCurrentPlayer)
			{
				this._indicatorTransform.gameObject.SetActive(isVisible);
				this._driftIndicator.SetVisibility(isVisible);
				this.ArrowsTransform.gameObject.SetActive(isVisible);
				bool active = this._data.Combat.SpawnController.State == SpawnStateKind.PreSpawned && GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.BombDelivery;
				this._respawnTargetTransform.gameObject.SetActive(active);
			}
			this._auraTransform.gameObject.SetActive(isVisible);
			this.SetIndicatorRayVisibility(isVisible && this._data.Combat.Player.IsCurrentPlayer);
		}

		private void MovementModeChanged(CarInput.DrivingStyleKind drivingStyleKind)
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreboardState.BombDelivery || !this._data.Combat.Player.IsCurrentPlayer)
			{
				return;
			}
			this.SetIndicatorRayVisibility(drivingStyleKind == CarInput.DrivingStyleKind.FollowMouse);
		}

		private void ListenToObjectSpawn(PlayerEvent data)
		{
			if (this._playerData == null || data.TargetId != this._playerData.PlayerCarId)
			{
				return;
			}
			this.SetVisibility(true);
			bool flag = this.IsBombDeliveryState(GameHubBehaviour.Hub.BombManager.CurrentBombGameState);
			if (flag)
			{
				this._playerIndicator.SetVisibility(true);
			}
		}

		private void ListenToPreObjectSpawn(PlayerEvent data)
		{
			if (this._playerData == null || data.TargetId != this._playerData.PlayerCarId)
			{
				return;
			}
			this.SetVisibility(false);
			this._playerIndicator.SetVisibility(false);
		}

		private void ListenToObjectRespawning(PlayerEvent data)
		{
			if (this._playerData == null || data.TargetId != this._playerData.PlayerCarId)
			{
				return;
			}
			this.SetVisibility(false);
			this._playerIndicator.SetVisibility(false);
		}

		private void ListenToObjectUnspawn(PlayerEvent data)
		{
			if (this._playerData == null || data.TargetId != this._playerData.PlayerCarId)
			{
				return;
			}
			this.SetVisibility(false);
			this._playerIndicator.SetVisibility(false);
		}

		public void AddCarIndicator(Transform carTransform, PlayerData playerData, Color playerColor)
		{
			this._mainTransform = (Transform)Loading.Content.GetAsset("car_indicator").Asset;
			this._mainTransform = Object.Instantiate<Transform>(this._mainTransform);
			this._mainTransform.parent = carTransform;
			this._mainTransform.localPosition = new Vector3(0f, 0.5f, 0f);
			this._mainTransform.localRotation = carTransform.localRotation;
			this._carIndicatorTexture = (Texture)Loading.Content.GetAsset("input_arrow").Asset;
			this._carIndicatorRespawnTexture = (Texture)Loading.Content.GetAsset("crosshair").Asset;
			this._carIndicatorRayTexture = (Texture)Loading.Content.GetAsset("input_arrow_repeat").Asset;
			this._carAuraTexture = (Texture)Loading.Content.GetAsset("aura_glow_base").Asset;
			this._indicatorTransform = this._mainTransform.Find("indicator");
			this._respawnTargetTransform = this._mainTransform.Find("respawn_crosshair");
			this._indicatorRayRingTransform = this._mainTransform.Find("indicator_ray_ring");
			this._auraTransform = this._mainTransform.Find("aura");
			this.ArrowsTransform = this._mainTransform.Find("arrows");
			this.InitializeDriftIndicator(playerColor);
			this.InitializePlayerIndicator();
			this._playerData = playerData;
			this._carInput = carTransform.GetComponent<CarInput>();
			this._playerController = carTransform.GetComponent<PlayerController>();
			this._indicatorTransform.localPosition = new Vector3(0f, 0.5f, this._playerData.Character.IndicatorConfig.IndicatorArrowOffset);
			this._indicatorRenderer = this._indicatorTransform.GetComponent<MeshRenderer>();
			this._indicatorColor = playerColor;
			this._indicatorRayRenderer = this._mainTransform.Find("indicator_ray").GetComponent<LineRenderer>();
			MeshRenderer component = this._auraTransform.GetComponent<MeshRenderer>();
			component.material.SetTexture(CarIndicator._mainTexPropertyId, this._carAuraTexture);
			component.material.SetColor(CarIndicator._tintColorPropertyId, this._indicatorColor * 0.5f);
			this._auraTransform.localScale = new Vector3(this._playerData.Character.IndicatorConfig.PlayerBorderIndicatorSize, this._playerData.Character.IndicatorConfig.PlayerBorderIndicatorSize, this._playerData.Character.IndicatorConfig.PlayerBorderIndicatorSize);
			this._indicatorRayRingRenderer = this._indicatorRayRingTransform.GetComponent<MeshRenderer>();
			this._indicatorRayInnerRingRenderer = this._indicatorRayRingTransform.Find("indicator_ray_inner_ring").GetComponent<MeshRenderer>();
			this.SetIndicatorRayVisibility(false);
			this.ArrowsTransform.gameObject.SetActive(false);
			this._carIndicatorMeshRenderer = this._indicatorTransform.GetComponent<MeshRenderer>();
			this._carIndicatorMeshRenderer.material.SetTexture(CarIndicator._mainTexPropertyId, this._carIndicatorTexture);
			if (this._playerController.Combat.Id.IsOwner)
			{
				this.SetMeshRendererTextureAndColor(this._respawnTargetTransform, this._carIndicatorRespawnTexture, GUIColorsInfo.Instance.RespawnCrosshairColor);
			}
			else
			{
				this._respawnTargetTransform.gameObject.SetActive(false);
			}
			this.SetPlayerIndicatorsVisibility(this._playerData.IsCurrentPlayer);
			this._configured = true;
			this._currentDrivingStyle = ((!(this._carInput == null)) ? this._carInput.CurrentDrivingStyle : CarInput.DrivingStyleKind.FollowMouse);
		}

		private void InitializeDriftIndicator(Color playerColor)
		{
			Transform driftIndicatorTransform = this._mainTransform.Find("drift_indicator");
			Texture driftTexture = (Texture)Loading.Content.GetAsset("drift_indicator_base").Asset;
			this._driftIndicator = new DriftIndicator();
			this._driftIndicator.Initialize(driftIndicatorTransform, driftTexture, playerColor);
			this._driftIndicator.SetDriftIndicatorEnable(!GameHubBehaviour.Hub.Options.Game.ShowPlayerIndicator);
		}

		private void InitializePlayerIndicator()
		{
			Transform indicatorTransform = this._mainTransform.Find("player_indicator");
			this._playerIndicator = base.gameObject.AddComponent<PlayerIndicator>();
			Texture borderTexture = (Texture)Loading.Content.GetAsset("PlayerIndicator").Asset;
			Texture driftTexture = (Texture)Loading.Content.GetAsset("input_arrow").Asset;
			this._playerIndicator.Intialize(borderTexture, indicatorTransform, driftTexture);
			this._playerIndicator.SetBorderEnable(GameHubBehaviour.Hub.Options.Game.ShowPlayerIndicator);
			this._playerIndicator.SetBorderAlpha(GameHubBehaviour.Hub.Options.Game.GetConverterPlayerIndicatorAlpha());
		}

		private void SetMeshRendererTextureAndColor(Transform transform, Texture texture, Color color)
		{
			MeshRenderer component = transform.GetComponent<MeshRenderer>();
			component.material.SetTexture(CarIndicator._mainTexPropertyId, texture);
			component.material.SetColor(CarIndicator._tintColorPropertyId, color);
		}

		private void SetPlayerIndicatorsVisibility(bool isCurrentPlayer)
		{
			if (!isCurrentPlayer)
			{
				this._indicatorTransform.gameObject.SetActive(false);
				this._driftIndicator.SetVisibility(false);
				return;
			}
			this._carIndicatorArrow = base.gameObject.AddComponent<CarIndicatorArrow>();
			this._carIndicatorArrow.Initialize(this._carInput.transform, this._playerData.Team, this.ArrowsTransform, GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId, this._gameCameraEngine);
			this._indicatorRayRenderer.material.SetTexture(CarIndicator._mainTexPropertyId, this._carIndicatorRayTexture);
			this._indicatorRayRingRenderer.material.SetTexture(CarIndicator._mainTexPropertyId, (Texture)Loading.Content.GetAsset("indicator_mouse_outer_ring").Asset);
			this._indicatorRayInnerRingRenderer.material.SetTexture(CarIndicator._mainTexPropertyId, (Texture)Loading.Content.GetAsset("indicator_mouse_inner_ring").Asset);
			this._indicatorRayRenderer.material.SetColor(CarIndicator._tintColorPropertyId, this._indicatorColor * 0.5f);
			this._indicatorRayRingRenderer.material.SetColor(CarIndicator._tintColorPropertyId, this._indicatorColor * 0.5f);
			this._indicatorRayInnerRingRenderer.material.SetColor(CarIndicator._tintColorPropertyId, this._indicatorColor * 0.5f);
		}

		private void LateUpdate()
		{
			if (!this._configured || !this._isPlayerBuilt)
			{
				return;
			}
			if (this.load)
			{
				this.load = false;
				Content asset = Loading.Content.GetAsset(this.TextureName);
				GameHubBehaviour.Hub.State.Current.LoadingToken.AddLoadable(Loading.GetResourceLoadable(asset));
				Loading.Engine.LoadToken(GameHubBehaviour.Hub.State.Current.LoadingToken, delegate(LoadingResult result)
				{
					if (LoadStatusExtensions.IsError(result.Status))
					{
						return;
					}
					this._crosshairTexture = (Texture)Loading.Content.GetAsset(this.TextureName).Asset;
					this._carIndicatorMeshRenderer.material.SetTexture(CarIndicator._mainTexPropertyId, this._crosshairTexture);
				});
			}
			if (this.refresh)
			{
				this.refresh = false;
				this._crosshairTexture = (Texture)Loading.Content.GetAsset(this.TextureName).Asset;
				this._carIndicatorMeshRenderer.material.SetTexture(CarIndicator._mainTexPropertyId, this._crosshairTexture);
			}
			if (this._carInput != null)
			{
				float num = (this._playerData.Character.Car.ForwardAcceleration + this._data.Combat.Attributes.AccelerationMod) * (1f + this._data.Combat.Attributes.AccelerationModPct);
				if (Mathf.Abs(this._maxSafeInputAngle - this._carInput.MaxSafeInputAngle) > 0.0001f && num > 0f)
				{
					this._maxSafeInputAngle = this._carInput.MaxSafeInputAngle;
					this._driftIndicator.SetupDriftMesh(this._maxSafeInputAngle, this._playerData.Character.IndicatorConfig.DriftIndicatorRadius, this._playerData.Character.IndicatorConfig.DriftIndicatorThickness, this._playerData.Character.IndicatorConfig.DriftIndicatorXDiff, this._playerData.Character.IndicatorConfig.DriftIndicatorZDiff);
					this._playerIndicator.SetupPlayerIndicator(this._maxSafeInputAngle, this._playerData.Character.IndicatorConfig.PlayerBorderIndicatorSize, this._playerData.Character.IndicatorConfig.PlayerBorderOffsetPosition);
				}
				float targetY = this._carInput.TargetY;
				if (this._maxSafeInputAngle < Mathf.Abs(targetY))
				{
					this._indicatorRenderer.material.SetColor(CarIndicator._tintColorPropertyId, Color.white * 0.5f);
				}
				else
				{
					this._indicatorRenderer.material.SetColor(CarIndicator._tintColorPropertyId, this._indicatorColor * 0.5f);
				}
				bool isReverse = this._carInput.InverseReverse && this._carInput.IsReverse;
				this._playerIndicator.UpdatePlayerBorder(isReverse);
				this._driftIndicator.RotateDriftIndicator(isReverse);
				if (this._activeDeviceInput.GetActiveDevice() != 3)
				{
					Ray ray = this._gameCameraEngine.UnityCamera.ScreenPointToRay(Input.mousePosition);
					Vector3 vector;
					if (!UnityUtils.RayCastGroundPlane(ray, out vector, 0f))
					{
						return;
					}
					Vector3 position = this._indicatorTransform.parent.position;
					position.y = vector.y;
					Vector3 vector2 = Vector3.Normalize(vector - position);
					Quaternion quaternion = Quaternion.LookRotation(vector2);
					Vector3 vector3 = position + this._playerData.Character.IndicatorConfig.IndicatorArrowOffset * vector2;
					this._indicatorTransform.SetPositionAndRotation(vector3, quaternion);
					Vector3 forward = this._indicatorTransform.forward;
					Vector3 vector4 = this._indicatorTransform.position + forward * this._carInput.RayConfig.Offset;
					float num2 = 0f;
					if ((base.transform.position - vector).sqrMagnitude > this._playerData.Character.IndicatorConfig.IndicatorArrowOffset * this._playerData.Character.IndicatorConfig.IndicatorArrowOffset)
					{
						num2 = Vector3.Distance(vector4, vector);
					}
					this._indicatorRayRenderer.SetPosition(0, vector4);
					this._indicatorRayRenderer.widthMultiplier = this._carInput.RayConfig.Width;
					this._indicatorRayRenderer.SetPosition(1, vector4 + forward * (num2 - this._carInput.RayConfig.RingRadius));
					this._indicatorRayRingTransform.position = vector4 + forward * num2;
					this._indicatorRayRingTransform.position = vector4 + forward * num2;
					if (!Mathf.Approximately(this._carInput.TargetV, 0f))
					{
						this._indicatorRayInnerRingRenderer.transform.localScale = Vector3.MoveTowards(this._indicatorRayInnerRingRenderer.transform.localScale, Vector3.one * this._carInput.RayConfig.RingMinScale, this._carInput.RayConfig.RingSpeed * Time.deltaTime);
					}
					else
					{
						this._indicatorRayInnerRingRenderer.transform.localScale = Vector3.MoveTowards(this._indicatorRayInnerRingRenderer.transform.localScale, Vector3.one, this._carInput.RayConfig.RingSpeed * Time.deltaTime);
					}
				}
				if (this._currentDrivingStyle != this._carInput.CurrentDrivingStyle)
				{
					this._currentDrivingStyle = this._carInput.CurrentDrivingStyle;
					this.MovementModeChanged(this._currentDrivingStyle);
				}
				this.UpdateRespawnTarget();
			}
		}

		private void UpdateRespawnTarget()
		{
			if (this._respawnTargetTransform.gameObject.activeSelf)
			{
				Vector3 mousePos = this._playerController.Inputs.MousePos;
				this._respawnTargetTransform.SetPositionAndRotation(mousePos, Quaternion.AngleAxis(45f, Vector3.up));
			}
		}

		private void SetIndicatorRayVisibility(bool visible)
		{
			visible &= (this._currentDrivingStyle == CarInput.DrivingStyleKind.FollowMouse && !SpectatorController.IsSpectating && this._data.Combat.SpawnController.State == SpawnStateKind.Spawned);
			this._indicatorTransform.gameObject.SetActive(visible);
			this._indicatorRayRenderer.gameObject.SetActive(visible);
			this._indicatorRayRingTransform.gameObject.SetActive(visible);
		}

		public void SetPlayerIndicationBorderAnimationConfig(PlayerIndicatorBorderAnimationConfig config)
		{
			this._playerIndicator.SetPlayerIndicationBorderAnimationConfig(config);
		}

		private void OnEnable()
		{
			SpectatorController.EvtSpectatorRoleChanged += this.onSpectatorRoleChanged;
		}

		private void OnDisable()
		{
			SpectatorController.EvtSpectatorRoleChanged -= this.onSpectatorRoleChanged;
		}

		private void onSpectatorRoleChanged(SpectatorRole updatedSpectatorRole)
		{
			bool flag = updatedSpectatorRole != SpectatorRole.None;
			this.SetIndicatorRayVisibility(!flag);
			if (this.ArrowsTransform != null)
			{
				this.ArrowsTransform.gameObject.SetActive(!flag);
			}
			bool playerIndicatorsVisibility = updatedSpectatorRole == SpectatorRole.None && this._playerData.IsCurrentPlayer;
			this.SetPlayerIndicatorsVisibility(playerIndicatorsVisibility);
		}

		[InjectOnClient]
		private IInputGetActiveDevicePoller _activeDeviceInput;

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		private const string CarIndicatorPrefabName = "car_indicator";

		private const string CarIndicatorChildPrefabName = "indicator";

		private const string CarIndicatorChildRayPrefabName = "indicator_ray";

		private const string CarIndicatorChildRayRingPrefabName = "indicator_ray_ring";

		private const string CarIndicatorChildRayInnerRingPrefabName = "indicator_ray_inner_ring";

		private const string CarIndicatorChildDriftPrefabName = "drift_indicator";

		private const string CarIndicatorChildArrowPrefabName = "arrows";

		private const string CarIndicatorChildAuraPrefabName = "aura";

		private const string CarIndicatorChildRespawnPrefabName = "respawn_crosshair";

		private const string CarIndicatorRespawnTextureName = "crosshair";

		private const string CarIndicatorTextureName = "input_arrow";

		private const string CarIndicatorRayTexture = "input_arrow_repeat";

		private const string CarIndicatorRayRingTexture = "indicator_mouse_outer_ring";

		private const string CarIndicatorRayInnerRingTexture = "indicator_mouse_inner_ring";

		private const string CarDriftIndicatorTextureName = "drift_indicator_base";

		private const string CarAuraTextureName = "aura_glow_base";

		private const string CarIndicatorPlayerIndicatorTextureName = "PlayerIndicator";

		private const string CarIndicatorPlayerIndicatorTransform = "player_indicator";

		private Texture _carIndicatorTexture;

		private Texture _carIndicatorRespawnTexture;

		private Texture _carIndicatorRayTexture;

		private Texture _carAuraTexture;

		private PlayerIndicator _playerIndicator;

		private DriftIndicator _driftIndicator;

		private Transform _mainTransform;

		private Transform _indicatorTransform;

		private Transform _respawnTargetTransform;

		private Transform _auraTransform;

		internal Transform ArrowsTransform;

		private CarIndicatorArrow _carIndicatorArrow;

		private CarInput _carInput;

		private PlayerController _playerController;

		private MeshRenderer _indicatorRenderer;

		private float _maxSafeInputAngle;

		private Color _indicatorColor;

		private LineRenderer _indicatorRayRenderer;

		private Transform _indicatorRayRingTransform;

		private MeshRenderer _indicatorRayRingRenderer;

		private MeshRenderer _indicatorRayInnerRingRenderer;

		private static int _tintColorPropertyId = -1;

		private static int _mainTexPropertyId = -1;

		private CarInput.DrivingStyleKind _currentDrivingStyle;

		private CombatData _data;

		private bool _configured;

		private MeshRenderer _carIndicatorMeshRenderer;

		private bool _isPlayerBuilt;

		private PlayerData _playerData;

		public bool refresh;

		public bool load;

		public string TextureName = "crosshair";

		private Texture _crosshairTexture;

		public bool TestIndicatorConfig;

		[Serializable]
		public class CustomConfig
		{
			public float DriftIndicatorRadius = 10f;

			public float DriftIndicatorThickness = 0.2f;

			public float IndicatorArrowOffset = 10.2f;

			[FormerlySerializedAs("AuraIndicatorSize")]
			public float PlayerBorderIndicatorSize = 1f;

			public float DriftIndicatorXDiff;

			public float DriftIndicatorZDiff;

			public float PlayerBorderOffsetPosition;
		}

		[Serializable]
		public class IndicatorRayConfig
		{
			public float Offset = 1f;

			public float RingSpeed = 10f;

			public float RingMinScale = 0.5f;

			public float RingRadius = 2.4f;

			public float Width = 1f;
		}
	}
}
