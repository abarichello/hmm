using System;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.InGame
{
	public class InGameTutorialBehaviourPathSystemHolder : InGameTutorialBehaviourBase
	{
		protected virtual void OnDrawGizmos()
		{
			for (int i = 0; i < this.path.nodes.Count; i++)
			{
				TutorialPathNode tutorialPathNode = this.path.nodes[i];
				if (!(tutorialPathNode == null))
				{
					this.DrawGizmoForNode(tutorialPathNode, new Color(1f - (float)i / (float)this.path.nodes.Count, (float)i / (float)this.path.nodes.Count, 0f));
				}
			}
		}

		protected virtual void DrawGizmoForNode(TutorialPathNode tNode, Color pColor)
		{
			Gizmos.color = pColor;
			Gizmos.DrawIcon(tNode.transform.position, "node.png");
			Gizmos.DrawWireCube(tNode.transform.position, new Vector3(2f, 0.05f, 2f));
			if (tNode.nextNode != null)
			{
				Gizmos.DrawLine(tNode.transform.position, tNode.nextNode.transform.position);
			}
			if (tNode.altNode != null)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(tNode.transform.position, tNode.altNode.transform.position);
			}
		}

		[HideInInspector]
		public TutorialPathSystem path = new TutorialPathSystem();
	}
}
