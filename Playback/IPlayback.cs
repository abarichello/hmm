using System;
using Pocketverse;

namespace HeavyMetalMachines.Playback
{
	public interface IPlayback
	{
		void Init();

		void Play();

		void Stop();

		bool IsRunningReplay { get; }

		void AddStateKeyFrame(byte keyFrameType, int frameId, int previousFrameId, int time, byte[] data);

		void SetBuffer(IMatchBuffer buffer);
	}
}
