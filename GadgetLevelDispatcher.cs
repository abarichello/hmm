using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class GadgetLevelDispatcher : GadgetLevelParser
	{
		public GadgetLevelDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.GadgetLevel, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.GadgetLevel, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayExecutionQueue).Bind(FrameKind.GadgetLevel, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.GadgetLevel, new ProcessFrame(this.PlaybackProcess));
		}

		private void PlaybackProcess(IFrame frame, IFrameProcessContext ctx)
		{
			this.Process(frame.GetReadData());
		}

		private const FrameKind Kind = FrameKind.GadgetLevel;
	}
}
