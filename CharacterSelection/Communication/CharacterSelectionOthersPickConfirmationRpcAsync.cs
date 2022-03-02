using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionOthersPickConfirmationRpcAsync : BaseRemoteStub<CharacterSelectionOthersPickConfirmationRpcAsync>, ICharacterSelectionOthersPickConfirmationRpcAsync, IAsync
	{
		public CharacterSelectionOthersPickConfirmationRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture BroadcastPickConfirmation(PickConfirmationSerialized pickConfirmation)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1059, 4, new object[]
			{
				pickConfirmation
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
