using System;
using HeavyMetalMachines.Frontend;
using Pocketverse;

namespace HeavyMetalMachines.ApplicationStates
{
	public class ProceedToReconnectState : IProceedToReconnectState
	{
		public ProceedToReconnectState(StateMachine stateMachine, ReconnectMenu reconnectState)
		{
			this._stateMachine = stateMachine;
			this._reconnectState = reconnectState;
		}

		public void Proceed()
		{
			this._stateMachine.GotoState(this._reconnectState, false);
		}

		private readonly StateMachine _stateMachine;

		private readonly ReconnectMenu _reconnectState;
	}
}
