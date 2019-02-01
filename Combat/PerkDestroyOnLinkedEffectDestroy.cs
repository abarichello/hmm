using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnLinkedEffectDestroy : BasePerk, DestroyEffect.IDestroyEffectListener
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				GameHubBehaviour.Hub.Events.Effects.ListenToDestroy(this.Effect.Data.TargetEventId, new EffectsManager.EffectDestroyed(this.ServerDestroy));
			}
		}

		private void ServerDestroy(EffectRemoveEvent data)
		{
			this.Effect.TriggerDestroy(-1, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
		}

		public void OnDestroyEffect(DestroyEffect evt)
		{
			GameHubBehaviour.Hub.Events.Effects.UnlistenToDestroy(this.Effect.Data.TargetEventId, new EffectsManager.EffectDestroyed(this.ServerDestroy));
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyOnLinkedEffectDestroy));
	}
}
