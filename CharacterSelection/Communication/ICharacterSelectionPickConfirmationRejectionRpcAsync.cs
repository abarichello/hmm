using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionPickConfirmationRejectionRpcAsync : IAsync
	{
		IFuture RejectionSent(PickConfirmationRejectionSerialized pickConfirmationRejection);
	}
}
