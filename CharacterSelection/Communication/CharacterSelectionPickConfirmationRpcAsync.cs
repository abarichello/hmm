using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionPickConfirmationRpcAsync : BaseRemoteStub<CharacterSelectionPickConfirmationRpcAsync>, ICharacterSelectionPickConfirmationRpcAsync, IAsync
	{
		public CharacterSelectionPickConfirmationRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture SendPickConfirmation(Guid characterId)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1061, 4, new object[]
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
