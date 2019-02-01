using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class GraphViewNode : ScriptableObject, IGraphViewNode
	{
		public virtual bool ValidateConditions(List<KeyValuePair<string, object>> conditions)
		{
			return false;
		}

		public Vector2 GetPosition()
		{
			return Vector2.zero;
		}

		public void SetPosition(int x, int y)
		{
		}

		public GraphViewNodeColor GetColor()
		{
			return GraphViewNodeColor.Gray;
		}

		public virtual Vector2 GetDimensions()
		{
			return new Vector2(150f, 100f);
		}

		public IList<IGraphViewNodeLink> GetLinks()
		{
			return this.links;
		}

		public void AddLink(IGraphViewNodeLink link)
		{
			this.links.Add(link);
		}

		public void RemoveLink(IGraphViewNodeLink link)
		{
			this.links.Remove(link);
		}

		public virtual bool ValidateConditions(List<string> conditions)
		{
			return false;
		}

		public virtual string[] GetValidConditions()
		{
			return new string[0];
		}

		public virtual string GetName()
		{
			if (string.IsNullOrEmpty(base.name))
			{
				return "(unnamed)";
			}
			return base.name;
		}

		[HideInInspector]
		public List<IGraphViewNodeLink> links = new List<IGraphViewNodeLink>();
	}
}
