using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionClientsConnectionRpcDispatch : BaseRemoteStub<CharacterSelectionClientsConnectionRpcDispatch>, ICharacterSelectionClientsConnectionRpcDispatch, IDispatch
	{
		public CharacterSelectionClientsConnectionRpcDispatch(int guid) : base(guid)
		{
		}

		public void BroadcastClientDisconnectedRemote(MatchClientSerializable client)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1054, 5, base.IsReliable, new object[]
			{
				client
			});
		}

		public void BroadcastClientReconnectedRemote(MatchClientSerializable client)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1054, 6, base.IsReliable, new object[]
			{
				client
			});
		}

		int IDispatch.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IDispatch.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}

		bool IDispatch.get_IsReliable()
		{
			return base.IsReliable;
		}

		void IDispatch.set_IsReliable(bool value)
		{
			base.IsReliable = value;
		}
	}
}
