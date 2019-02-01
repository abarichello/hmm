using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class ResetOutOfCombatBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			List<PlayerData> bots = GameHubBehaviour.Hub.Players.Bots;
			for (int i = 0; i < bots.Count; i++)
			{
				CombatObject component = bots[i].CharacterInstance.GetComponent<CombatObject>();
				if (component)
				{
					component.OutOfCombatGadget.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
				}
			}
			base.playerController.Combat.OutOfCombatGadget.CurrentCooldownTime = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			this.CompleteBehaviourAndSync();
		}
	}
}
