using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HeavyMetalMachines.Utils
{
	public class NodeHolderLifeCycle<T> where T : INodeHolder
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<T> OnMemberRemoved;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<T> OnMemberAdded;

		public bool AddMember(T holder)
		{
			int num = 0;
			for (int i = 0; i < holder.NodeRemovals.Length; i++)
			{
				int num2 = holder.NodeRemovals[i];
				T t;
				if (this._nodeHolders.TryGetValue(num2, out t))
				{
					bool flag = this.UpdateTree(t, num2);
					if (flag)
					{
						this._liveHolders.Add(t);
					}
					else
					{
						this._liveHolders.Remove(t);
					}
				}
				else if (Array.IndexOf<int>(holder.NodeIds, num2) >= 0)
				{
					num++;
				}
			}
			bool flag2 = holder.NodeIds.Length > num || this._liveHolders.Count > 0;
			this._liveHolders.Clear();
			if (flag2)
			{
				for (int j = 0; j < holder.NodeIds.Length; j++)
				{
					int num3 = holder.NodeIds[j];
					if (Array.IndexOf<int>(holder.NodeRemovals, num3) < 0)
					{
						this._nodeHolders[num3] = holder;
					}
				}
				for (int k = 0; k < holder.NodeRemovals.Length; k++)
				{
					int num4 = holder.NodeRemovals[k];
					if (Array.IndexOf<int>(holder.NodeIds, num4) < 0)
					{
						this._removeHolders[num4] = holder;
					}
				}
				this.LiveHolders.Add(holder);
				if (this.OnMemberAdded != null)
				{
					this.OnMemberAdded(holder);
				}
				return true;
			}
			if (this.OnMemberAdded != null)
			{
				this.OnMemberAdded(holder);
			}
			if (this.OnMemberRemoved != null)
			{
				this.OnMemberRemoved(holder);
			}
			return false;
		}

		private bool UpdateTree(T holder, int removal)
		{
			this._nodeHolders.Remove(removal);
			this._removedCreationHolders[removal] = holder;
			bool flag = false;
			for (int i = 0; i < holder.NodeIds.Length; i++)
			{
				int key = holder.NodeIds[i];
				if (this._nodeHolders.ContainsKey(key))
				{
					flag = true;
				}
			}
			for (int j = 0; j < holder.NodeRemovals.Length; j++)
			{
				int key2 = holder.NodeRemovals[j];
				if (this._removedCreationHolders.ContainsKey(key2))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				this.ForgetNode(holder);
			}
			return flag;
		}

		private void ForgetNode(T node)
		{
			this.LiveHolders.Remove(node);
			for (int i = 0; i < node.NodeIds.Length; i++)
			{
				int key = node.NodeIds[i];
				this._removedCreationHolders.Remove(key);
				T holder;
				if (this._removeHolders.TryGetValue(key, out holder))
				{
					this.CheckRemoval(holder);
				}
			}
			for (int j = 0; j < node.NodeRemovals.Length; j++)
			{
				int key2 = node.NodeRemovals[j];
				this._removeHolders.Remove(key2);
			}
			if (this.OnMemberRemoved != null)
			{
				this.OnMemberRemoved(node);
			}
		}

		private void CheckRemoval(T holder)
		{
			for (int i = 0; i < holder.NodeIds.Length; i++)
			{
				int key = holder.NodeIds[i];
				if (this._nodeHolders.ContainsKey(key))
				{
					return;
				}
			}
			for (int j = 0; j < holder.NodeRemovals.Length; j++)
			{
				int key2 = holder.NodeRemovals[j];
				if (this._removedCreationHolders.ContainsKey(key2))
				{
					return;
				}
			}
			this.ForgetNode(holder);
		}

		public List<T> LiveHolders = new List<T>();

		private Dictionary<int, T> _nodeHolders = new Dictionary<int, T>();

		private Dictionary<int, T> _removeHolders = new Dictionary<int, T>();

		private Dictionary<int, T> _removedCreationHolders = new Dictionary<int, T>();

		private HashSet<T> _liveHolders = new HashSet<T>();
	}
}
