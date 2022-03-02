using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionClientsConnectionRpcAsync : IAsync
	{
		IFuture BroadcastClientDisconnectedRemote(MatchClientSerializable client);

		IFuture BroadcastClientReconnectedRemote(MatchClientSerializable client);
	}
}
