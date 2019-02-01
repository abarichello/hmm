using System;
using HeavyMetalMachines.Tutorial.InGame;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class LoadPlayerPortraitsTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			base.GameGui.HudPlayersController.TutorialAddPlayers();
		}
	}
}
