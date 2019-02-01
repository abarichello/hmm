using System;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class CharacterWorkshop : GameState
	{
		private void BackToPrevious()
		{
			base.GoToState(GameHubBehaviour.Hub.State.Last, false);
		}
	}
}
