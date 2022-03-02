using System;
using UniRx;

namespace HeavyMetalMachines.FeaturesToggle.View
{
	public interface IOpenFeatureToggleConfiguration
	{
		IObservable<Unit> Open();
	}
}
