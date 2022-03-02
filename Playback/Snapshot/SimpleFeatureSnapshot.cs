using System;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public abstract class SimpleFeatureSnapshot : AbstractFeatureSnapshot
	{
		protected override bool CheckAndAddKeyFrame(IFrame frame)
		{
			return true;
		}

		protected override void OnClear()
		{
		}

		protected override void ApplySnapshot(IFrame frame, int targetTime, IFrameProcessContext ctx)
		{
			ctx.AddToExecutionQueue(frame.FrameId);
		}
	}
}
