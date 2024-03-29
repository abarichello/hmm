﻿using System;
using System.Collections.Generic;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class HudMinimapPlayerObject : HudMinimapObject
	{
		public override void OnUpdate()
		{
			base.UpdatePosition(this.TargetTransform.position);
			this.UpdateArrowRotation();
			this.UpdateBombBorder();
		}

		private void UpdateArrowRotation()
		{
			Vector3 forward = this.TargetTransform.forward;
			Vector2 vector;
			vector..ctor(forward.x, forward.z);
			Vector3 vector2 = Quaternion.AngleAxis((float)this._arrowRotationOffset, Vector3.forward) * vector;
			this._arrowRotatorGroupTransform.up = -vector2;
		}

		public void Setup(PlayerData playerData, IGameArenaInfo arenaInfo, HudMinimapUiController.HudMinimapPlayerGuiAssets playerGuiAssets)
		{
			this.Setup();
			this._playerCarId = playerData.PlayerCarId;
			this.PlayerTeamKind = playerData.Team;
			this.IsCurrentPlayer = playerData.IsCurrentPlayer;
			this._iconImage.TryToLoadAsset(HudUtils.GetPlayerPixelArtIconName(GameHubBehaviour.Hub, playerData.Character.CharacterItemTypeGuid));
			this.TargetTransform = playerData.CharacterInstance.transform;
			TeamKind currentPlayerTeam = GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			this._arrowRotationOffset = ((currentPlayerTeam != TeamKind.Blue) ? arenaInfo.TeamRedArrowRotation : arenaInfo.TeamBlueArrowRotation);
			this.UpdateBombBorder();
			if (currentPlayerTeam == playerData.Team)
			{
				if (playerData.IsCurrentPlayer)
				{
					this._borderImage.sprite = playerGuiAssets.BorderCurrentPlayerSprite;
					this._arrowImage.sprite = playerGuiAssets.ArrowCurrentPlayerSprite;
					base.transform.localScale = new Vector3(this._currentPlayerScale, this._currentPlayerScale, this._currentPlayerScale);
				}
				else
				{
					this._borderImage.sprite = playerGuiAssets.BorderAllyPlayerSprite;
					this._arrowImage.sprite = playerGuiAssets.ArrowAllyPlayerSprite;
				}
			}
			else
			{
				this._borderImage.sprite = playerGuiAssets.BorderEnemyPlayerSprite;
				this._arrowImage.sprite = playerGuiAssets.ArrowEnemyPlayerSprite;
			}
			this._combatObject = playerData.CharacterInstance.GetComponent<CombatObject>();
			this._combatObjectId = this._combatObject.GetInstanceID();
			this._combatObject.ListenToObjectSpawn += this.OnCombatObjecSpawn;
			this._combatObject.ListenToObjectUnspawn += this.OnCombatObjecUnspawn;
			this._isAlive = this._combatObject.IsAlive();
			this.SetVisibility(this._isAlive);
			this.OnUpdate();
		}

		private void UpdateBombBorder()
		{
			List<int> bombCarriersIds = GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds;
			bool flag = bombCarriersIds.Count > 0 && bombCarriersIds[0] == this._playerCarId;
			this._grabBombBorderCanvasGroup.alpha = ((!flag) ? 0f : 1f);
		}

		public Vector3 GetGuiObjectPosition()
		{
			return base.transform.position;
		}

		public void TryToSetVisibility(bool isVisible)
		{
			this.SetVisibility(isVisible && this._isAlive);
		}

		public void SetVisibility(bool isVisible)
		{
			this._mainCanvasGroup.alpha = ((!isVisible) ? 0f : 1f);
		}

		private void OnCombatObjecSpawn(CombatObject combatObject, SpawnEvent msg)
		{
			this.SetVisibility(this._isAlive = true);
		}

		private void OnCombatObjecUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			this.SetVisibility(this._isAlive = false);
		}

		protected void OnDestroy()
		{
			if (this._combatObjectId != -1)
			{
				this._combatObject.ListenToObjectSpawn -= this.OnCombatObjecSpawn;
				this._combatObject.ListenToObjectUnspawn -= this.OnCombatObjecUnspawn;
			}
			this._combatObject = null;
		}

		[SerializeField]
		private Canvas _mainCanvas;

		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		[SerializeField]
		private int _carrierBombSortingOrder;

		[SerializeField]
		private float _currentPlayerScale = 1.2f;

		[SerializeField]
		private HmmUiImage _iconImage;

		[SerializeField]
		private CanvasGroup _grabBombBorderCanvasGroup;

		[SerializeField]
		private Image _borderImage;

		[SerializeField]
		private Image _arrowImage;

		[SerializeField]
		private RectTransform _arrowRotatorGroupTransform;

		[NonSerialized]
		public TeamKind PlayerTeamKind;

		[NonSerialized]
		public bool IsCurrentPlayer;

		private int _arrowRotationOffset;

		private int _playerCarId;

		private CombatObject _combatObject;

		private int _combatObjectId = -1;

		private bool _isAlive;
	}
}
