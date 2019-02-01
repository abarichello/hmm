using System;

namespace HeavyMetalMachines
{
	[Serializable]
	public class LinkWaypoint
	{
		public LinkWaypoint(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public int x;

		public int y;
	}
}
