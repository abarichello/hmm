using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudMinimapDeliveryObject : HudMinimapObject
	{
		public override void Setup()
		{
			base.Setup(false);
			this._arenaScaleModifier = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex].OvertimeGuiDeliveryScaleModifier;
			this.SetVisibility(GameHubBehaviour.Hub.BombManager.ScoreBoard.IsInOvertime);
			GameHubBehaviour.Hub.BombManager.ScoreController.OnBombTriggersReady += this.ScoreControllerOnBombTriggersReady;
		}

		private void SetVisibility(bool isVisible)
		{
			this._mainCanvasGroup.alpha = ((!isVisible) ? 0f : 1f);
		}

		protected void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ScoreController.OnBombTriggersReady -= this.ScoreControllerOnBombTriggersReady;
			this._targetAllyDeliveryPointTransform = null;
			this._targetEnemyDeliveryPointTransform = null;
		}

		private void ScoreControllerOnBombTriggersReady()
		{
			TeamKind currentPlayerTeam = GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			BombTargetTrigger bombTargetTrigger = GameHubBehaviour.Hub.BombManager.ScoreController.GetBombTargetTrigger(currentPlayerTeam);
			BombTargetTrigger bombTargetTrigger2 = GameHubBehaviour.Hub.BombManager.ScoreController.GetBombTargetTrigger((currentPlayerTeam != TeamKind.Blue) ? TeamKind.Blue : TeamKind.Red);
			this._targetAllyDeliveryPointTransform = bombTargetTrigger.transform;
			this._targetEnemyDeliveryPointTransform = bombTargetTrigger2.transform;
			this._initialTargetAllyDeliveryPointScale = this._targetAllyDeliveryPointTransform.localScale.x;
			this._initialTargetEnemyDeliveryPointScale = this._targetEnemyDeliveryPointTransform.localScale.x;
		}

		public override void OnUpdate()
		{
			bool isInOvertime = GameHubBehaviour.Hub.BombManager.ScoreBoard.IsInOvertime;
			this.SetVisibility(isInOvertime);
			if (isInOvertime)
			{
				float normalizedScale = this._targetAllyDeliveryPointTransform.localScale.x / this._initialTargetAllyDeliveryPointScale;
				this.UpdateDeliveryPoint(this._allyStaticDeliveryPoint, this._targetAllyDeliveryPointTransform, normalizedScale);
				this.UpdateDeliveryPoint(this._enemyStaticDeliveryPoint, this._targetEnemyDeliveryPointTransform, normalizedScale);
			}
		}

		private void UpdateDeliveryPoint(RectTransform guiDeliveryPoint, Transform targetDeliveryPointTransform, float normalizedScale)
		{
			base.UpdatePosition(guiDeliveryPoint, targetDeliveryPointTransform.position);
			base.UpdateRotation(guiDeliveryPoint, targetDeliveryPointTransform.rotation);
			normalizedScale *= this._arenaScaleModifier;
			guiDeliveryPoint.localScale = new Vector3(normalizedScale, 1f, 1f);
		}

		[SerializeField]
		private RectTransform _allyStaticDeliveryPoint;

		[SerializeField]
		private RectTransform _enemyStaticDeliveryPoint;

		[SerializeField]
		private CanvasGroup _mainCanvasGroup;

		private Transform _targetAllyDeliveryPointTransform;

		private Transform _targetEnemyDeliveryPointTransform;

		private float _initialTargetAllyDeliveryPointScale;

		private float _initialTargetEnemyDeliveryPointScale;

		[Range(0f, 1f)]
		[SerializeField]
		[Tooltip("Read Only Serialization. The Prefab Value Has No In-Game Effect.")]
		private float _arenaScaleModifier = 1f;
	}
}
