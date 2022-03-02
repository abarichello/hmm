using System;
using HeavyMetalMachines.Frontend;
using Pocketverse;

namespace HeavyMetalMachines.ApplicationStates
{
	public class ProceedToTutorialState : IProceedToTutorialState
	{
		public ProceedToTutorialState(StateMachine stateMachine, MatchmakingTutorial matchmakingTutorial, GuiLoadingController guiLoadingController)
		{
			this._stateMachine = stateMachine;
			this._matchmakingTutorial = matchmakingTutorial;
			this._guiLoadingController = guiLoadingController;
		}

		public void Proceed()
		{
			this._guiLoadingController.ShowTutorialLoading(false);
			this._stateMachine.GotoState(this._matchmakingTutorial, false);
		}

		private readonly StateMachine _stateMachine;

		private readonly MatchmakingTutorial _matchmakingTutorial;

		private readonly GuiLoadingController _guiLoadingController;
	}
}
