using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionInitializationRpcAsync : BaseRemoteStub<CharacterSelectionInitializationRpcAsync>, ICharacterSelectionInitializationRpcAsync, IAsync
	{
		public CharacterSelectionInitializationRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture SendInitializationData(InitializationDataSerialized dataSerialized)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1057, 3, new object[]
			{
				dataSerialized
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
