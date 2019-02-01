using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Combat;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class HudGadgetUiController : GameHubBehaviour
	{
		protected void Start()
		{
			GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned += this.OnAllPlayersSpawned;
		}

		protected void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Players.ListenToAllPlayersSpawned -= this.OnAllPlayersSpawned;
		}

		private void OnAllPlayersSpawned()
		{
			if (SpectatorController.IsSpectating)
			{
				return;
			}
			this._combatObject = GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<CombatObject>();
		}

		private CombatObject _combatObject;
	}
}
