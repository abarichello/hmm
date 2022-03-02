using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionPickConfirmationRejectionRpcDispatch : IDispatch
	{
		void RejectionSent(PickConfirmationRejectionSerialized pickConfirmationRejection);
	}
}
