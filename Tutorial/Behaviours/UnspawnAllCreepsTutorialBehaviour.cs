using System;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class UnspawnAllCreepsTutorialBehaviour : ActionTutorialBehaviourBase
	{
		protected override void ExecuteAction()
		{
			GameHubBehaviour.Hub.Events.Creeps.UnspawnAllCreeps();
		}
	}
}
