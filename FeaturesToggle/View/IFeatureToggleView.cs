using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.FeaturesToggle.View
{
	public interface IFeatureToggleView
	{
		ILabel NameLabel { get; }

		IToggle Toggle { get; }
	}
}
