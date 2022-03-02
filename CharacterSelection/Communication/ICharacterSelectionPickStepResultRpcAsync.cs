using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionPickStepResultRpcAsync : IAsync
	{
		IFuture ReceiveResult(PickStepResultSerialized pickStepResult);
	}
}
