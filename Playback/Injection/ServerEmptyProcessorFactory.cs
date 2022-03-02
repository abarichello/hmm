using System;

namespace HeavyMetalMachines.Playback.Injection
{
	internal class ServerEmptyProcessorFactory : IFrameProcessorFactory
	{
		public void Initialize()
		{
		}

		public IFrameProcessorProvider GetProvider(OperationKind kind)
		{
			return this._empty;
		}

		private ServerEmptyProcessorProvider _empty = new ServerEmptyProcessorProvider();
	}
}
