using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionPickConfirmationRpcDispatch : IDispatch
	{
		void SendPickConfirmation(Guid characterId);
	}
}
