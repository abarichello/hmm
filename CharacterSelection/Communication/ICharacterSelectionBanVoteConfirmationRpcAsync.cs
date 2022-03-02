using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionBanVoteConfirmationRpcAsync : IAsync
	{
		IFuture SendBanConfirmation(Guid characterId);
	}
}
