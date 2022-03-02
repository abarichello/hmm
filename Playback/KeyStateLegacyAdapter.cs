using System;
using Pocketverse;

namespace HeavyMetalMachines.Playback
{
	public class KeyStateLegacyAdapter
	{
		public KeyStateLegacyAdapter(IFrameProcessorFactory factory, IKeyStateParser parser)
		{
			this._parser = parser;
			FrameKind type = parser.Type.Convert();
			factory.GetProvider(OperationKind.Playback).Bind(type, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(type, new ProcessFrame(this.NoProcess));
			factory.GetProvider(OperationKind.ReplayRewind).Bind(type, new ProcessFrame(this.NoProcess));
			factory.GetProvider(OperationKind.ReplayExecutionQueue).Bind(type, new ProcessFrame(this.NoProcess));
			factory.GetProvider(OperationKind.ArrivalDuringReplay).Bind(type, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.FastForward).Bind(type, new ProcessFrame(this.Process));
		}

		public void Process(IFrame frame, IFrameProcessContext ctx)
		{
			this._parser.Update(frame.GetReadData());
		}

		public void NoProcess(IFrame frame, IFrameProcessContext ctx)
		{
		}

		private IKeyStateParser _parser;
	}
}
