using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionPickStepResultRpcDispatch : IDispatch
	{
		void ReceiveResult(PickStepResultSerialized pickStepResult);
	}
}
