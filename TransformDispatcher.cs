using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class TransformDispatcher : TransformParser
	{
		public TransformDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.TransformStates, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.TransformStates, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.TransformStates, new ProcessFrame(this.PlaybackProcess));
		}

		private void PlaybackProcess(IFrame frame, IFrameProcessContext ctx)
		{
			this.Process(frame.GetReadData());
		}

		private const FrameKind Kind = FrameKind.TransformStates;
	}
}
