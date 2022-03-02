using System;
using HeavyMetalMachines.Frontend;
using Pocketverse;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class MainMenuGuiProviderFromHub : GameHubObject, IMainMenuGuiProvider
	{
		public bool TryToGetMainMenuGui(out MainMenuGui mainMenuGui)
		{
			MainMenu mainMenu;
			if (!this.TryToGetMainMenuGameState(out mainMenu))
			{
				mainMenuGui = null;
				return false;
			}
			mainMenuGui = mainMenu.GetStateGuiController<MainMenuGui>();
			return mainMenuGui != null;
		}

		private bool TryToGetMainMenuGameState(out MainMenu gameState)
		{
			gameState = (GameHubObject.Hub.State.Current as MainMenu);
			return gameState != null;
		}
	}
}
