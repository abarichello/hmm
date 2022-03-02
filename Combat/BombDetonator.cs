using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombDetonator : GameHubBehaviour
	{
		public void Detonate(int deliveryScore)
		{
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			BombVisualController.GetInstance().Detonate(deliveryScore);
			foreach (ParticleSystem particleSystem in base.GetComponentsInChildren<ParticleSystem>())
			{
				particleSystem.Play();
			}
		}

		public void RemoveBombAsset()
		{
			GameHubBehaviour.Hub.Events.TriggerEvent(new PickupRemoveEvent
			{
				Causer = -1,
				PickupId = base.Id.ObjId,
				Position = base.transform.position,
				Reason = SpawnReason.Hide,
				TargetEventId = -1
			});
		}
	}
}
