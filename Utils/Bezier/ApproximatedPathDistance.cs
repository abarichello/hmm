using System;
using System.Collections.Generic;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Utils.Bezier
{
	public class ApproximatedPathDistance : GameHubBehaviour
	{
		public List<BotAINode> Path
		{
			get
			{
				return this._path;
			}
		}

		public Dictionary<int, float> Segments
		{
			get
			{
				return this._segments;
			}
		}

		public int Count
		{
			get
			{
				return this._path.Count;
			}
		}

		public float SqrLength
		{
			get
			{
				return this._sqrLength;
			}
		}

		public void Start()
		{
			BotAIPath botAIPath = UnityEngine.Object.FindObjectOfType<BotAIPath>();
			this.Load(botAIPath.Nodes, this.Data);
		}

		public void Clear()
		{
			this._path.Clear();
			this._segments.Clear();
		}

		public void Load(List<BotAINode> AllNodes, ApproximatedPathDistanceData data)
		{
			Vector2[] array = new Vector2[data.PathIndex.Length];
			int[] array2 = new int[data.PathIndex.Length];
			for (int i = 0; i < data.PathIndex.Length; i++)
			{
				BotAINode botAINode = AllNodes[data.PathIndex[i]];
				this._path.Add(botAINode);
				array[i].x = botAINode.transform.position.x;
				array[i].y = botAINode.transform.position.z;
				array2[i] = i;
			}
			for (int j = 0; j < data.SegmentKeys.Length; j++)
			{
				this._segments.Add(data.SegmentKeys[j], data.SegmentValues[j]);
			}
			this._sqrLength = this._segments[data.PathIndex[0]];
			this.kdtree = new KdTree<int>();
			this.kdtree.Build(array, array2);
			if (this.kNearestNodesIndex == null)
			{
				this.kNearestNodesIndex = new List<int>(3);
			}
		}

		public void DistanceToGoal(Vector3 point, ref ApproximatedPathDistance.SegmentInfo info)
		{
			int index = info.index;
			ApproximatedPathDistance.ResetSegmentInfo(ref info);
			Vector2 point2;
			point2.x = point.x;
			point2.y = point.z;
			this.kdtree.KNearestNeighbors(point2, 3, ref this.kNearestNodesIndex);
			this.kNearestNodesIndex.Sort();
			bool flag = false;
			int num = 524288;
			num |= 512;
			for (int i = this.kNearestNodesIndex.Count - 1; i >= 0; i--)
			{
				int num2 = this.kNearestNodesIndex[i];
				if (num2 > 0)
				{
					int num3 = num2 - 1;
					Vector3 direction = this._path[num3].position - point;
					if (!Physics.Raycast(point, direction, direction.magnitude, num))
					{
						this.CalcDistanceSqr(point, num3, num2, ref info);
						flag = true;
					}
				}
				int num4 = num2 + 1;
				if (num4 < this._path.Count)
				{
					Vector3 direction2 = this._path[num4].position - point;
					if (!Physics.Raycast(point, direction2, direction2.magnitude, num))
					{
						this.CalcDistanceSqr(point, num2, num4, ref info);
						flag = true;
					}
				}
			}
			if (!flag)
			{
				this.CalcDistanceSqr(point, index, index + 1, ref info);
			}
			this.SetSegmentInfoValues(ref info);
		}

		private void SetSegmentInfoValues(ref ApproximatedPathDistance.SegmentInfo info)
		{
			BotAINode botAINode = this._path[info.index];
			info.node = botAINode.position;
			info.nearest = info.projOnNearest + info.node;
			info.projMag = info.projOnNearest.sqrMagnitude;
			info.sqrDistance = this._segments[botAINode.Hash] - info.projMag;
		}

		private static void ResetSegmentInfo(ref ApproximatedPathDistance.SegmentInfo info)
		{
			info.index = 0;
			info.minDistSqr = float.MaxValue;
			info.projMag = 0f;
			info.nearest = Vector3.zero;
			info.projOnNearest = Vector3.zero;
		}

		private void CalcDistanceSqr(Vector3 point, int vertexIIndex, int vertexJIndex, ref ApproximatedPathDistance.SegmentInfo info)
		{
			Vector3 vector = point - this._path[vertexIIndex].position;
			Vector3 vector2 = this._path[vertexJIndex].position - this._path[vertexIIndex].position;
			float d = Mathf.Clamp01(Vector3.Dot(vector, vector2) / Vector3.Dot(vector2, vector2));
			Vector3 vector3 = d * vector2;
			Vector3 vector4 = vector - vector3;
			float num = Vector3.Dot(vector4, vector4);
			if (num < info.minDistSqr)
			{
				info.minDistSqr = num;
				info.projOnNearest = vector3;
				info.index = vertexIIndex;
			}
		}

		public void CalculateDistances(BotAIPath AllNodesPath)
		{
			BotAIPathFind.Acquire(AllNodesPath.Nodes);
			BombTargetTrigger[] array = UnityEngine.Object.FindObjectsOfType<BombTargetTrigger>();
			BotAINode botAINode = null;
			BotAINode targetNode = null;
			foreach (BombTargetTrigger bombTargetTrigger in array)
			{
				if (bombTargetTrigger.TeamOwner == TeamKind.Red)
				{
					botAINode = BotAIPathFind.GetClosestNode(AllNodesPath, bombTargetTrigger.transform.position, bombTargetTrigger.TeamOwner, false);
				}
				else
				{
					targetNode = BotAIPathFind.GetClosestNode(AllNodesPath, bombTargetTrigger.transform.position, bombTargetTrigger.TeamOwner, false);
				}
			}
			BotAIPathFind.FindPath(AllNodesPath, botAINode.transform.position, targetNode, true, TeamKind.Red, ref this._path);
			float num = 0f;
			int num2 = this._path.Count - 1;
			this._segments.Add(this._path[num2].Hash, num);
			for (int j = num2; j > 0; j--)
			{
				num += Vector3.SqrMagnitude(this._path[j].position - this._path[j - 1].position);
				this._segments.Add(this._path[j - 1].Hash, num);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ApproximatedPathDistance));

		public ApproximatedPathDistanceData Data;

		private List<BotAINode> _path = new List<BotAINode>();

		private readonly Dictionary<int, float> _segments = new Dictionary<int, float>();

		private List<int> kNearestNodesIndex;

		private float _sqrLength;

		private KdTree<int> kdtree;

		private const int kNN = 3;

		public struct SegmentInfo
		{
			public int index;

			public float minDistSqr;

			public float projMag;

			public float sqrDistance;

			public Vector3 nearest;

			public Vector3 node;

			public Vector3 projOnNearest;
		}
	}
}
