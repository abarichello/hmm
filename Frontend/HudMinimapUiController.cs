using System;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudMinimapUiController : GameHubBehaviour, ICleanupListener
	{
		protected void Awake()
		{
			this.InternalSetVisibility(false, true);
			int quantity = GameHubBehaviour.Hub.Players.PlayersAndBots.Count;
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				quantity = 2;
			}
			ObjectPoolUtils.CreateObjectPool<HudMinimapPlayerObject>(this._minimapPlayerObjectReference, out this._hudMinimapPlayerObjects, quantity);
			for (int i = 0; i < this._hudMinimapPlayerObjects.Length; i++)
			{
				this._hudMinimapPlayerObjects[i].gameObject.SetActive(true);
			}
		}

		protected void Start()
		{
			GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex];
			if (string.IsNullOrEmpty(gameArenaInfo.MinimapTextureName))
			{
				base.gameObject.SetActive(false);
			}
			else
			{
				this.ApplyArenaConfig(gameArenaInfo);
				UICamera.onScreenResize += this.RecalculateMinimap;
				GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned += this.OnAllPlayersSpawned;
				GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned += this.OnAllPlayersSpawned;
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnListenToPhaseChange;
				GameHubBehaviour.Hub.BombManager.ListenToBombUnspawn += this.BombManagerOnListenToBombUnspawn;
				this._minimapBombObject.Setup();
				this._minimapDeliveryObject.Setup();
			}
		}

		private void BombManagerOnListenToBombUnspawn(PickupRemoveEvent pickupRemoveEvent)
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this._initialized = false;
			}
		}

		private void BombManagerOnListenToPhaseChange(BombScoreBoard.State state)
		{
			if (state == BombScoreBoard.State.EndGame)
			{
				if (this._initialized)
				{
					this.InternalSetVisibility(false, true);
				}
				this._initialized = false;
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			if (this._initialized)
			{
				this.InternalSetVisibility(false, true);
			}
			this._initialized = false;
		}

		private void RecalculateMinimap()
		{
			this._baseInfo.RecalculateBasePoints();
			if (this._initialized)
			{
				for (int i = 0; i < this._hudMinimapPlayerObjects.Length; i++)
				{
					this._hudMinimapPlayerObjects[i].CalculateMinimapProportions();
				}
				this._minimapBombObject.CalculateMinimapProportions();
				this._minimapDeliveryObject.CalculateMinimapProportions();
			}
		}

		public void SetVisibility(bool isVisible, bool immediate)
		{
			if (this._isVisible == isVisible)
			{
				return;
			}
			if (!this.CanChangeVisibility(isVisible))
			{
				return;
			}
			this.InternalSetVisibility(isVisible, immediate);
		}

		private void InternalSetVisibility(bool isVisible, bool imediate)
		{
			this._isVisible = isVisible;
			if (imediate || PauseController.Instance.IsGamePaused)
			{
				this._windowCanvasGroup.alpha = ((!isVisible) ? 0f : 1f);
				return;
			}
			GUIUtils.PlayAnimation(this._windowAnimation, !isVisible, (!isVisible) ? 2f : 1f, string.Empty);
		}

		private bool CanChangeVisibility(bool isVisible)
		{
			return !isVisible || !this._currentPlayerIsInDeadZone;
		}

		private void ApplyArenaConfig(GameArenaInfo arenaInfo)
		{
			TeamKind currentPlayerTeam = GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			RectTransform rectTransform = this._minimapBackgroundSprite.rectTransform;
			base.transform.localScale = Vector3.one * (float)arenaInfo.MinimapScale;
			this._minimapBackgroundSprite.TryToLoadAsset(arenaInfo.MinimapTextureName);
			Vector3 localPosition = rectTransform.localPosition;
			localPosition.y += (float)arenaInfo.MinimapTextureYOffset;
			localPosition.x += (float)arenaInfo.MinimapTextureXOffset;
			rectTransform.localPosition = localPosition;
			Vector2 sizeDelta = new Vector2(arenaInfo.MinimapTextureSize.x, arenaInfo.MinimapTextureSize.y);
			rectTransform.sizeDelta = sizeDelta;
			this._pivotGroupTransform.sizeDelta = sizeDelta;
			this._pivotGroupTransform.localPosition = localPosition;
			this._pivotGroupTransform.localPosition += new Vector3(arenaInfo.IconPositionOffset.x, arenaInfo.IconPositionOffset.y);
			if (!SpectatorController.IsSpectating && currentPlayerTeam == arenaInfo.ArenaFlipTeam && arenaInfo.ArenaFlipScale == -1)
			{
				Vector3 localScale = rectTransform.localScale;
				localScale.x *= (float)arenaInfo.ArenaFlipScale;
				rectTransform.localScale = localScale;
				Vector3 localPosition2 = rectTransform.localPosition;
				localPosition2.x -= rectTransform.sizeDelta.x;
				rectTransform.localPosition = localPosition2;
			}
			if (GameHubBehaviour.Hub.Match.ArenaIndex == 3 && arenaInfo.ArenaFlipRotation == 180)
			{
				Quaternion localRotation = this._pivotGroupTransform.localRotation;
				localRotation.z = (float)arenaInfo.ArenaFlipRotation;
				this._pivotGroupTransform.localRotation = localRotation;
				Vector3 localPosition3 = this._pivotGroupTransform.localPosition;
				localPosition3.x -= this._pivotGroupTransform.sizeDelta.x;
				this._pivotGroupTransform.localPosition = localPosition3;
			}
			this._hideWhenInDeadZone = arenaInfo.HideWhenInDeadZone;
			this._baseInfo.Setup(currentPlayerTeam, arenaInfo);
			this._angleY = (float)((currentPlayerTeam != TeamKind.Blue) ? arenaInfo.TeamRedAngleY : arenaInfo.TeamBlueAngleY);
			Quaternion localRotation2 = Quaternion.Euler(new Vector3(0f, 0f, this._angleY));
			this.MinimapObjectsHubTransform.localRotation = localRotation2;
			Shader.SetGlobalFloat("_MinimapAngle", 0.0174532924f * this._angleY);
			if (currentPlayerTeam == arenaInfo.ArenaFlipTeam)
			{
				Shader.DisableKeyword("NORMAL_MODE");
				Shader.EnableKeyword("INVERSE_MODE");
			}
			else
			{
				Shader.EnableKeyword("NORMAL_MODE");
				Shader.DisableKeyword("INVERSE_MODE");
			}
			this._deliveryStaticBaseCanvasGroup.gameObject.SetActive(GameHubBehaviour.Hub.Match.ArenaIndex != 1 && GameHubBehaviour.Hub.Match.ArenaIndex != 3);
		}

		protected void OnDestroy()
		{
			UICamera.onScreenResize -= this.RecalculateMinimap;
			GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned -= this.OnAllPlayersSpawned;
			GameHubBehaviour.Hub.Events.Bots.ListenToAllPlayersSpawned -= this.OnAllPlayersSpawned;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnListenToPhaseChange;
			GameHubBehaviour.Hub.BombManager.ListenToBombUnspawn -= this.BombManagerOnListenToBombUnspawn;
			this._initialized = false;
		}

		private void OnAllPlayersSpawned()
		{
			if (!GameHubBehaviour.Hub.Events.Bots.CarCreationFinished || !GameHubBehaviour.Hub.Events.Players.CarCreationFinished)
			{
				return;
			}
			GameArenaInfo arenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex];
			TeamKind currentPlayerTeam = GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			List<PlayerData> playersAndBots = GameHubBehaviour.Hub.Players.PlayersAndBots;
			int num = 0;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				PlayerData playerData = playersAndBots[i];
				if (!playerData.IsCurrentPlayer && playerData.Team == currentPlayerTeam && !playerData.IsNarrator)
				{
					this._hudMinimapPlayerObjects[num++].Setup(playerData, arenaInfo, this.PlayerGuiAssets);
				}
			}
			for (int j = 0; j < playersAndBots.Count; j++)
			{
				PlayerData playerData2 = playersAndBots[j];
				if (playerData2.Team != currentPlayerTeam && !playerData2.IsNarrator)
				{
					this._hudMinimapPlayerObjects[num++].Setup(playerData2, arenaInfo, this.PlayerGuiAssets);
				}
			}
			HudMinimapPlayerObject hudMinimapPlayerObject = null;
			if (num < this._hudMinimapPlayerObjects.Length && GameHubBehaviour.Hub.Players.CurrentPlayerData != null && !GameHubBehaviour.Hub.Players.CurrentPlayerData.IsNarrator)
			{
				hudMinimapPlayerObject = this._hudMinimapPlayerObjects[num++];
				hudMinimapPlayerObject.Setup(GameHubBehaviour.Hub.Players.CurrentPlayerData, arenaInfo, this.PlayerGuiAssets);
			}
			this._minimapBombObject.CalculateMinimapProportions();
			this._minimapDeliveryObject.CalculateMinimapProportions();
			this._baseInfo.RecalculateBasePoints();
			this._currentPlayerIsInDeadZone = false;
			if (this._hideWhenInDeadZone && hudMinimapPlayerObject != null)
			{
				this._currentPlayerIsInDeadZone = this._baseInfo.IsInsideTeamArea(GameHubBehaviour.Hub.Players.CurrentPlayerTeam, hudMinimapPlayerObject.GetGuiObjectPosition());
				hudMinimapPlayerObject.TryToSetVisibility(!this._currentPlayerIsInDeadZone);
			}
			if (this._currentPlayerIsInDeadZone && this._isVisible)
			{
				this.InternalSetVisibility(false, true);
			}
			this._initialized = true;
		}

		protected void LateUpdate()
		{
			if (!this._initialized)
			{
				return;
			}
			if (!this._hideWhenInDeadZone && !this._isVisible)
			{
				return;
			}
			for (int i = 0; i < this._hudMinimapPlayerObjects.Length; i++)
			{
				this._hudMinimapPlayerObjects[i].OnUpdate();
			}
			if (this._hideWhenInDeadZone)
			{
				this.DeadZoneCheckUpdate();
			}
			if (this._isVisible)
			{
				this._minimapBombObject.OnUpdate();
				this._minimapDeliveryObject.OnUpdate();
				this._deliveryStaticBaseCanvasGroup.alpha = ((!GameHubBehaviour.Hub.BombManager.ScoreBoard.IsInOvertime) ? 1f : 0f);
			}
		}

		private void DeadZoneCheckUpdate()
		{
			if (Time.frameCount < this._deadZoneTestFrameCount + 30)
			{
				return;
			}
			HudMinimapPlayerObject hudMinimapPlayerObject = this._hudMinimapPlayerObjects[this._deadZoneCheckListCount];
			bool flag = this._baseInfo.IsInsideTeamArea(hudMinimapPlayerObject.PlayerTeamKind, hudMinimapPlayerObject.GetGuiObjectPosition());
			hudMinimapPlayerObject.TryToSetVisibility(!flag);
			if (hudMinimapPlayerObject.IsCurrentPlayer && flag != this._currentPlayerIsInDeadZone)
			{
				this._currentPlayerIsInDeadZone = flag;
				this.SetVisibility(!this._currentPlayerIsInDeadZone, true);
			}
			this._deadZoneCheckListCount++;
			if (this._deadZoneCheckListCount >= this._hudMinimapPlayerObjects.Length)
			{
				this._deadZoneCheckListCount = 0;
				this._deadZoneTestFrameCount = Time.frameCount;
			}
		}

		public HudMinimapUiController.HudMinimapPlayerGuiAssets PlayerGuiAssets;

		[SerializeField]
		private Animation _windowAnimation;

		[SerializeField]
		private CanvasGroup _windowCanvasGroup;

		[SerializeField]
		private HmmUiImage _minimapBackgroundSprite;

		[SerializeField]
		private RectTransform _pivotGroupTransform;

		[SerializeField]
		private HudMinimapPlayerObject _minimapPlayerObjectReference;

		[SerializeField]
		private HudMinimapBombObject _minimapBombObject;

		[SerializeField]
		private HudMinimapDeliveryObject _minimapDeliveryObject;

		[SerializeField]
		private CanvasGroup _deliveryStaticBaseCanvasGroup;

		[SerializeField]
		protected RectTransform MinimapObjectsHubTransform;

		private HudMinimapPlayerObject[] _hudMinimapPlayerObjects;

		private const int DeadZoneCheckFrameSkip = 30;

		private bool _hideWhenInDeadZone;

		private int _deadZoneTestFrameCount;

		private int _deadZoneCheckListCount;

		private bool _currentPlayerIsInDeadZone;

		private float _angleY;

		private Rotation2D _rotator2D;

		[Header("[Bases and dead zones]")]
		[SerializeField]
		private HudMinimapBaseInfo _baseInfo;

		private bool _isVisible;

		private bool _initialized;

		[Serializable]
		public struct HudMinimapPlayerGuiAssets
		{
			public Sprite BorderCurrentPlayerSprite;

			public Sprite BorderAllyPlayerSprite;

			public Sprite BorderEnemyPlayerSprite;

			public Sprite ArrowCurrentPlayerSprite;

			public Sprite ArrowAllyPlayerSprite;

			public Sprite ArrowEnemyPlayerSprite;
		}
	}
}
