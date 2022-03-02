using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionOthersPickConfirmationRpcDispatch : IDispatch
	{
		void BroadcastPickConfirmation(PickConfirmationSerialized pickConfirmation);
	}
}
