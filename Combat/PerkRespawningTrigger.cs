using System;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkRespawningTrigger : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient() || GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery)
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(this.Effect.Owner);
			if (combat.IsBot)
			{
				GameHubBehaviour.Hub.Events.Bots.SetRespawningData(combat.Player.PlayerCarId, this.Effect.Data.Origin, this.Effect.Data.Direction, SpawnReason.Respawn);
			}
			else
			{
				GameHubBehaviour.Hub.Events.Players.SetRespawningData(combat.Player.PlayerCarId, this.Effect.Data.Origin, this.Effect.Data.Direction, SpawnReason.Respawn);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkRespawningTrigger));
	}
}
