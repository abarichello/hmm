using System;

namespace HeavyMetalMachines.Playback
{
	public enum OperationKind
	{
		Playback,
		ReplayPlayback,
		ReplayRewind,
		ReplayExecutionQueue,
		ArrivalDuringReplay,
		FastForward,
		Rewind,
		RewindExecutionQueue
	}
}
