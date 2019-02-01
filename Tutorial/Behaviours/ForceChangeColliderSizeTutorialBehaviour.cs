using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class ForceChangeColliderSizeTutorialBehaviour : InGameTutorialBehaviourPathSystemHolder
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			BoxCollider component = GameHubBehaviour.Hub.Players.Players[0].CharacterInstance.GetComponent<BoxCollider>();
			component.size = this._carCollider.Size;
			component.center = this._carCollider.Center;
		}

		[SerializeField]
		private CarCollider _carCollider;
	}
}
