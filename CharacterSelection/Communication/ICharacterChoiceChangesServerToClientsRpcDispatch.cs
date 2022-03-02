using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterChoiceChangesServerToClientsRpcDispatch : IDispatch
	{
		void CharacterChoiceReceived(CharacterChoiceSerialized characterChoiceSerialized);
	}
}
