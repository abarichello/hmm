using System;

namespace HeavyMetalMachines.Playback
{
	public interface IServerPlaybackDispatcher
	{
		int GetNextFrameId();

		void SendFrame(FrameKind kind, bool reliable, int frameId, int previousFrameId, byte[] data);

		void SendSnapshot(byte to, FrameKind kind, int frameId, int previousFrameId, int time, byte[] data);
	}
}
