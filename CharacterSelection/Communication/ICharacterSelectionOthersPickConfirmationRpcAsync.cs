using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionOthersPickConfirmationRpcAsync : IAsync
	{
		IFuture BroadcastPickConfirmation(PickConfirmationSerialized pickConfirmation);
	}
}
