using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class AccountItemStatsWindow : GameHubBehaviour
	{
		public void CloseWindow()
		{
			base.gameObject.SetActive(false);
		}

		public UITexture previewTexture;

		public UILabel previewItemName;

		public UILabel previewItemName2;

		public UILabel previewItemDescription;

		public UILabel previewCharacterName;
	}
}
