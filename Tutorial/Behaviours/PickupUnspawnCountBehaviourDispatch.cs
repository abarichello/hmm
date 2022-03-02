using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class PickupUnspawnCountBehaviourDispatch : BaseRemoteStub<PickupUnspawnCountBehaviourDispatch>, IPickupUnspawnCountBehaviourDispatch, IDispatch
	{
		public PickupUnspawnCountBehaviourDispatch(int guid) : base(guid)
		{
		}

		public void UpdatePickupInterfaceOnClient(int pickupsCounts)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1014, 3, base.IsReliable, new object[]
			{
				pickupsCounts
			});
		}

		int IDispatch.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IDispatch.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}

		bool IDispatch.get_IsReliable()
		{
			return base.IsReliable;
		}

		void IDispatch.set_IsReliable(bool value)
		{
			base.IsReliable = value;
		}
	}
}
