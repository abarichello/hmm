using System;
using System.Collections;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Tutorial.Behaviours;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class WaypointTutorialBehaviour : InGameTutorialBehaviourPathSystemHolder
	{
		private void Awake()
		{
		}

		private void GetNextWaypoint()
		{
			this._reachedWaypoint = false;
			if (this._currentWaypoint != null)
			{
				this._currentWaypoint.gameObject.SetActive(false);
				this.currentWaypointInfo.Deactivate();
			}
			if (this.path.isLastNode(this._currentWaypoint))
			{
				this.OnWaypointsPathCompleted();
				return;
			}
			this._currentWaypoint = (this._nextWaypoint ?? this.path.GetNextNode());
			this._nextWaypoint = this.path.GetNextNode();
			if (this._currentWaypoint != null)
			{
				this._currentWaypoint.gameObject.SetActive(true);
			}
			this.currentWaypointInfo = this._currentWaypoint.GetComponentInChildren<WaypointNodeInfo>();
			this.currentWaypointInfo.Activate();
			if (this._nextWaypoint != null)
			{
				this._nextWaypoint.gameObject.SetActive(true);
			}
		}

		protected override void StartBehaviourOnClient()
		{
			base.StartBehaviourOnClient();
			if (GameHubBehaviour.Hub.Net.isTest)
			{
				return;
			}
			this._playerCO = base.playerController.GetComponent<CombatObject>();
			this._arrowHintInstance = Object.Instantiate<GameObject>(this.arrowHint);
			this._arrowHintInstance.transform.parent = this._playerCO.Dummy.transform;
			this._arrowHintInstance.transform.localScale = Vector3.one;
			this._arrowHintInstance.transform.localPosition = Vector3.zero;
			this.GetNextWaypoint();
		}

		protected override void UpdateOnClient()
		{
			base.UpdateOnClient();
			if (this._currentWaypoint == null || this._arrowHintInstance == null)
			{
				return;
			}
			this._arrowHintInstance.transform.LookAt((this._nextWaypoint ?? this._currentWaypoint).transform);
		}

		public void WaypointReached()
		{
			if (!this._reachedWaypoint)
			{
				this._reachedWaypoint = true;
				base.StartCoroutine(this.OnWaypointReach());
			}
		}

		private IEnumerator OnWaypointReach()
		{
			yield return base.StartCoroutine(this.StartNodeActions(this._currentWaypoint));
			this.GetNextWaypoint();
			yield break;
		}

		private void OnWaypointsPathCompleted()
		{
			this._currentWaypoint = null;
			this._nextWaypoint = null;
			if (this._arrowHintInstance != null)
			{
				Object.Destroy(this._arrowHintInstance);
			}
			this.CompleteBehaviourAndSync();
		}

		protected override void OnStepCompletedOnClient()
		{
			base.OnStepCompletedOnClient();
			if (this._arrowHintInstance != null)
			{
				Object.Destroy(this._arrowHintInstance);
			}
		}

		private IEnumerator StartNodeActions(TutorialPathNode pNextNode)
		{
			DialogBehaviour tDialog = pNextNode.GetComponent<DialogBehaviour>();
			if (tDialog == null)
			{
				yield break;
			}
			tDialog.ShowDialogForBehaviour();
			while (!tDialog.behaviourCompleted)
			{
				yield return null;
			}
			yield break;
		}

		protected override void DrawGizmoForNode(TutorialPathNode tNode, Color pColor)
		{
			base.DrawGizmoForNode(tNode, pColor);
			WaypointNodeInfo componentInChildren = tNode.GetComponentInChildren<WaypointNodeInfo>();
			if (componentInChildren != null)
			{
				Gizmos.color = pColor;
				Gizmos.DrawWireSphere(tNode.transform.position, componentInChildren.sphereRadius);
			}
		}

		public InputModifierTutorialBehaviour inputModifier;

		private CombatObject _playerCO;

		public GameObject arrowHint;

		private GameObject _arrowHintInstance;

		private TutorialPathNode _currentWaypoint;

		private TutorialPathNode _nextWaypoint;

		private bool _reachedWaypoint;

		private WaypointNodeInfo currentWaypointInfo;
	}
}
