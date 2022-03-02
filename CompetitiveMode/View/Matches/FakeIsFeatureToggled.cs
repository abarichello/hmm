using System;
using Hoplon.ToggleableFeatures;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class FakeIsFeatureToggled : IIsFeatureToggled
	{
		bool IIsFeatureToggled.Check(Feature feature)
		{
			return false;
		}

		bool IIsFeatureToggled.Check(string featureName)
		{
			return false;
		}
	}
}
