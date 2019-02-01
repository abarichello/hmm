using System;
using System.Collections.Generic;
using HeavyMetalMachines.Tutorial.Behaviours;

namespace HeavyMetalMachines.Tutorial.InGame
{
	[Serializable]
	public class TutorialPathSystem
	{
		private void Init()
		{
			this._initialized = true;
			if (this.autoInactiveObjects)
			{
				for (int i = 0; i < this.nodes.Count; i++)
				{
					TutorialPathNode tutorialPathNode = this.nodes[i];
					tutorialPathNode.gameObject.SetActive(false);
				}
			}
		}

		public void Reset()
		{
			for (int i = 0; i < this.nodes.Count; i++)
			{
				TutorialPathNode tutorialPathNode = this.nodes[i];
				tutorialPathNode.Reset();
			}
		}

		public bool isLastNode(TutorialPathNode node)
		{
			return this.nodes.IndexOf(node) == this.nodes.Count - 1;
		}

		public TutorialPathNode GetNextNode()
		{
			if (!this._initialized)
			{
				this.Init();
			}
			if (this._currentNode == null)
			{
				this._currentNode = this.nodes[0];
			}
			else
			{
				if (this.autoInactiveObjects)
				{
					this._currentNode.gameObject.SetActive(false);
					HudMaskTutorialBehaviour component = this._currentNode.gameObject.GetComponent<HudMaskTutorialBehaviour>();
					if (component)
					{
						component.CompleteBehaviourAndSync();
					}
				}
				this._currentNode = this._currentNode.IterateNode();
			}
			if (this._currentNode != null && this.autoInactiveObjects)
			{
				this._currentNode.gameObject.SetActive(true);
				HudMaskTutorialBehaviour component2 = this._currentNode.gameObject.GetComponent<HudMaskTutorialBehaviour>();
				if (component2)
				{
					component2.StartBehaviour(null);
				}
			}
			return this._currentNode;
		}

		public bool autoInactiveObjects = true;

		public bool closePath;

		public List<TutorialPathNode> nodes = new List<TutorialPathNode>();

		private TutorialPathNode _currentNode;

		private bool _initialized;
	}
}
