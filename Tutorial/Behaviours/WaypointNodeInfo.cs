using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class WaypointNodeInfo : TutorialPathNode
	{
		public void Awake()
		{
			TutorialPathNode componentInParent = base.GetComponentInParent<TutorialPathNode>();
			Debug.Assert(componentInParent != null, string.Format("WaypointNodeInfo must have a parent with script: TutorialPathNode. {0} ObjectPath:{1}", Environment.NewLine, base.gameObject.GetFullPath()), Debug.TargetTeam.All);
			UnityUtils.SnapToGroundPlane(componentInParent.transform, 0f);
		}

		public void Activate()
		{
			this.activeWaypoint.SetActive(true);
			this.inactiveWaypoint.SetActive(false);
		}

		public void Deactivate()
		{
			this.activeWaypoint.SetActive(false);
			this.inactiveWaypoint.SetActive(true);
		}

		public float sphereRadius = 5f;

		public GameObject activeWaypoint;

		public GameObject inactiveWaypoint;
	}
}
