using System;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public interface IFeatureSnapshot
	{
		FrameKind Kind { get; }

		void RewindToTime(int currentTime, int targetTime, IFrameProcessContext ctx);

		void AddFrame(IFrame frame);

		void Clear();
	}
}
