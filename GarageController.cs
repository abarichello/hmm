using System;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class GarageController : GameHubBehaviour
	{
		private void Awake()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				TeamKind team = this.Team;
				if (team != TeamKind.Red)
				{
					if (team == TeamKind.Blue)
					{
						GameHubBehaviour.Hub.BotAIHub.GarageControllerBlu = this;
					}
				}
				else
				{
					GameHubBehaviour.Hub.BotAIHub.GarageControllerRed = this;
				}
			}
			else
			{
				base.enabled = false;
			}
		}

		private void OnDestroy()
		{
			if (!GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			GameHubBehaviour.Hub.BotAIHub.GarageControllerRed = null;
			GameHubBehaviour.Hub.BotAIHub.GarageControllerBlu = null;
		}

		public TeamKind Team;

		public enum UpgradeOperationKind
		{
			Upgrade,
			Return,
			Sell
		}
	}
}
