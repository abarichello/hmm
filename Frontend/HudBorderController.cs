using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudBorderController : GameHubBehaviour
	{
		public void SetBorderForFullScreen()
		{
			this.BorderPanel.SetActive(false);
			this.FullScreenSettingsButton.SetActive(true);
			this.FullScreenDisconnectButton.SetActive(true);
		}

		public void SetBorderForWindowed()
		{
			this.BorderPanel.SetActive(true);
			this.FullScreenSettingsButton.SetActive(false);
			this.FullScreenDisconnectButton.SetActive(false);
		}

		public GameObject BorderPanel;

		public GameObject FullScreenSettingsButton;

		public GameObject FullScreenDisconnectButton;
	}
}
