using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class GadgetEventDispatcher : GadgetEventParser
	{
		public GadgetEventDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.GadgetEvent, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.GadgetEvent, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayRewind).Bind(FrameKind.GadgetEvent, new ProcessFrame(this.Rewind));
			factory.GetProvider(OperationKind.ReplayExecutionQueue).Bind(FrameKind.GadgetEvent, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.Rewind).Bind(FrameKind.GadgetEvent, new ProcessFrame(this.Rewind));
			factory.GetProvider(OperationKind.RewindExecutionQueue).Bind(FrameKind.GadgetEvent, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.GadgetEvent, new ProcessFrame(this.FastForward));
		}

		private void PlaybackProcess(IFrame frame, IFrameProcessContext ctx)
		{
			this.Process(frame.GetReadData());
		}

		private void Rewind(IFrame frame, IFrameProcessContext ctx)
		{
			bool flag = this.RewindProcess(frame);
			ctx.RemoveFromExecutionQueue(frame.FrameId);
			if (flag && frame.PreviousFrameId != -1)
			{
				ctx.AddToExecutionQueue(frame.PreviousFrameId);
			}
		}

		private void FastForward(IFrame frame, IFrameProcessContext ctx)
		{
			ctx.AddToExecutionQueue(frame.FrameId);
		}

		private const FrameKind Kind = FrameKind.GadgetEvent;
	}
}
