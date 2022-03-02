using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionBanStepResultRpcDispatch : IDispatch
	{
		void ReceiveResult(BanStepResultSerializable banStepResult);
	}
}
