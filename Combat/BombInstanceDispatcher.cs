using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class BombInstanceDispatcher : BombInstanceParser
	{
		public BombInstanceDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.BombInstance, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.BombInstance, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayRewind).Bind(FrameKind.BombInstance, new ProcessFrame(this.NoProcess));
			factory.GetProvider(OperationKind.ReplayExecutionQueue).Bind(FrameKind.BombInstance, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ArrivalDuringReplay).Bind(FrameKind.BombInstance, new ProcessFrame(this.NoProcess));
		}

		private void NoProcess(IFrame frame, IFrameProcessContext ctx)
		{
		}

		private void PlaybackProcess(IFrame frame, IFrameProcessContext ctx)
		{
			this.Process(frame.GetReadData());
		}

		private const FrameKind Kind = FrameKind.BombInstance;
	}
}
