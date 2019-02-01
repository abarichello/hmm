using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.BotAI
{
	public class BotAIPath : GameHubBehaviour
	{
		public static BotAIPath Terrain
		{
			get
			{
				return BotAIPath._terrain;
			}
		}

		private void Awake()
		{
			if (this.Kind == BotAIPath.PathKind.Terrain && !BotAIPath._terrain)
			{
				BotAIPath._terrain = this;
			}
		}

		private void OnDestroy()
		{
			if (BotAIPath._terrain == this)
			{
				BotAIPath._terrain = null;
			}
		}

		public float BombLinkWeigth = 1f;

		public float HazardLinkWeigth = 100f;

		public BotAIPath.PathKind Kind;

		public List<BotAINode> Nodes = new List<BotAINode>();

		private static BotAIPath _terrain;

		public enum PathKind
		{
			Terrain,
			Patrol
		}
	}
}
