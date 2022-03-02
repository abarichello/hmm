using System;
using HeavyMetalMachines.Publishing;
using Pocketverse;

namespace HeavyMetalMachines.Login
{
	public class ForcedPublisherService : IGetCurrentPublisher
	{
		public ForcedPublisherService(IConfigLoader config)
		{
			this._config = config;
		}

		public Publisher Get()
		{
			return Publishers.GetPublisherByName(this._config.GetValue(ConfigAccess.ForcedFakePublisher));
		}

		private readonly IConfigLoader _config;
	}
}
