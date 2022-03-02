using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionPickConfirmationRpcAsync : IAsync
	{
		IFuture SendPickConfirmation(Guid characterId);
	}
}
