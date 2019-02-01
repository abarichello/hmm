using System;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class InterfacePingTutorialBehaviour : ActionTutorialBehaviourBase
	{
		protected override void ExecuteAction()
		{
			GameObject gameObject = GameObject.Find(this.FromPath);
			GameObject gameObject2 = GameObject.Find(this.ToPath);
			HeavyMetalMachines.Utils.Debug.Assert(gameObject != null, string.Format("Could not find <from> gameObject with path:{0}", this.FromPath), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			HeavyMetalMachines.Utils.Debug.Assert(gameObject2 != null, string.Format("Could not find <to> gameObject with path:{0}", this.ToPath), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			if (gameObject == null || gameObject2 == null)
			{
				return;
			}
			Vector3 b = this.FromOffset * ((!this.IsOffsetInGuiSpace) ? 1f : TutorialUIController.Instance.InterfacePing.RootLocalScale());
			Vector3 b2 = this.ToOffset * ((!this.IsOffsetInGuiSpace) ? 1f : TutorialUIController.Instance.InterfacePing.RootLocalScale());
			TutorialUIController.Instance.InterfacePing.ExecutePing(gameObject.transform.position + b, gameObject2.transform.position + b2);
		}

		public string FromPath;

		public Vector3 FromOffset;

		public string ToPath;

		public Vector3 ToOffset;

		public bool IsOffsetInGuiSpace;
	}
}
