using System;
using HeavyMetalMachines.Frontend;
using Pocketverse;

namespace HeavyMetalMachines.ApplicationStates
{
	public class ProceedToCreateProfileState : IProceedToCreateProfileState
	{
		public ProceedToCreateProfileState(StateMachine stateMachine, Profile profileState)
		{
			this._stateMachine = stateMachine;
			this._profileState = profileState;
		}

		public void Proceed()
		{
			this._stateMachine.GotoState(this._profileState, false);
		}

		private readonly StateMachine _stateMachine;

		private readonly Profile _profileState;
	}
}
