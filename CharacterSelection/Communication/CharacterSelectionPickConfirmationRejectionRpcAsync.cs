using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionPickConfirmationRejectionRpcAsync : BaseRemoteStub<CharacterSelectionPickConfirmationRejectionRpcAsync>, ICharacterSelectionPickConfirmationRejectionRpcAsync, IAsync
	{
		public CharacterSelectionPickConfirmationRejectionRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture RejectionSent(PickConfirmationRejectionSerialized pickConfirmationRejection)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1060, 2, new object[]
			{
				pickConfirmationRejection
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
