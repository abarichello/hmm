using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class UnspawnCountBehaviourAsync : BaseRemoteStub<UnspawnCountBehaviourAsync>, IUnspawnCountBehaviourAsync, IAsync
	{
		public UnspawnCountBehaviourAsync(int guid) : base(guid)
		{
		}

		public IFuture UpdateInterfaceOnClient(int pickupsCounts)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1016, 4, new object[]
			{
				pickupsCounts
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
