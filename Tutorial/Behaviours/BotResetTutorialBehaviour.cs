using System;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class BotResetTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Bots.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Bots[i];
				BotAIController disabledComponent = BotAIUtils.GetDisabledComponent<BotAIController>(playerData.CharacterInstance);
				disabledComponent.goalManager.enabled = false;
				disabledComponent.goalManager.TutorialDisabled = true;
				disabledComponent.currentCombatObject = null;
				disabledComponent.StopBot(true);
				disabledComponent.enabled = false;
				SpawnController disabledComponent2 = BotAIUtils.GetDisabledComponent<SpawnController>(playerData.CharacterInstance);
				disabledComponent.Combat.Movement.ForcePosition(disabledComponent2.SpawnPosition.position, true);
				disabledComponent.Combat.Movement.ResetImpulseAndVelocity();
			}
			this.CompleteBehaviourAndSync();
		}
	}
}
