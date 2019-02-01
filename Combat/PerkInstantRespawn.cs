using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkInstantRespawn : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.BombDelivery)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(this.Effect.Owner);
			if (combat.IsBot)
			{
				GameHubBehaviour.Hub.Events.Bots.SetSpawnData(combat.Player, this.Effect.Data.Origin, this.Effect.Data.Direction, SpawnReason.Respawn);
			}
			else
			{
				GameHubBehaviour.Hub.Events.Players.SetSpawnData(combat.Player, this.Effect.Data.Origin, this.Effect.Data.Direction, SpawnReason.Respawn);
			}
			this.Effect.Gadget.Pressed = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkInstantRespawn));
	}
}
