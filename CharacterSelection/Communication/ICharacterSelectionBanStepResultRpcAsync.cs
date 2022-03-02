using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionBanStepResultRpcAsync : IAsync
	{
		IFuture ReceiveResult(BanStepResultSerializable banStepResult);
	}
}
