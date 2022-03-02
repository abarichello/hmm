using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.FeaturesToggle.View
{
	public interface IFeaturesToggleView
	{
		IButton ContinueButton { get; }

		IFeatureToggleView CreateFeatureView();
	}
}
