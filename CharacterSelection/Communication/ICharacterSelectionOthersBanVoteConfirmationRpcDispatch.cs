using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionOthersBanVoteConfirmationRpcDispatch : IDispatch
	{
		void BroadcastBanVoteConfirmation(ServerBanVoteSerializable client);
	}
}
