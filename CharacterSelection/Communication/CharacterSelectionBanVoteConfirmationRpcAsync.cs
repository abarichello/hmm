using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionBanVoteConfirmationRpcAsync : BaseRemoteStub<CharacterSelectionBanVoteConfirmationRpcAsync>, ICharacterSelectionBanVoteConfirmationRpcAsync, IAsync
	{
		public CharacterSelectionBanVoteConfirmationRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture SendBanConfirmation(Guid characterId)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1052, 4, new object[]
			{
				characterId
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
