using System;
using HeavyMetalMachines.Scene;
using HeavyMetalMachines.Tutorial.InGame;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class CreepSpawnBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			for (int i = 0; i < this.ActivatedCreepSpawn.Length; i++)
			{
				this.ActivatedCreepSpawn[i].Activate(true, -1);
			}
			this.CompleteBehaviourAndSync();
		}

		protected override void OnStepCompletedOnServer()
		{
			base.OnStepCompletedOnServer();
			if (this.DestroyCreepsUponComplete)
			{
				for (int i = 0; i < this.ActivatedCreepSpawn.Length; i++)
				{
					this.ActivatedCreepSpawn[i].DestroyAllCreeps();
				}
			}
		}

		public ActivatedCreepSpawn[] ActivatedCreepSpawn;

		public bool DestroyCreepsUponComplete;
	}
}
