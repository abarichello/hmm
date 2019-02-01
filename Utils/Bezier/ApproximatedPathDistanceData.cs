using System;
using Pocketverse;

namespace HeavyMetalMachines.Utils.Bezier
{
	public class ApproximatedPathDistanceData : GameHubScriptableObject
	{
		public int[] PathIndex;

		public int[] SegmentKeys;

		public float[] SegmentValues;
	}
}
