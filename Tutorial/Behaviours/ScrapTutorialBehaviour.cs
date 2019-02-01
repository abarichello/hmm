using System;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class ScrapTutorialBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			foreach (PlayerStats playerStats in GameHubBehaviour.Hub.ScrapBank.PlayerAccounts.Values)
			{
				playerStats.AddScrap(this.amount, false, ScrapBank.ScrapReason.none);
			}
			this.CompleteBehaviourAndSync();
		}

		[SerializeField]
		private int amount;
	}
}
