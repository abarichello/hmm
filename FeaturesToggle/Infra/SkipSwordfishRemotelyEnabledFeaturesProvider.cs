using System;
using System.Collections.Generic;
using Hoplon.ToggleableFeatures;

namespace HeavyMetalMachines.FeaturesToggle.Infra
{
	public class SkipSwordfishRemotelyEnabledFeaturesProvider : IRemotelyEnabledFeaturesProvider
	{
		public List<Feature> GetEnabledFeatures()
		{
			return new List<Feature>();
		}
	}
}
