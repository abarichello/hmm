using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionResultRpcAsync : IAsync
	{
		IFuture ReceiveResult(CharacterSelectionResultSerialized result);
	}
}
