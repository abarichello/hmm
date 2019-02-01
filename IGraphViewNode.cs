using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines
{
	public interface IGraphViewNode
	{
		Vector2 GetPosition();

		void SetPosition(int x, int y);

		Vector2 GetDimensions();

		GraphViewNodeColor GetColor();

		IList<IGraphViewNodeLink> GetLinks();

		void AddLink(IGraphViewNodeLink link);

		void RemoveLink(IGraphViewNodeLink link);

		string GetName();
	}
}
