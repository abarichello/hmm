using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionBanStepResultRpcAsync : BaseRemoteStub<CharacterSelectionBanStepResultRpcAsync>, ICharacterSelectionBanStepResultRpcAsync, IAsync
	{
		public CharacterSelectionBanStepResultRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture ReceiveResult(BanStepResultSerializable banStepResult)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 1, new object[]
			{
				banStepResult
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
