using System;
using System.Collections.Generic;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using UniRx;

namespace HeavyMetalMachines.FeaturesToggle.Infra
{
	public class ConfigAccessToggleableFeaturesProvider : IToggleableFeaturesProvider
	{
		public ConfigAccessToggleableFeaturesProvider(IConfigLoader configLoader)
		{
			this._configLoader = configLoader;
		}

		public IObservable<List<Feature>> GetToggleableFeaturesFlags()
		{
			List<Feature> list = new List<Feature>();
			list.Add(Features.Drafter);
			if (this._configLoader.GetBoolValue(ConfigAccess.FMODLiveUpdate))
			{
				list.Add(Features.FmodLiveUpdate);
			}
			return Observable.Return<List<Feature>>(list);
		}

		private readonly IConfigLoader _configLoader;
	}
}
