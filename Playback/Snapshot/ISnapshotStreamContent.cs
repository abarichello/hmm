using System;

namespace HeavyMetalMachines.Playback.Snapshot
{
	public interface ISnapshotStreamContent
	{
		short Version { get; set; }

		void ApplyStreamData(byte[] data);
	}
}
