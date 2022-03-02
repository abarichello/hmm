using System;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class MatchTeamsSnapshot : SimpleFeatureSnapshot
	{
		public override FrameKind Kind
		{
			get
			{
				return FrameKind.Teams;
			}
		}
	}
}
