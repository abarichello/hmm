using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterChoiceChangesClientToServerRpcDispatch : IDispatch
	{
		void CharacterChoiceReceived(Guid characterId);
	}
}
