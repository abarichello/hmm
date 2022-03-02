using System;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionStartRpcAsync : BaseRemoteStub<CharacterSelectionStartRpcAsync>, ICharacterSelectionStartRpcAsync, IAsync
	{
		public CharacterSelectionStartRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture SendStarted()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1064, 4, new object[0]);
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
