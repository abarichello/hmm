using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class StatsDispatcher : StatsParser
	{
		public StatsDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.PlayerStats, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ArrivalDuringReplay).Bind(FrameKind.PlayerStats, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.FastForward).Bind(FrameKind.PlayerStats, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.RewindExecutionQueue).Bind(FrameKind.PlayerStats, new ProcessFrame(this.Process));
		}

		private void Process(IFrame frame, IFrameProcessContext ctx)
		{
			this.Update(frame.GetReadData());
		}

		private const FrameKind Kind = FrameKind.PlayerStats;
	}
}
