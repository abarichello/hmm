using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionResultRpcDispatch : IDispatch
	{
		void ReceiveResult(CharacterSelectionResultSerialized result);
	}
}
