using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionBanVoteConfirmationRpcDispatch : IDispatch
	{
		void SendBanConfirmation(Guid characterId);
	}
}
