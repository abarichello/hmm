using System;

namespace HeavyMetalMachines.Playback
{
	public interface IFrameProcessorProvider
	{
		void Bind(FrameKind type, ProcessFrame processor);

		void Unbind(FrameKind type);

		ProcessFrame GetProcessor(FrameKind type);
	}
}
