using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class UnityUiController : GameHubBehaviour
	{
		public void Start()
		{
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				Object.Destroy(base.gameObject);
				return;
			}
			Camera mainCamera = UICamera.mainCamera;
			this.SetupCanvas(mainCamera, this._constantPixelSizeCanvas);
			this.SetupCanvas(mainCamera, this._scaledScreenSizeCanvas);
			GameHubBehaviour.Hub.State.ListenToPreStateChanged += this.ListenToPreStateChanged;
			UIProgressionController.OnProgressionControllerVisibilityChange += this.ListenToOpenEndMatchScreen;
		}

		private void ListenToOpenEndMatchScreen(bool visibility)
		{
			if (visibility)
			{
				Object.Destroy(base.gameObject);
			}
		}

		private void SetupCanvas(Camera camera, Canvas canvas)
		{
			canvas.worldCamera = camera;
			canvas.planeDistance = camera.farClipPlane - 1f;
		}

		private void ListenToPreStateChanged(GameState nextGameState)
		{
			if (nextGameState is Game)
			{
				return;
			}
			Object.Destroy(base.gameObject);
		}

		public void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				GameHubBehaviour.Hub.State.ListenToPreStateChanged -= this.ListenToPreStateChanged;
				UIProgressionController.OnProgressionControllerVisibilityChange -= this.ListenToOpenEndMatchScreen;
			}
		}

		[SerializeField]
		private Canvas _constantPixelSizeCanvas;

		[SerializeField]
		private Canvas _scaledScreenSizeCanvas;
	}
}
