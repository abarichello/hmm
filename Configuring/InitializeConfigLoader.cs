using System;
using Pocketverse;

namespace HeavyMetalMachines.Configuring
{
	public class InitializeConfigLoader : IInitializeConfigLoader
	{
		public InitializeConfigLoader(IConfigLoader configLoader)
		{
			this._configLoader = configLoader;
		}

		public void Initialize()
		{
			this._configLoader.Initialize();
		}

		private readonly IConfigLoader _configLoader;
	}
}
