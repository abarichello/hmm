using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public class CombatFeedbackDispatcher : CombatFeedbackParser
	{
		public CombatFeedbackDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.CombatFeedbacks, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.CombatFeedbacks, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.ReplayExecutionQueue).Bind(FrameKind.CombatFeedbacks, new ProcessFrame(this.PlaybackProcess));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.CombatFeedbacks, new ProcessFrame(this.PlaybackProcess));
		}

		private void PlaybackProcess(IFrame frame, IFrameProcessContext ctx)
		{
			this.Process(frame.GetReadData());
		}

		private const FrameKind Kind = FrameKind.CombatFeedbacks;
	}
}
