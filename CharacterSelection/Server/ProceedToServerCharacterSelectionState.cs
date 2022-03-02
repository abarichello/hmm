using System;
using HeavyMetalMachines.Matches;
using Hoplon.Assertions;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Server
{
	public class ProceedToServerCharacterSelectionState : IProceedToServerCharacterSelectionState
	{
		public ProceedToServerCharacterSelectionState(IGetCurrentMatch getCurrentMatch, StateMachine stateMachine, CharacterSelectionServerState serverState)
		{
			this._getCurrentMatch = getCurrentMatch;
			this._stateMachine = stateMachine;
			this._serverState = serverState;
		}

		public void Proceed()
		{
			this.AssertCurrentMatchExists();
			this._stateMachine.GotoState(this._serverState, false);
		}

		private void AssertCurrentMatchExists()
		{
			Assert.IsTrue(this._getCurrentMatch.GetIfExisting() != null, "Cannot proceed to Character Selection state without a current match.");
		}

		private readonly IGetCurrentMatch _getCurrentMatch;

		private readonly StateMachine _stateMachine;

		private readonly CharacterSelectionServerState _serverState;
	}
}
