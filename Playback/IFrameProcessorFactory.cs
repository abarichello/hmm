using System;

namespace HeavyMetalMachines.Playback
{
	public interface IFrameProcessorFactory
	{
		IFrameProcessorProvider GetProvider(OperationKind kind);
	}
}
