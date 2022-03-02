using System;
using Pocketverse;

namespace HeavyMetalMachines.Playback.Injection
{
	internal class ServerEmptyProcessorProvider : IFrameProcessorProvider
	{
		public void Bind(FrameKind type, ProcessFrame processor)
		{
		}

		public void Unbind(FrameKind type)
		{
		}

		public ProcessFrame GetProcessor(FrameKind type)
		{
			return new ProcessFrame(this.EmptyProcess);
		}

		private void EmptyProcess(IFrame frame, IFrameProcessContext ctx)
		{
			ServerEmptyProcessorProvider.Log.ErrorFormatStackTrace("Trying to call processor for FrameKind={0}", new object[]
			{
				(FrameKind)frame.Type
			});
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ServerEmptyPlayback));
	}
}
