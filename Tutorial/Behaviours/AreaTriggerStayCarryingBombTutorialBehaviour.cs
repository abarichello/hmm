using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class AreaTriggerStayCarryingBombTutorialBehaviour : InGameTutorialBehaviourBase
	{
		private void OnTriggerStay(Collider coll)
		{
			if (GameHubBehaviour.Hub.BombManager.IsCarryingBomb(base.playerController.Combat.Id.ObjId))
			{
				CombatObject combat = CombatRef.GetCombat(coll);
				if (base.playerController.Combat == combat)
				{
					this.CompleteBehaviourAndSync();
				}
			}
		}
	}
}
