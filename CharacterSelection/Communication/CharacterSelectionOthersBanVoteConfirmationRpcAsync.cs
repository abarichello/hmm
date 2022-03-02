using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionOthersBanVoteConfirmationRpcAsync : BaseRemoteStub<CharacterSelectionOthersBanVoteConfirmationRpcAsync>, ICharacterSelectionOthersBanVoteConfirmationRpcAsync, IAsync
	{
		public CharacterSelectionOthersBanVoteConfirmationRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture BroadcastBanVoteConfirmation(ServerBanVoteSerializable client)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1058, 4, new object[]
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
