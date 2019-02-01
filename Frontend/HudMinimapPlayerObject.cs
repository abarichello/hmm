using System;
using System.Collections.Generic;
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
			Quaternion localRotation = this._arrowRotatorGroupTransform.localRotation;
			Vector3 eulerAngles = localRotation.eulerAngles;
			float num = -this.TargetTransform.localRotation.eulerAngles.y;
			eulerAngles.Set(0f, 0f, num + (float)this._arrowRotationOffset);
			localRotation.eulerAngles = eulerAngles;
			this._arrowRotatorGroupTransform.localRotation = localRotation;
		}

		public void Setup(PlayerData playerData, GameArenaInfo arenaInfo, HudMinimapUiController.HudMinimapPlayerGuiAssets playerGuiAssets)
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
			if (GameHubBehaviour.Hub.Match.ArenaIndex == 3 && arenaInfo.ArenaFlipRotation == 180)
			{
				Quaternion localRotation = this._iconImage.rectTransform.localRotation;
				localRotation.z = (float)arenaInfo.ArenaFlipRotation;
				this._iconImage.rectTransform.localRotation = localRotation;
			}
			this.SetVisibility(this._isAlive);
			this.OnUpdate();
		}

		private void UpdateBombBorder()
		{
			List<int> bombCarriersIds = GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds;
			bool flag = bombCarriersIds.Count > 0 && bombCarriersIds[0] == this._playerCarId;
			this._grabBombBorderCanvasGroup.alpha = ((!flag) ? 0f : 1f);
			if (flag)
			{
				if (!this._mainCanvas.overrideSorting)
				{
					this._mainCanvas.overrideSorting = true;
					this._mainCanvas.sortingOrder = this._carrierBombSortingOrder;
				}
			}
			else if (this._mainCanvas.overrideSorting)
			{
				this._mainCanvas.overrideSorting = false;
			}
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
