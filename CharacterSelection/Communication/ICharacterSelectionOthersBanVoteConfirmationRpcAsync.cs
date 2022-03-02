using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionOthersBanVoteConfirmationRpcAsync : IAsync
	{
		IFuture BroadcastBanVoteConfirmation(ServerBanVoteSerializable client);
	}
}
