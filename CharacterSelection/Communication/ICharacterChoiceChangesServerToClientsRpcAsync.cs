using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterChoiceChangesServerToClientsRpcAsync : IAsync
	{
		IFuture CharacterChoiceReceived(CharacterChoiceSerialized characterChoiceSerialized);
	}
}
