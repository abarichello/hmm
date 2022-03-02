using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.GameStates.Game.HudGadgetCursor
{
	public class HudCursorController : GameHubBehaviour, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		private void Start()
		{
			Canvas[] componentsInParent = base.GetComponentsInParent<Canvas>();
			this._mainCanvasRectTransform = componentsInParent[componentsInParent.Length - 1].GetComponent<RectTransform>();
		}

		public void Update()
		{
			if (this._playerControllerDetected)
			{
				Vector3 vector = this._gameCameraEngine.UnityCamera.WorldToViewportPoint(this._playerController.MousePosition);
				Vector2 sizeDelta = this._mainCanvasRectTransform.sizeDelta;
				this._mainRectTransform.localPosition = new Vector2(vector.x * sizeDelta.x, vector.y * sizeDelta.y);
			}
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete playerBuildComplete)
		{
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData == null || GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance == null || playerBuildComplete.Id != GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.ObjId)
			{
				return;
			}
			this._playerController = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetComponent<PlayerController>();
			this._playerControllerDetected = true;
		}

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		[SerializeField]
		private RectTransform _mainRectTransform;

		private IPlayerController _playerController;

		private bool _playerControllerDetected;

		private RectTransform _mainCanvasRectTransform;
	}
}
