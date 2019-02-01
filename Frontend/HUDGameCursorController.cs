using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HUDGameCursorController : GameHubBehaviour, PlayerController.IControllerCursor
	{
		private void Awake()
		{
			this._visible = false;
			this.CursorImage.gameObject.SetActive(false);
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback += this.CurrentPlayerCreatedCallback;
		}

		private void CurrentPlayerCreatedCallback(PlayerEvent playerEvent)
		{
			this._playerController = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<PlayerController>();
			this._playerController.ControllerCursor = this;
		}

		private void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback -= this.CurrentPlayerCreatedCallback;
			if (this._playerController != null && this._playerController.ControllerCursor == this)
			{
				this._playerController.ControllerCursor = null;
			}
			this._playerController = null;
		}

		public bool Visible
		{
			get
			{
				return this._visible;
			}
			set
			{
				if (value != this._visible)
				{
					this.CursorImage.gameObject.SetActive(value);
				}
				this._visible = value;
			}
		}

		public Vector3 CursorPos { get; set; }

		private void LateUpdate()
		{
			if (!this.Visible)
			{
				return;
			}
			this.CursorImage.localPosition = CarCamera.Singleton.Camera.WorldToScreenPoint(this.CursorPos);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HUDGameCursorController));

		public RectTransform CursorImage;

		private PlayerController _playerController;

		private bool _visible;
	}
}
