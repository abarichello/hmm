using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Playback
{
	public class KeyFrameLegacyAdapter
	{
		public KeyFrameLegacyAdapter(IFrameProcessorFactory factory, IKeyFrameParser parser)
		{
			this._parser = parser;
			FrameKind type = parser.Type.Convert();
			factory.GetProvider(OperationKind.Playback).Bind(type, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(type, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ReplayRewind).Bind(type, new ProcessFrame(this.RewindProcess));
			factory.GetProvider(OperationKind.ReplayExecutionQueue).Bind(type, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ArrivalDuringReplay).Bind(type, new ProcessFrame(this.NoProcess));
			factory.GetProvider(OperationKind.FastForward).Bind(type, new ProcessFrame(this.Process));
		}

		private void NoProcess(IFrame frame, IFrameProcessContext ctx)
		{
		}

		private void Process(IFrame frame, IFrameProcessContext ctx)
		{
			this._parser.Process(frame.GetReadData());
		}

		private void RewindProcess(IFrame frame, IFrameProcessContext ctx)
		{
			bool flag = this._parser.RewindProcess(frame);
			if (flag && frame.PreviousFrameId != -1)
			{
				ctx.AddToExecutionQueue(frame.PreviousFrameId);
			}
			if (frame.PreviousFrameId == 0)
			{
				Debug.LogError("Prev=0 parser type=" + this._parser.GetType());
			}
			ctx.RemoveFromExecutionQueue(frame.FrameId);
		}

		private IKeyFrameParser _parser;
	}
}
