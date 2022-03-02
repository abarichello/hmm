using System;
using HeavyMetalMachines.Tutorial.Behaviours;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class FollowPathBehaviour : InGameTutorialBehaviourPathSystemHolder
	{
		protected override void StartBehaviourOnServer()
		{
			base.StartBehaviourOnServer();
			this._botSpawned = true;
			this._node = this.path.GetNextNode();
		}

		protected override void UpdateOnServer()
		{
			base.UpdateOnServer();
			if (!this._botSpawned || this._node == null)
			{
				return;
			}
			this._botStarter.BotAIController.MoveToDirectionCanGoStraight(this._node.transform.position, false);
			float sqrMagnitude = (this._botStarter.BotAIController.transform.position - this._node.transform.position).sqrMagnitude;
			if (sqrMagnitude >= this.AcceptanceDistanceSqr)
			{
				return;
			}
			this._node = this.path.GetNextNode();
			if (this._node != null)
			{
				this._botStarter.BotAIController.MoveToDirectionCanGoStraight(this._node.transform.position, false);
			}
			else
			{
				this._botStarter.BotAIController.StopBot(true);
				if (this.OnPathFinisheCompleteBehaviour)
				{
					this.CompleteBehaviourAndSync();
				}
			}
		}

		public bool OnPathFinisheCompleteBehaviour;

		private bool _botSpawned;

		private TutorialPathNode _node;

		public BotStarterTutorialBehaviour _botStarter;

		[Tooltip("Distance squared to accept node as done and try to go to next node")]
		public float AcceptanceDistanceSqr = 2000f;
	}
}
