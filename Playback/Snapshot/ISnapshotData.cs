using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public interface ISnapshotData<in TSnapshotData> where TSnapshotData : ISnapshotData<TSnapshotData>
	{
		IFrame BaseFrame { get; }

		IList<IFrame> Frames { get; }

		void Init(IFrame baseFrame, TSnapshotData lastSnapshot);
	}
}
