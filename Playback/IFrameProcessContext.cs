using System;

namespace HeavyMetalMachines.Playback
{
	public interface IFrameProcessContext
	{
		int PlaybackTime { get; }

		void AddToExecutionQueue(int frameId);

		bool RemoveFromExecutionQueue(int frameId);

		void EnqueueAction(int time, Action action);
	}
}
