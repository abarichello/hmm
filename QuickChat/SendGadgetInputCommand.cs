using System;
using HeavyMetalMachines.Combat.Gadget;

namespace HeavyMetalMachines.QuickChat
{
	public class SendGadgetInputCommand : ISendGadgetInputCommand
	{
		public SendGadgetInputCommand(MatchPlayers matchPlayers)
		{
			this._matchPlayers = matchPlayers;
		}

		public void Send(GadgetSlot slot)
		{
			PlayerController bitComponent = this._matchPlayers.CurrentPlayerData.CharacterInstance.GetBitComponent<PlayerController>();
			bitComponent.AddGadgetCommand(slot);
		}

		private readonly MatchPlayers _matchPlayers;
	}
}
