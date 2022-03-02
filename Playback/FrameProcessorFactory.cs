using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Playback
{
	public class FrameProcessorFactory : GameHubObject, IFrameProcessorFactory
	{
		public IFrameProcessorProvider GetProvider(OperationKind kind)
		{
			IFrameProcessorProvider result;
			if (!this._providers.TryGetValue(kind, out result))
			{
				IFrameProcessorProvider frameProcessorProvider = new FrameProcessorProvider();
				this._providers[kind] = frameProcessorProvider;
				result = frameProcessorProvider;
			}
			return result;
		}

		private Dictionary<OperationKind, IFrameProcessorProvider> _providers = new Dictionary<OperationKind, IFrameProcessorProvider>();
	}
}
