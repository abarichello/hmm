using System;
using HeavyMetalMachines.Publishing;
using Pocketverse;

namespace HeavyMetalMachines.Login
{
	public class SkipSwordfishPublisherService : IGetCurrentPublisher
	{
		public SkipSwordfishPublisherService(IConfigLoader configLoader)
		{
			this._configLoader = configLoader;
		}

		public Publisher Get()
		{
			string value = this._configLoader.GetValue(ConfigAccess.ForcedFakePublisher);
			if (string.IsNullOrEmpty(value))
			{
				return Publishers.Steam;
			}
			return Publishers.GetPublisherByName(value);
		}

		private readonly IConfigLoader _configLoader;
	}
}
