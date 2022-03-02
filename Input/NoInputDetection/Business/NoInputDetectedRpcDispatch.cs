using System;
using Pocketverse;

namespace HeavyMetalMachines.Input.NoInputDetection.Business
{
	public class NoInputDetectedRpcDispatch : BaseRemoteStub<NoInputDetectedRpcDispatch>, INoInputDetectedRpcDispatch, IDispatch
	{
		public NoInputDetectedRpcDispatch(int guid) : base(guid)
		{
		}

		public void ReceiveNoInputDetectedMessage(byte playerAddress)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1024, 3, base.IsReliable, new object[]
			{
				playerAddress
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
