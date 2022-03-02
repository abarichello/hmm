using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.InGame;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial
{
	public class TutorialPathNode : GameHubBehaviour
	{
		private void OnEnable()
		{
			this.waypointTutorialBehaviour = base.transform.parent.GetComponent<WaypointTutorialBehaviour>();
		}

		public void Reset()
		{
			this._conditionCounter = 0f;
		}

		public TutorialPathNode IterateNode()
		{
			TutorialPathNode.NextNodeConditions nextNodeConditions = this.nextNodeCondition;
			if (nextNodeConditions == TutorialPathNode.NextNodeConditions.Directly)
			{
				return this.nextNode;
			}
			if (nextNodeConditions == TutorialPathNode.NextNodeConditions.AfterReachCount)
			{
				return ((this._conditionCounter += 1f) <= this.conditionLimit) ? (this.altNode ?? this.nextNode) : this.nextNode;
			}
			if (nextNodeConditions != TutorialPathNode.NextNodeConditions.AfterTime)
			{
				return this.nextNode;
			}
			return ((this._conditionCounter += Time.deltaTime) <= this.conditionLimit) ? (this.altNode ?? this.nextNode) : this.nextNode;
		}

		private void OnTriggerEnter(Collider other)
		{
			if (this.waypointTutorialBehaviour != null)
			{
				CombatObject combat = CombatRef.GetCombat(other);
				if (combat != null && combat.Player.PlayerAddress == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerAddress)
				{
					this.waypointTutorialBehaviour.WaypointReached();
				}
			}
		}

		[ReadOnly]
		public TutorialPathNode nextNode;

		public TutorialPathNode altNode;

		public TutorialPathNode.NextNodeConditions nextNodeCondition;

		public float conditionLimit;

		private float _conditionCounter;

		private WaypointTutorialBehaviour waypointTutorialBehaviour;

		public enum NextNodeConditions
		{
			Directly,
			AfterReachCount,
			AfterTime
		}
	}
}
