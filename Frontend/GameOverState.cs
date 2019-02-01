using System;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Frontend
{
	public class GameOverState : GameState
	{
		protected override void OnStateEnabled()
		{
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMatch(ClientBITags.RecapStart, false);
		}

		protected override void OnStateDisabled()
		{
			if (GameHubBehaviour.Hub.State.Last == this)
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClientMatch(ClientBITags.RecapEnd, false);
			}
		}

		public void BackToMain()
		{
			SceneManager.LoadScene("Void");
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
			base.GoToState(this.Menu, false);
		}

		public MainMenu Menu;
	}
}
