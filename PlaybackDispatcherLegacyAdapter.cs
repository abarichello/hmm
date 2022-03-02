using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PlaybackDispatcherLegacyAdapter : GameHubObject, IServerPlaybackDispatcher
	{
		public int GetNextFrameId()
		{
			return GameHubObject.Hub.PlaybackManager.GetNextFrameId();
		}

		public void SendFrame(FrameKind kind, bool reliable, int frameId, int previousFrameId, byte[] data)
		{
			GameHubObject.Hub.PlaybackManager.SendKeyFrame((byte)kind, reliable, frameId, previousFrameId, data);
		}

		public void SendSnapshot(byte to, FrameKind kind, int frameId, int previousFrameId, int time, byte[] data)
		{
			GameHubObject.Hub.PlaybackManager.SendFullKeyFrame(to, (byte)kind, frameId, previousFrameId, time, data);
		}
	}
}
