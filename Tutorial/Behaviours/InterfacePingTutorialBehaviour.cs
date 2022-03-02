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
			Debug.Assert(gameObject != null, string.Format("Could not find <from> gameObject with path:{0}", this.FromPath), Debug.TargetTeam.All);
			Debug.Assert(gameObject2 != null, string.Format("Could not find <to> gameObject with path:{0}", this.ToPath), Debug.TargetTeam.All);
			if (gameObject == null || gameObject2 == null)
			{
				return;
			}
			Vector3 vector = this.FromOffset * ((!this.IsOffsetInGuiSpace) ? 1f : TutorialUIController.Instance.InterfacePing.RootLocalScale());
			Vector3 vector2 = this.ToOffset * ((!this.IsOffsetInGuiSpace) ? 1f : TutorialUIController.Instance.InterfacePing.RootLocalScale());
			TutorialUIController.Instance.InterfacePing.ExecutePing(gameObject.transform.position + vector, gameObject2.transform.position + vector2);
		}

		public string FromPath;

		public Vector3 FromOffset;

		public string ToPath;

		public Vector3 ToOffset;

		public bool IsOffsetInGuiSpace;
	}
}
