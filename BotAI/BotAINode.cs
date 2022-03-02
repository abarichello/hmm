using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	[Serializable]
	public class BotAINode : GameHubBehaviour
	{
		public bool HasRangeCollider { get; private set; }

		public Collider RangeCollider { get; private set; }

		private void Awake()
		{
			this.RangeCollider = base.GetComponent<Collider>();
			this.HasRangeCollider = this.RangeCollider;
		}

		public List<BotAINode> LinkedNodes = new List<BotAINode>();

		public List<BotAINode.NodeLinkKind> LinkKinds = new List<BotAINode.NodeLinkKind>();

		public BotAINode.NodeRequiredForPathKind RequirementKind;

		public float Range = 10f;

		public int Hash;

		public Vector3 position;

		[NonSerialized]
		public BotAINode NextNode;

		[NonSerialized]
		public BotAINode PreviousNode;

		public enum NodeRequiredForPathKind
		{
			Never,
			Always,
			WithBomb,
			WithoutBomb
		}

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
