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
	public class ForcePlayerSpawnBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			PlayerData playerData = GameHubBehaviour.Hub.Players.Players[0];
			if (!GameHubBehaviour.Hub.Events.Players.IsUnspawned(playerData.PlayerCarId))
			{
				return;
			}
			SpawnController disabledComponent = BotAIUtils.GetDisabledComponent<SpawnController>(playerData.CharacterInstance);
			if (this.UseBaseSpawnPosition)
			{
				GameHubBehaviour.Hub.Events.Players.ForceObjectSpawn(disabledComponent, disabledComponent.SpawnPosition.position, disabledComponent.SpawnPosition.forward);
			}
			else
			{
				GameHubBehaviour.Hub.Events.Players.ForceObjectSpawn(disabledComponent, this.CustomSpawnPosition.position, this.CustomSpawnPosition.forward);
			}
			GameHubBehaviour.Hub.Events.Players.ForgetEvents(playerData.PlayerCarId);
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.ListenToPlayerSpawn;
		}

		private void ListenToPlayerSpawn(PlayerEvent data)
		{
			this.CompleteBehaviourAndSync();
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.ListenToPlayerSpawn;
		}

		public bool UseBaseSpawnPosition;

		public Transform CustomSpawnPosition;
	}
}
