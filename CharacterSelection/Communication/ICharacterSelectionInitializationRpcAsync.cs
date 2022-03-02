using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionInitializationRpcAsync : IAsync
	{
		IFuture SendInitializationData(InitializationDataSerialized dataSerialized);
	}
}
