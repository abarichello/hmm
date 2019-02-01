using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class RemoveOutOfCombatBehaviour : InGameTutorialBehaviourBase
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
					MartyrModifiersOutOfCombat martyrModifiersOutOfCombat = (MartyrModifiersOutOfCombat)component.OutOfCombatGadget;
					martyrModifiersOutOfCombat.DestroyEffectAndResetCooldown();
					martyrModifiersOutOfCombat.CurrentCooldownTime = 86400000L + martyrModifiersOutOfCombat.CurrentTime;
				}
			}
			MartyrModifiersOutOfCombat martyrModifiersOutOfCombat2 = (MartyrModifiersOutOfCombat)base.playerController.Combat.OutOfCombatGadget;
			martyrModifiersOutOfCombat2.DestroyEffectAndResetCooldown();
			martyrModifiersOutOfCombat2.CurrentCooldownTime = 86400000L + martyrModifiersOutOfCombat2.CurrentTime;
			this.CompleteBehaviourAndSync();
		}

		private const long OneDayInMilliseconds = 86400000L;
	}
}
