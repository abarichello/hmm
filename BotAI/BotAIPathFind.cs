using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	[Serializable]
	public class BotAIPathFind : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<List<BotAINode>> OnAIPathFound;

		private void Start()
		{
			this.AllNodes = BotAIPath.Terrain;
		}

		private void OnDrawGizmos()
		{
			if (this.PathFound.Count == 0 || !this.Draw)
			{
				return;
			}
			Gizmos.color = Color.red;
			for (int i = 0; i < this.PathFound.Count; i++)
			{
				Gizmos.DrawSphere(this.PathFound[i].position, 5f);
			}
		}

		public void FindPath(BotAIPath path, Vector3 startingPosition)
		{
			bool isBombBlocking = GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned && GameHubBehaviour.Hub.BombManager.IsCarryingBomb(base.Id.ObjId);
			this.StartingNode = BotAIPathFind.FindPath(path, startingPosition, this.TargetNode, isBombBlocking, base.Id.GetBitComponent<CombatObject>().Team, ref this.PathFound);
			if (this.OnAIPathFound != null)
			{
				this.OnAIPathFound(this.PathFound);
			}
		}

		private static float CostEstimate(BotAIPath path, BotAINode one, BotAINode other)
		{
			int key = (one.Hash <= other.Hash) ? (one.Hash + 1000 * other.Hash) : (one.Hash * 1000 + other.Hash);
			float magnitude;
			if (!BotAIPathFind.nodeCosts.TryGetValue(key, out magnitude))
			{
				magnitude = (one.position - other.position).magnitude;
				BotAIPathFind.nodeCosts[key] = magnitude;
			}
			return magnitude;
		}

		public static void Acquire(List<BotAINode> nodes)
		{
			if (nodes.Count >= 1000)
			{
				throw new ArgumentException("Graph can have a maximum of 1000 nodes");
			}
			if (BotAIPathFind.open == null)
			{
				BotAIPathFind.open = new PriorityQueue<float, BotAINode>(nodes.Count);
			}
			if (BotAIPathFind.closed == null)
			{
				BotAIPathFind.closed = new HashSet<BotAINode>();
			}
			if (BotAIPathFind.parent == null)
			{
				BotAIPathFind.parent = new Dictionary<BotAINode, BotAINode>(nodes.Count);
			}
			if (BotAIPathFind.g_scores == null)
			{
				BotAIPathFind.g_scores = new Dictionary<BotAINode, float>(nodes.Count);
			}
			if (BotAIPathFind.nodeCosts == null)
			{
				BotAIPathFind.nodeCosts = new Dictionary<int, float>((nodes.Count - 1) * (nodes.Count - 1) / 2);
			}
			if (BotAIPathFind.kdtree == null)
			{
				Vector2[] array = new Vector2[nodes.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i].x = nodes[i].position.x;
					array[i].y = nodes[i].position.z;
				}
				BotAIPathFind.kdtree = new KdTree<BotAINode>();
				BotAIPathFind.kdtree.Build(array, nodes.ToArray());
			}
			if (BotAIPathFind.kNearestNodes == null)
			{
				BotAIPathFind.kNearestNodes = new List<BotAINode>(5);
			}
			BotAIPathFind.refCount++;
		}

		public static void Release()
		{
			if (--BotAIPathFind.refCount == 0)
			{
				if (BotAIPathFind.open != null)
				{
					BotAIPathFind.open.Clear();
					BotAIPathFind.open = null;
				}
				if (BotAIPathFind.closed != null)
				{
					BotAIPathFind.closed.Clear();
					BotAIPathFind.closed = null;
				}
				if (BotAIPathFind.parent != null)
				{
					BotAIPathFind.parent.Clear();
					BotAIPathFind.parent = null;
				}
				if (BotAIPathFind.g_scores != null)
				{
					BotAIPathFind.g_scores.Clear();
					BotAIPathFind.g_scores = null;
				}
				if (BotAIPathFind.nodeCosts != null)
				{
					BotAIPathFind.nodeCosts.Clear();
					BotAIPathFind.nodeCosts = null;
				}
				if (BotAIPathFind.kdtree != null)
				{
					BotAIPathFind.kdtree.Clear();
					BotAIPathFind.kdtree = null;
				}
				if (BotAIPathFind.kNearestNodes != null)
				{
					BotAIPathFind.kNearestNodes.Clear();
					BotAIPathFind.kNearestNodes = null;
				}
			}
			if (BotAIPathFind.refCount < 0)
			{
				BotAIPathFind.refCount = 0;
				BotAIPathFind.Log.FatalFormat("refCount was negative!", new object[0]);
			}
		}

		public static BotAINode FindPath(BotAIPath path, Vector3 currentPosition, BotAINode targetNode, bool isBombBlocking, TeamKind team, ref List<BotAINode> finalPath)
		{
			BotAINode closestNode = BotAIPathFind.GetClosestNode(path, currentPosition, team, isBombBlocking);
			finalPath.Clear();
			if (closestNode.name.Equals(targetNode.name))
			{
				finalPath.Add(closestNode);
				return closestNode;
			}
			for (int i = 0; i < path.Nodes.Count; i++)
			{
				path.Nodes[i].PreviousNode = null;
				path.Nodes[i].NextNode = null;
			}
			BotAIPathFind.open.Clear();
			BotAIPathFind.closed.Clear();
			BotAIPathFind.parent.Clear();
			BotAIPathFind.g_scores.Clear();
			BotAIPathFind.open.Push(closestNode, 0f);
			BotAIPathFind.g_scores.Add(closestNode, 0f);
			while (BotAIPathFind.open.Size > 0)
			{
				BotAINode botAINode = BotAIPathFind.open.Pop();
				if (botAINode == targetNode)
				{
					finalPath.Add(targetNode);
					while (BotAIPathFind.parent.ContainsKey(botAINode))
					{
						botAINode = BotAIPathFind.parent[botAINode];
						finalPath.Add(botAINode);
					}
					finalPath.Reverse();
					return closestNode;
				}
				BotAIPathFind.closed.Add(botAINode);
				float num = BotAIPathFind.g_scores[botAINode];
				for (int j = 0; j < botAINode.LinkedNodes.Count; j++)
				{
					BotAINode botAINode2 = botAINode.LinkedNodes[j];
					BotAINode.NodeLinkKind nodeLinkKind = botAINode.LinkKinds[j];
					if (!BotAIPathFind.closed.Contains(botAINode2))
					{
						float num2 = 1f;
						switch (nodeLinkKind)
						{
						case BotAINode.NodeLinkKind.BombBlocked:
							if (isBombBlocking)
							{
								goto IL_2BD;
							}
							num2 = path.BombLinkWeigth;
							break;
						case BotAINode.NodeLinkKind.RedBlock:
							if (team == TeamKind.Red)
							{
								goto IL_2BD;
							}
							break;
						case BotAINode.NodeLinkKind.BluBlock:
							if (team == TeamKind.Blue)
							{
								goto IL_2BD;
							}
							break;
						case BotAINode.NodeLinkKind.Hazard:
							if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned && GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds.Count > 0)
							{
								num2 = path.HazardLinkWeigth;
							}
							break;
						}
						float num3 = num + num2 * BotAIPathFind.CostEstimate(path, botAINode, botAINode2);
						if (!BotAIPathFind.open.Contains(botAINode2))
						{
							BotAIPathFind.parent.Add(botAINode2, botAINode);
							BotAIPathFind.g_scores.Add(botAINode2, num3);
							BotAIPathFind.open.Push(botAINode2, num3 + BotAIPathFind.CostEstimate(path, botAINode2, targetNode));
						}
						else if (BotAIPathFind.g_scores[botAINode2] > num3)
						{
							BotAIPathFind.parent[botAINode2] = botAINode;
							BotAIPathFind.g_scores[botAINode2] = num3;
							BotAIPathFind.open.SetPriority(botAINode2, num3 + BotAIPathFind.CostEstimate(path, botAINode2, targetNode));
						}
					}
					IL_2BD:;
				}
			}
			return null;
		}

		public static BotAINode GetClosestNode(BotAIPath path, Vector3 position, TeamKind team, bool bombBlock)
		{
			Vector2 point;
			point.x = position.x;
			point.y = position.z;
			BotAIPathFind.kdtree.KNearestNeighbors(point, 5, ref BotAIPathFind.kNearestNodes);
			BotAINode botAINode = null;
			for (int i = 0; i < BotAIPathFind.kNearestNodes.Count; i++)
			{
				Vector3 direction = BotAIPathFind.kNearestNodes[i].position - position;
				if (!Physics.Raycast(position, direction, direction.magnitude, LayerManager.GetBombAndTeamSceneryMask(bombBlock, team)))
				{
					botAINode = BotAIPathFind.kNearestNodes[i];
					break;
				}
			}
			if (botAINode == null)
			{
				return BotAIPathFind.kNearestNodes[0];
			}
			return botAINode;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BotAIPathFind));

		public List<BotAINode> PathFound = new List<BotAINode>();

		public BotAIPath AllNodes;

		public BotAINode StartingNode;

		public BotAINode TargetNode;

		public bool Draw;

		private static PriorityQueue<float, BotAINode> open;

		private static HashSet<BotAINode> closed;

		private static Dictionary<BotAINode, BotAINode> parent;

		private static Dictionary<BotAINode, float> g_scores;

		private static Dictionary<int, float> nodeCosts;

		private static KdTree<BotAINode> kdtree;

		private static List<BotAINode> kNearestNodes;

		private const int kNN = 5;

		private static int refCount;
	}
}
