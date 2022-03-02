using System;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public class MatchPlayersSnapshot : SimpleFeatureSnapshot
	{
		public override FrameKind Kind
		{
			get
			{
				return FrameKind.Players;
			}
		}
	}
}
