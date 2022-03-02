using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class CurrentPlayerControllerWrapper : GameHubObject, ICurrentPlayerController
	{
		public void AddGadgetCommand(GadgetSlot slot)
		{
			PlayerController bitComponent = GameHubObject.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<PlayerController>();
			bitComponent.AddGadgetCommand(slot);
		}
	}
}
