using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class PickupUnspawnCountBehaviourAsync : BaseRemoteStub<PickupUnspawnCountBehaviourAsync>, IPickupUnspawnCountBehaviourAsync, IAsync
	{
		public PickupUnspawnCountBehaviourAsync(int guid) : base(guid)
		{
		}

		public IFuture UpdatePickupInterfaceOnClient(int pickupsCounts)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1013, 3, new object[]
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
