using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class PlayerUnspawnBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			base.playerController.Combat.ListenToObjectUnspawn += this.OnPlayerUnspawn;
		}

		private void OnPlayerUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			this.CompleteBehaviourAndSync();
			base.playerController.Combat.ListenToObjectUnspawn -= this.OnPlayerUnspawn;
		}
	}
}
