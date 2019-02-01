using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnBombPhaseChange : BasePerk, DestroyEffect.IDestroyEffectListener
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

		private void ServerDestroy(BombScoreBoard.State state)
		{
			this.Effect.TriggerDefaultDestroy(-1);
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.ServerDestroy;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyOnBombPhaseChange));
	}
}
