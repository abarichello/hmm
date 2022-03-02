using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Utils;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class EndMatchTutorialBehaviour : ActionTutorialBehaviourBase
	{
		protected override void ExecuteAction()
		{
			Game game = GameHubBehaviour.Hub.State.Current as Game;
			Debug.Assert(game != null, "Can't End Match cause it's not in State: Game", Debug.TargetTeam.All);
			game.EndMatch();
			game.ClearBackToMain();
		}
	}
}
