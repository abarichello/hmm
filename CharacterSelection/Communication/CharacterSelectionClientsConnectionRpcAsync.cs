using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionClientsConnectionRpcAsync : BaseRemoteStub<CharacterSelectionClientsConnectionRpcAsync>, ICharacterSelectionClientsConnectionRpcAsync, IAsync
	{
		public CharacterSelectionClientsConnectionRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture BroadcastClientDisconnectedRemote(MatchClientSerializable client)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1054, 5, new object[]
			{
				client
			});
			return future;
		}

		public IFuture BroadcastClientReconnectedRemote(MatchClientSerializable client)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1054, 6, new object[]
			{
				client
			});
			return future;
		}

		int IAsync.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IAsync.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}
	}
}
