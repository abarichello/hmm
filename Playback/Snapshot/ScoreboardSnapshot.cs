using System;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class ScoreboardSnapshot : SimpleFeatureSnapshot
	{
		public override FrameKind Kind
		{
			get
			{
				return FrameKind.Scoreboard;
			}
		}
	}
}
