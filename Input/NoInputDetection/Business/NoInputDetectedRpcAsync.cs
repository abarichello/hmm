using System;
using Pocketverse;

namespace HeavyMetalMachines.Input.NoInputDetection.Business
{
	public class NoInputDetectedRpcAsync : BaseRemoteStub<NoInputDetectedRpcAsync>, INoInputDetectedRpcAsync, IAsync
	{
		public NoInputDetectedRpcAsync(int guid) : base(guid)
		{
		}

		public IFuture ReceiveNoInputDetectedMessage(byte playerAddress)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1024, 3, new object[]
			{
				playerAddress
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
