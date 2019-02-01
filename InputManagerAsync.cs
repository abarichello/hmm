using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class InputManagerAsync : BaseRemoteStub<InputManagerAsync>, IInputManagerAsync, IAsync
	{
		public InputManagerAsync(int guid) : base(guid)
		{
		}

		public IFuture ClientSendInput(PlayerController.InputMap inputs)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1019, 3, new object[]
			{
				inputs
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
