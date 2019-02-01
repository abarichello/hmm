using System;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	[Obsolete("User UnspawnCountBehaviour instead")]
	public class CreepLastHitCountBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn += this.CreepsOnListenToCreepUnspawn;
		}

		private void CreepsOnListenToCreepUnspawn(CreepRemoveEvent data)
		{
			if (GameHubBehaviour.Hub.Players.Players[0].CharacterInstance.Id.ObjId != data.CauserId)
			{
				return;
			}
			this._creepCount++;
			if (this.CreepsToCompleteBehaviour <= this._creepCount)
			{
				GameHubBehaviour.Hub.Events.Creeps.ListenToCreepUnspawn -= this.CreepsOnListenToCreepUnspawn;
				this.CompleteBehaviourAndSync();
			}
		}

		public int CreepsToCompleteBehaviour;

		private int _creepCount;
	}
}
