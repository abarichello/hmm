using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class ForceUnspawnBehaviour : InGameTutorialBehaviourBase
	{
		private void OnDisable()
		{
			for (int i = 0; i < this.DeathTriggerList.Count; i++)
			{
				switch (this.DeathTriggerList[i])
				{
				case TargetKind.RedCreep:
					ForceUnspawnBehaviour.UnspawnCreeps(GameHubBehaviour.Hub.Events.Creeps.GetRedCreepList());
					break;
				case TargetKind.BluCreep:
					ForceUnspawnBehaviour.UnspawnCreeps(GameHubBehaviour.Hub.Events.Creeps.GetBlueCreepList());
					break;
				case TargetKind.RedBot:
					this.UnspawnBots(TeamKind.Blue);
					break;
				case TargetKind.BluBot:
					this.UnspawnBots(TeamKind.Blue);
					break;
				case TargetKind.Player:
					this.UnspawnAllPlayers();
					break;
				}
			}
		}

		private void UnspawnBots(TeamKind team)
		{
			List<PlayerData> list = GameHubBehaviour.Hub.Players.Bots.FindAll((PlayerData b) => b.Team == team);
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i].CharacterInstance == null))
				{
					GameHubBehaviour.Hub.Events.Bots.BotForceUnspawn(list[i]);
				}
			}
		}

		private void UnspawnAllPlayers()
		{
			List<PlayerData> players = GameHubBehaviour.Hub.Players.Players;
			for (int i = 0; i < players.Count; i++)
			{
				CombatController component = players[i].CharacterInstance.GetComponent<CombatController>();
				component.ForceDeath();
			}
		}

		private static void UnspawnCreeps(List<CreepController> creeps)
		{
			for (int i = 0; i < creeps.Count; i++)
			{
				creeps[i].Combat.Controller.ForceDeath();
			}
		}

		public List<TargetKind> DeathTriggerList = new List<TargetKind>();
	}
}
