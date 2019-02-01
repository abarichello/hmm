using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class GraphViewNodeLink : ScriptableObject, IGraphViewNodeLink
	{
		public IGraphViewNode GetTarget()
		{
			return this.target;
		}

		public void SetTarget(IGraphViewNode node, int index)
		{
			this.target = (node as GraphViewNode);
			this.targetIndex = index;
		}

		public int GetTargetIndex()
		{
			return this.targetIndex;
		}

		public IGraphViewNode GetOrigin()
		{
			return this.owner;
		}

		public void SetOrigin(IGraphViewNode node, int index)
		{
			this.owner = (node as GraphViewNode);
			this.ownerIndex = index;
		}

		public int GetOriginIndex()
		{
			return this.ownerIndex;
		}

		public IList<LinkWaypoint> GetWaypoints()
		{
			return this.waypoints;
		}

		public void AddWaypoint(int x, int y, int position)
		{
			if (position == -1)
			{
				this.waypoints.Add(new LinkWaypoint(x, y));
			}
			else
			{
				this.waypoints.Insert(position, new LinkWaypoint(x, y));
			}
		}

		public bool IsVertical()
		{
			return false;
		}

		[SerializeField]
		private GraphViewNode target;

		[SerializeField]
		private GraphViewNode owner;

		public int targetIndex;

		public int ownerIndex;

		public List<LinkWaypoint> waypoints = new List<LinkWaypoint>();
	}
}
