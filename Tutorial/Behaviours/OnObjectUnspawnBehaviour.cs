using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class OnObjectUnspawnBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			if (this.Combat == null)
			{
				Debug.LogError("CombatObject is null On ObjectUnspawnBehaviour. GameObject Name: " + base.gameObject.name);
				return;
			}
			this.Combat.ListenToObjectUnspawn += this.OnObjectUnspawn;
		}

		private void OnObjectUnspawn(CombatObject combatObject, UnspawnEvent msg)
		{
			this.CompleteBehaviourAndSync();
			this.Combat.ListenToObjectUnspawn -= this.OnObjectUnspawn;
		}

		public CombatObject Combat;
	}
}
