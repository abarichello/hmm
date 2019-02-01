using System;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class GraphContainer : ScriptableObject
	{
		public virtual GraphView CreateView()
		{
			return new GraphView();
		}

		public GraphView graphView;
	}
}
