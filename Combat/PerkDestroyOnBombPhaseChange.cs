using System;
using HeavyMetalMachines.Infra.Context;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnBombPhaseChange : BasePerk, DestroyEffectMessage.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.ServerDestroy;
			}
			else
			{
				base.enabled = false;
			}
		}

		private void ServerDestroy(BombScoreboardState state)
		{
			this.Effect.TriggerDefaultDestroy(-1);
		}

		public void OnDestroyEffect(DestroyEffectMessage evt)
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.ServerDestroy;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyOnBombPhaseChange));
	}
}
