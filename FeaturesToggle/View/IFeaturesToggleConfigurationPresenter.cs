using System;
using UniRx;

namespace HeavyMetalMachines.FeaturesToggle.View
{
	public interface IFeaturesToggleConfigurationPresenter
	{
		IObservable<Unit> ShowAndWaitForResolution();
	}
}
