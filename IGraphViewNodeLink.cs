using System;
using System.Collections.Generic;

namespace HeavyMetalMachines
{
	public interface IGraphViewNodeLink
	{
		IGraphViewNode GetTarget();

		void SetTarget(IGraphViewNode node, int index);

		int GetTargetIndex();

		IGraphViewNode GetOrigin();

		void SetOrigin(IGraphViewNode node, int index);

		int GetOriginIndex();

		IList<LinkWaypoint> GetWaypoints();

		bool IsVertical();

		void AddWaypoint(int x, int y, int position);
	}
}
