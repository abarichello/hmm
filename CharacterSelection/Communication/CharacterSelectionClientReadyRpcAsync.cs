using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionClientReadyRpcAsync : BaseRemoteStub<CharacterSelectionClientReadyRpcAsync>, ICharacterSelectionClientReadyRpcAsync, IAsync
	{
		public CharacterSelectionClientReadyRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture SendReady()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1053, 4, new object[0]);
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
