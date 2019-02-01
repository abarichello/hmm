using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Utils
{
	internal class KdTree<T>
	{
		public bool IsEmpty
		{
			get
			{
				return this._root == null;
			}
		}

		~KdTree()
		{
			this.Clear();
		}

		public void Build(Vector2[] keys, T[] data)
		{
			if (keys == null)
			{
				throw new ArgumentNullException("keys");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (keys.Length != data.Length)
			{
				throw new ArgumentException("keys must be the same length as data.");
			}
			this.Clear();
			this._root = this.Build(keys, data, 0, keys.Length - 1, 0);
			if (this._knnQueue == null)
			{
				this._knnQueue = new PriorityQueue<float, T>(16);
			}
		}

		private KdTree<T>.Node Build(Vector2[] keys, T[] data, int left, int right, int depth)
		{
			if (left > right)
			{
				return null;
			}
			if (left == right)
			{
				return new KdTree<T>.Node(keys[left], data[left], null, null);
			}
			int num = right - left + 1;
			if ((depth & 1) == 0)
			{
				Array.Sort<Vector2, T>(keys, data, left, num, this._xComparer);
			}
			else
			{
				Array.Sort<Vector2, T>(keys, data, left, num, this._yComparer);
			}
			int num2 = left + (num >> 1);
			return new KdTree<T>.Node(keys[num2], data[num2], this.Build(keys, data, left, num2 - 1, depth + 1), this.Build(keys, data, num2 + 1, right, depth + 1));
		}

		public void Clear()
		{
			if (this._root != null)
			{
				this.Clear(ref this._root);
			}
		}

		private void Clear(ref KdTree<T>.Node node)
		{
			if (node.Left != null)
			{
				this.Clear(ref node.Left);
			}
			if (node.Right != null)
			{
				this.Clear(ref node.Right);
			}
			node.Data = default(T);
			node = null;
		}

		public void RangeSearch(Vector2 center, float radius, ref List<T> foundData)
		{
			if (foundData == null)
			{
				throw new ArgumentNullException("foundData");
			}
			foundData.Clear();
			this.RangeSearch(center, radius, this._root, 0, ref foundData);
		}

		private void RangeSearch(Vector2 center, float radius, KdTree<T>.Node node, int depth, ref List<T> found)
		{
			if (node == null)
			{
				return;
			}
			if ((center - node.Key).sqrMagnitude <= radius * radius)
			{
				found.Add(node.Data);
			}
			int index = depth & 1;
			float num = node.Key[index];
			float num2 = center[index];
			if (num2 + radius <= num)
			{
				this.RangeSearch(center, radius, node.Left, depth + 1, ref found);
			}
			else if (num2 - radius > num)
			{
				this.RangeSearch(center, radius, node.Right, depth + 1, ref found);
			}
			else
			{
				this.RangeSearch(center, radius, node.Left, depth + 1, ref found);
				this.RangeSearch(center, radius, node.Right, depth + 1, ref found);
			}
		}

		public T NearestNeighbor(Vector2 point)
		{
			if (this._root == null)
			{
				throw new AccessViolationException("Tree is empty");
			}
			T result = default(T);
			this.NearestNeighbor(point, this._root, 0, float.MaxValue, ref result);
			return result;
		}

		private float NearestNeighbor(Vector2 point, KdTree<T>.Node node, int depth, float radius, ref T data)
		{
			float sqrMagnitude = (point - node.Key).sqrMagnitude;
			if (sqrMagnitude < radius * radius)
			{
				data = node.Data;
				radius = Mathf.Sqrt(sqrMagnitude);
			}
			int index = depth & 1;
			float num = node.Key[index];
			float num2 = point[index];
			if (node.Left != null && num2 <= num)
			{
				radius = this.NearestNeighbor(point, node.Left, depth + 1, radius, ref data);
				if (node.Right != null && num2 + radius > num)
				{
					return this.NearestNeighbor(point, node.Right, depth + 1, radius, ref data);
				}
			}
			else if (node.Right != null && num2 > num)
			{
				radius = this.NearestNeighbor(point, node.Right, depth + 1, radius, ref data);
				if (node.Left != null && num2 - radius <= num)
				{
					return this.NearestNeighbor(point, node.Left, depth + 1, radius, ref data);
				}
			}
			return radius;
		}

		public void KNearestNeighbors(Vector2 point, int k, ref List<T> neighbors)
		{
			if (this._root == null)
			{
				throw new AccessViolationException("Tree is empty");
			}
			if (k <= 0)
			{
				throw new ArgumentException("k must be positive.", "k");
			}
			if (k > this._knnQueue.Capacity)
			{
				this._knnQueue = new PriorityQueue<float, T>(2 * this._knnQueue.Capacity);
			}
			neighbors.Clear();
			this.KNearestNeighbors(point, this._root, 0, float.MaxValue, k);
			while (!this._knnQueue.IsEmpty)
			{
				neighbors.Add(this._knnQueue.Pop());
			}
			neighbors.Reverse();
		}

		private float KNearestNeighbors(Vector2 point, KdTree<T>.Node node, int depth, float radius, int k)
		{
			float sqrMagnitude = (point - node.Key).sqrMagnitude;
			if (sqrMagnitude < radius * radius || this._knnQueue.Size < k)
			{
				if (this._knnQueue.Size == k)
				{
					this._knnQueue.Pop();
				}
				this._knnQueue.Push(node.Data, -Mathf.Sqrt(sqrMagnitude));
				radius = -this._knnQueue.GetPriority(this._knnQueue.Peek());
			}
			int index = depth & 1;
			float num = node.Key[index];
			float num2 = point[index];
			if (node.Left != null && num2 <= num)
			{
				radius = this.KNearestNeighbors(point, node.Left, depth + 1, radius, k);
				if (node.Right != null && num2 + radius > num)
				{
					return this.KNearestNeighbors(point, node.Right, depth + 1, radius, k);
				}
			}
			else if (node.Right != null && num2 > num)
			{
				radius = this.KNearestNeighbors(point, node.Right, depth + 1, radius, k);
				if (node.Left != null && num2 - radius <= num)
				{
					return this.KNearestNeighbors(point, node.Left, depth + 1, radius, k);
				}
			}
			return radius;
		}

		private KdTree<T>.Node _root;

		private KdTree<T>.XComparer _xComparer = new KdTree<T>.XComparer();

		private KdTree<T>.YComparer _yComparer = new KdTree<T>.YComparer();

		private PriorityQueue<float, T> _knnQueue;

		private class Node
		{
			public Node(Vector2 key, T data, KdTree<T>.Node left = null, KdTree<T>.Node right = null)
			{
				this.Key = key;
				this.Data = data;
				this.Left = left;
				this.Right = right;
			}

			public T Data;

			public Vector2 Key;

			public KdTree<T>.Node Left;

			public KdTree<T>.Node Right;
		}

		private class XComparer : IComparer<Vector2>
		{
			public int Compare(Vector2 first, Vector2 second)
			{
				int num = first.x.CompareTo(second.x);
				return (num != 0) ? num : first.y.CompareTo(second.y);
			}
		}

		private class YComparer : IComparer<Vector2>
		{
			public int Compare(Vector2 first, Vector2 second)
			{
				int num = first.y.CompareTo(second.y);
				return (num != 0) ? num : first.x.CompareTo(second.x);
			}
		}
	}
}
