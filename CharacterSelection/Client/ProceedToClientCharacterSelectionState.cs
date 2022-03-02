using System;
using HeavyMetalMachines.Matches;
using Hoplon.Assertions;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class ProceedToClientCharacterSelectionState : IProceedToClientCharacterSelectionState
	{
		public ProceedToClientCharacterSelectionState(IGetCurrentMatch getCurrentMatch, StateMachine stateMachine, CharacterSelectionClientState state)
		{
			this._getCurrentMatch = getCurrentMatch;
			this._stateMachine = stateMachine;
			this._state = state;
		}

		public void Proceed()
		{
			this.AssertCurrentMatchExists();
			this._stateMachine.GotoState(this._state, false);
		}

		private void AssertCurrentMatchExists()
		{
			Assert.IsTrue(this._getCurrentMatch.GetIfExisting() != null, "Cannot proceed to Character Selection state without a current match.");
		}

		private readonly IGetCurrentMatch _getCurrentMatch;

		private readonly StateMachine _stateMachine;

		private readonly CharacterSelectionClientState _state;
	}
}
