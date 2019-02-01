using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudMinimapBombObject : HudMinimapObject
	{
		public override void Setup()
		{
			base.Setup();
			GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex];
			this._carriedBombOffset = gameArenaInfo.MinimapCarriedBombOffset;
		}

		public override void OnUpdate()
		{
			if (GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned)
			{
				if (this._mainCanvasGroup.alpha < 0.001f)
				{
					this._mainCanvasGroup.alpha = 1f;
				}
				Vector3 vector = GameHubBehaviour.Hub.BombManager.BombMovement.transform.position;
				bool flag = GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds.Count > 0;
				if (flag)
				{
					int id = GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds[0];
					PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(id);
					Vector3 position = playerOrBotsByObjectId.CharacterInstance.transform.position;
					position.y = 0f;
					vector.y = 0f;
					Vector3 vector2 = position + (vector - position).normalized * this._carriedBombOffset;
					if (this._positionLerpCounter < 0.999f)
					{
						vector = Vector3.Lerp(vector, vector2, this._positionLerpCounter);
						this._positionLerpCounter += Time.deltaTime;
						this._positionLerpCounter = Mathf.Clamp01(this._positionLerpCounter);
					}
					else
					{
						vector = vector2;
					}
					this._lastCarriedBombGuiPosition = vector;
				}
				else
				{
					vector = Vector3.Lerp(vector, this._lastCarriedBombGuiPosition, this._positionLerpCounter);
					this._positionLerpCounter -= Time.deltaTime;
					this._positionLerpCounter = Mathf.Clamp01(this._positionLerpCounter);
				}
				base.UpdatePosition(vector);
			}
			else
			{
				if (this._mainCanvasGroup.alpha > 0.999f)
				{
					this._mainCanvasGroup.alpha = 0f;
					base.UpdatePosition(Vector3.zero);
				}
				this._positionLerpCounter = 0f;
				this._lastCarriedBombGuiPosition = Vector3.zero;
			}
		}

		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		private float _carriedBombOffset;

		private float _positionLerpCounter;

		private Vector3 _lastCarriedBombGuiPosition;
	}
}
