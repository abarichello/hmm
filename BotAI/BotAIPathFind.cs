using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.BotAI.PathFind;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Hoplon.Math;
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
			if (BotAIPathFind.routes == null)
			{
				BotAIPathFind.routes = new Dictionary<AiPathRoute, int[]>(nodes.Count);
			}
			if (BotAIPathFind.nodesByHash == null)
			{
				BotAIPathFind.nodesByHash = new Dictionary<int, BotAINode>(nodes.Count);
				for (int i = 0; i < nodes.Count; i++)
				{
					BotAIPathFind.nodesByHash[nodes[i].Hash] = nodes[i];
				}
			}
			if (BotAIPathFind.kdtree == null)
			{
				Vector2[] array = new Vector2[nodes.Count];
				for (int j = 0; j < array.Length; j++)
				{
					array[j].x = nodes[j].position.x;
					array[j].y = nodes[j].position.z;
				}
				BotAIPathFind.kdtree = new KdTree<BotAINode>();
				BotAIPathFind.kdtree.Build(array, nodes.ToArray());
			}
			if (BotAIPathFind.kNearestNodes == null)
			{
				BotAIPathFind.kNearestNodes = new List<BotAINode>(5);
			}
			if (GameHubBehaviour.Hub)
			{
				BotAIPathFind._forcePathFind = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.ForceAlwaysRunPathFind);
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
				if (BotAIPathFind.routes != null)
				{
					BotAIPathFind.routes.Clear();
					BotAIPathFind.routes = null;
				}
				if (BotAIPathFind.nodesByHash != null)
				{
					BotAIPathFind.nodesByHash.Clear();
					BotAIPathFind.nodesByHash = null;
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
			if (closestNode.Hash == targetNode.Hash)
			{
				finalPath.Add(closestNode);
				return closestNode;
			}
			float num = Vector2.left[0];
			AiPathRoute key = new AiPathRoute(closestNode.Hash, targetNode.Hash, isBombBlocking, team);
			int[] array;
			if (!BotAIPathFind._forcePathFind && BotAIPathFind.routes.TryGetValue(key, out array))
			{
				for (int i = 0; i < array.Length; i++)
				{
					finalPath.Add(BotAIPathFind.nodesByHash[array[i]]);
				}
				return closestNode;
			}
			for (int j = 0; j < path.Nodes.Count; j++)
			{
				path.Nodes[j].PreviousNode = null;
				path.Nodes[j].NextNode = null;
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
					array = finalPath.ConvertAll<int>((BotAINode x) => x.Hash).ToArray();
					BotAIPathFind.routes[key] = array;
					return closestNode;
				}
				BotAIPathFind.closed.Add(botAINode);
				float num2 = BotAIPathFind.g_scores[botAINode];
				for (int k = 0; k < botAINode.LinkedNodes.Count; k++)
				{
					BotAINode botAINode2 = botAINode.LinkedNodes[k];
					BotAINode.NodeLinkKind nodeLinkKind = botAINode.LinkKinds[k];
					if (!BotAIPathFind.closed.Contains(botAINode2))
					{
						float num3 = 1f;
						switch (nodeLinkKind)
						{
						case BotAINode.NodeLinkKind.BombBlocked:
							if (isBombBlocking)
							{
								goto IL_379;
							}
							num3 = path.BombLinkWeigth;
							break;
						case BotAINode.NodeLinkKind.RedBlock:
							if (team == TeamKind.Red)
							{
								goto IL_379;
							}
							break;
						case BotAINode.NodeLinkKind.BluBlock:
							if (team == TeamKind.Blue)
							{
								goto IL_379;
							}
							break;
						case BotAINode.NodeLinkKind.Hazard:
							if (GameHubBehaviour.Hub && GameHubBehaviour.Hub.BombManager.ActiveBomb.IsSpawned && GameHubBehaviour.Hub.BombManager.ActiveBomb.BombCarriersIds.Count > 0)
							{
								num3 = path.HazardLinkWeigth;
							}
							break;
						}
						float num4 = num2 + num3 * BotAIPathFind.CostEstimate(path, botAINode, botAINode2);
						if (!BotAIPathFind.open.Contains(botAINode2))
						{
							BotAIPathFind.parent.Add(botAINode2, botAINode);
							BotAIPathFind.g_scores.Add(botAINode2, num4);
							BotAIPathFind.open.Push(botAINode2, num4 + BotAIPathFind.CostEstimate(path, botAINode2, targetNode));
						}
						else if (BotAIPathFind.g_scores[botAINode2] > num4)
						{
							BotAIPathFind.parent[botAINode2] = botAINode;
							BotAIPathFind.g_scores[botAINode2] = num4;
							BotAIPathFind.open.SetPriority(botAINode2, num4 + BotAIPathFind.CostEstimate(path, botAINode2, targetNode));
						}
					}
					IL_379:;
				}
			}
			return null;
		}

		public static BotAINode GetClosestNode(BotAIPath path, Vector3 position, TeamKind team, bool bombBlock)
		{
			Vector2 vector;
			vector.x = position.x;
			vector.y = position.z;
			BotAIPathFind.kdtree.KNearestNeighbors(vector, 5, ref BotAIPathFind.kNearestNodes);
			BotAINode botAINode = null;
			for (int i = 0; i < BotAIPathFind.kNearestNodes.Count; i++)
			{
				Vector3 vector2 = BotAIPathFind.kNearestNodes[i].position - position;
				if (!Physics.Raycast(position, vector2, vector2.magnitude, LayerManager.GetBombAndTeamSceneryMask(bombBlock, team)))
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

		private static Dictionary<AiPathRoute, int[]> routes;

		private static Dictionary<int, BotAINode> nodesByHash;

		private static bool _forcePathFind;
	}
}
