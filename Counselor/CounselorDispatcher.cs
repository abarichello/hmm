using System;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines.Counselor
{
	public class CounselorDispatcher : CounselorParser
	{
		public CounselorDispatcher(IFrameProcessorFactory factory)
		{
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.Counselor, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ArrivalDuringReplay).Bind(FrameKind.Counselor, new ProcessFrame(this.Process));
		}

		private void Process(IFrame frame, IFrameProcessContext ctx)
		{
			this.Update(frame.GetReadData());
		}

		private const FrameKind Kind = FrameKind.Counselor;
	}
}
