using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Playback
{
	public class FrameProcessorProvider : IFrameProcessorProvider
	{
		public ProcessFrame GetProcessor(FrameKind type)
		{
			ProcessFrame result;
			if (this._processors.TryGetValue((byte)type, out result))
			{
				return result;
			}
			return this.NullProcessor;
		}

		public void Bind(FrameKind type, ProcessFrame processor)
		{
			this._processors[(byte)type] = processor;
		}

		public void Unbind(FrameKind type)
		{
			this._processors.Remove((byte)type);
		}

		private readonly BitLogger Log = new BitLogger(typeof(FrameProcessorProvider));

		private readonly ProcessFrame NullProcessor = delegate(IFrame x, IFrameProcessContext y)
		{
		};

		private readonly Dictionary<byte, ProcessFrame> _processors = new Dictionary<byte, ProcessFrame>();
	}
}
