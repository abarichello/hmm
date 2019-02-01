using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class PickupSpawnBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			for (int i = 0; i < this.Dummies.Length; i++)
			{
				GameHubBehaviour.Hub.Events.TriggerEvent(new PickupDropEvent
				{
					Position = this.Dummies[i].position,
					PickupAsset = this.PickupAsset,
					UnspawnOnLifeTimeEnd = false
				});
			}
		}

		public string PickupAsset;

		public Transform[] Dummies;
	}
}
