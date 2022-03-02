using System;
using HeavyMetalMachines.ApplicationStates;
using HeavyMetalMachines.Frontend;
using Pocketverse;

namespace HeavyMetalMachines.MainMenuView
{
	public class ProceedToMainMenu : IProceedToMainMenu
	{
		public ProceedToMainMenu(StateMachine stateMachine, MainMenu mainMenuState)
		{
			this._stateMachine = stateMachine;
			this._mainMenuState = mainMenuState;
		}

		public void Proceed()
		{
			this._stateMachine.GotoState(this._mainMenuState, false);
		}

		private readonly StateMachine _stateMachine;

		private readonly MainMenu _mainMenuState;
	}
}
