using System;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionPickStepResultRpcAsync : BaseRemoteStub<CharacterSelectionPickStepResultRpcAsync>, ICharacterSelectionPickStepResultRpcAsync, IAsync
	{
		public CharacterSelectionPickStepResultRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture ReceiveResult(PickStepResultSerialized pickStepResult)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1062, 3, new object[]
			{
				pickStepResult
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
