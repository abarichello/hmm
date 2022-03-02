using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.ToggleableFeatures;

namespace HeavyMetalMachines.FeaturesToggle.Infra
{
	public class EnvironmentConfigurationRemotelyFeaturesProvider : IRemotelyEnabledFeaturesProvider
	{
		public EnvironmentConfigurationRemotelyFeaturesProvider(IRemotelyEnabledFeaturesNamesStorage remotelyEnabledFeaturesNamesStorage)
		{
			this._remotelyEnabledFeaturesNamesStorage = remotelyEnabledFeaturesNamesStorage;
		}

		public List<Feature> GetEnabledFeatures()
		{
			IEnumerable<string> featuresNames = this._remotelyEnabledFeaturesNamesStorage.FeaturesNames;
			if (EnvironmentConfigurationRemotelyFeaturesProvider.<>f__mg$cache0 == null)
			{
				EnvironmentConfigurationRemotelyFeaturesProvider.<>f__mg$cache0 = new Func<string, Feature>(EnvironmentConfigurationRemotelyFeaturesProvider.ConvertToBusiness);
			}
			return featuresNames.Select(EnvironmentConfigurationRemotelyFeaturesProvider.<>f__mg$cache0).ToList<Feature>();
		}

		private static Feature ConvertToBusiness(string enabledFeatureName)
		{
			return new Feature(enabledFeatureName);
		}

		private readonly IRemotelyEnabledFeaturesNamesStorage _remotelyEnabledFeaturesNamesStorage;

		[CompilerGenerated]
		private static Func<string, Feature> <>f__mg$cache0;
	}
}
