using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PauseControllerDispatch : BaseRemoteStub<PauseControllerDispatch>, IPauseControllerDispatch, IDispatch
	{
		public PauseControllerDispatch(int guid) : base(guid)
		{
		}

		public void TogglePauseServer()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1028, 7, base.IsReliable, new object[0]);
		}

		public void TriggerPauseNotification(int kind, float delay)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1028, 16, base.IsReliable, new object[]
			{
				kind,
				delay
			});
		}

		public void ChangePauseStateOnClient(int newState, long playerId, int timeRemaining, int timeoutMillis, int activations)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1028, 20, base.IsReliable, new object[]
			{
				newState,
				playerId,
				timeRemaining,
				timeoutMillis,
				activations
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
