using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class UnspawnCountBehaviourDispatch : BaseRemoteStub<UnspawnCountBehaviourDispatch>, IUnspawnCountBehaviourDispatch, IDispatch
	{
		public UnspawnCountBehaviourDispatch(int guid) : base(guid)
		{
		}

		public void UpdateInterfaceOnClient(int pickupsCounts)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1015, 4, base.IsReliable, new object[]
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
