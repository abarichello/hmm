using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionResultRpcAsync : BaseRemoteStub<CharacterSelectionResultRpcAsync>, ICharacterSelectionResultRpcAsync, IAsync
	{
		public CharacterSelectionResultRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture ReceiveResult(CharacterSelectionResultSerialized result)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1063, 3, new object[]
			{
				result
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
