using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public interface ICharacterSelectionClientsConnectionRpcDispatch : IDispatch
	{
		void BroadcastClientDisconnectedRemote(MatchClientSerializable client);

		void BroadcastClientReconnectedRemote(MatchClientSerializable client);
	}
}
