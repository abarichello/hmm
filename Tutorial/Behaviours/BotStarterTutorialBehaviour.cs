using System;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class BotStarterTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this.BotData = GameHubBehaviour.Hub.Players.Bots.Find((PlayerData b) => b.TeamSlot == this.Slot && b.Team == this.TeamKind);
			if (this.BotData == null)
			{
				BotStarterTutorialBehaviour.Log.ErrorFormat("Bot could not be found. Slot {0} - TeamKind {1}", new object[]
				{
					this.Slot,
					this.TeamKind
				});
				return;
			}
			if (GameHubBehaviour.Hub.Events.Bots.IsUnspawned(this.BotData.PlayerCarId))
			{
				SpawnController disabledComponent = BotAIUtils.GetDisabledComponent<SpawnController>(this.BotData.CharacterInstance);
				if (!disabledComponent)
				{
					BotStarterTutorialBehaviour.Log.ErrorFormat("Could not Get SpawnController for: {0} BotAdress: {1}", new object[]
					{
						this.BotData.PlayerAddress
					});
					return;
				}
				GameHubBehaviour.Hub.Events.Bots.ForceObjectSpawn(disabledComponent, disabledComponent.SpawnPosition.position, disabledComponent.SpawnPosition.forward);
			}
			this.BotAIController = BotAIUtils.GetDisabledComponent<BotAIController>(this.BotData.CharacterInstance);
			if (this.BotAIController == null)
			{
				BotStarterTutorialBehaviour.Log.ErrorFormat("BotAIController could not be found for: {0}. PlayerAdress: {1}", new object[]
				{
					(!this.BotData.CharacterInstance) ? "Character Instance is null" : this.BotData.CharacterInstance.name,
					this.BotData.PlayerAddress
				});
				return;
			}
			this.BotAIController.goalManager.TutorialDisabled = false;
			this.BotAIController.goalManager.enabled = this.AIBot;
			this.BotAIController.enabled = true;
			this.BotAIController.Combat.Movement.ForcePosition(this.Dummy.position, true);
			this.BotAIController.Combat.Movement.ResetImpulseAndVelocity();
			if (this.CompleteOnBotUnspawn)
			{
				GameHubBehaviour.Hub.Events.Bots.ListenToObjectUnspawn += this.OnBotUnspawn;
			}
		}

		private void OnBotUnspawn(PlayerEvent data)
		{
			this.CompleteBehaviourAndSync();
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectUnspawn -= this.OnBotUnspawn;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BotStarterTutorialBehaviour));

		public bool AIBot;

		public bool CompleteOnBotUnspawn;

		public int Slot;

		public TeamKind TeamKind;

		public Transform Dummy;

		[HideInInspector]
		public PlayerData BotData;

		[HideInInspector]
		public BotAIController BotAIController;
	}
}
