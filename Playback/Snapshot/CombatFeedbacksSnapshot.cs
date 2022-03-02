using System;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class CombatFeedbacksSnapshot : SimpleFeatureSnapshot
	{
		public override FrameKind Kind
		{
			get
			{
				return FrameKind.CombatFeedbacks;
			}
		}
	}
}
