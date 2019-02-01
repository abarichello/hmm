using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class GraphView : ScriptableObject
	{
		public IGraphViewNode CurrentState
		{
			get
			{
				return this.currentNode;
			}
		}

		public void SetNodeAsEntryPoint(IGraphViewNode node)
		{
			this.entryNode = node;
		}

		public bool IsNodeEntryPoint(IGraphViewNode node)
		{
			return this.entryNode == node;
		}

		public IGraphViewNode[] Nodes = new IGraphViewNode[0];

		private IGraphViewNode currentNode;

		private IGraphViewNode entryNode;
	}
}
