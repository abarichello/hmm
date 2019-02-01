using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	[Serializable]
	public class BotAINode : GameHubBehaviour
	{
		public List<BotAINode> LinkedNodes = new List<BotAINode>();

		public List<BotAINode.NodeLinkKind> LinkKinds = new List<BotAINode.NodeLinkKind>();

		public float Range = 10f;

		public int Hash;

		public Vector3 position;

		[NonSerialized]
		public BotAINode NextNode;

		[NonSerialized]
		public BotAINode PreviousNode;

		public enum NodeLinkKind
		{
			Standard,
			BombBlocked,
			RedBlock,
			BluBlock,
			Hazard
		}
	}
}
