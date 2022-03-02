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
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1028, 7, new object[0]);
			return future;
		}

		public IFuture TriggerPauseNotification(int kind, float delay)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1028, 16, new object[]
			{
				kind,
				delay
			});
			return future;
		}

		public IFuture ChangePauseStateOnClient(int newState, long playerId, int timeRemaining, int timeoutMillis, int activations)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1028, 20, new object[]
			{
				newState,
				playerId,
				timeRemaining,
				timeoutMillis,
				activations
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
