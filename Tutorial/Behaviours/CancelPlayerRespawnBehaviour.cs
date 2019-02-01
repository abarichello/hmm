using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class CancelPlayerRespawnBehaviour : InGameTutorialBehaviourBase
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this._spawnController = base.playerController.GetComponent<SpawnController>();
			this._spawnController.RespawnTimeMillis = -1;
		}

		protected override void OnStepCompletedOnServer()
		{
			base.OnStepCompletedOnServer();
			this._spawnController.RespawnTimeMillis = GameHubBehaviour.Hub.Events.Players.RespawnTimeSeconds * 1000;
		}

		private SpawnController _spawnController;
	}
}
