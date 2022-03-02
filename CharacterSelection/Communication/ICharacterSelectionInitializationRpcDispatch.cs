using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionInitializationRpcDispatch : IDispatch
	{
		void SendInitializationData(InitializationDataSerialized dataSerialized);
	}
}
