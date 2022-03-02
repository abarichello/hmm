using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterChoiceChangesClientToServerRpcAsync : IAsync
	{
		IFuture CharacterChoiceReceived(Guid characterId);
	}
}
