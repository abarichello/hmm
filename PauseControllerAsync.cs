using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PauseControllerAsync : BaseRemoteStub<PauseControllerAsync>, IPauseControllerAsync, IAsync
	{
		public PauseControllerAsync(int guid) : base(guid)
		{
		}

		public IFuture TogglePauseServer()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1027, 4, new object[0]);
			return future;
		}

		public IFuture TriggerPauseNotification(int kind, float delay)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1027, 10, new object[]
			{
				kind,
				delay
			});
			return future;
		}

		public IFuture ChangePauseStateOnClient(int newState, long playerId)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1027, 14, new object[]
			{
				newState,
				playerId
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
