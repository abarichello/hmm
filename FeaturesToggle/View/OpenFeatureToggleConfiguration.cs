using System;
using Hoplon.Reactive;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.FeaturesToggle.View
{
	public class OpenFeatureToggleConfiguration : IOpenFeatureToggleConfiguration
	{
		public OpenFeatureToggleConfiguration(IFeaturesToggleConfigurationPresenter featuresToggleConfigurationPresenter)
		{
			this._featuresToggleConfigurationPresenter = featuresToggleConfigurationPresenter;
		}

		public IObservable<Unit> Open()
		{
			IObservable<bool>[] array = new IObservable<bool>[2];
			array[0] = Observable.Select<Unit, bool>(OpenFeatureToggleConfiguration.WaitTimer(), (Unit _) => false);
			array[1] = Observable.Select<Unit, bool>(OpenFeatureToggleConfiguration.CheckButtonPress(), (Unit _) => true);
			return ObservableExtensions.IfElse<bool, Unit>(Observable.First<bool>(Observable.Merge<bool>(array)), (bool openConfiguration) => openConfiguration, (bool _) => this.OpenPresenter(), (bool _) => Observable.ReturnUnit());
		}

		private static IObservable<Unit> WaitTimer()
		{
			return Observable.AsUnitObservable<long>(Observable.Timer(TimeSpan.FromSeconds(2.0)));
		}

		private static IObservable<Unit> CheckButtonPress()
		{
			return Observable.AsUnitObservable<long>(Observable.First<long>(Observable.EveryUpdate(), (long _) => Input.GetKeyDown(127)));
		}

		private IObservable<Unit> OpenPresenter()
		{
			return this._featuresToggleConfigurationPresenter.ShowAndWaitForResolution();
		}

		private readonly IFeaturesToggleConfigurationPresenter _featuresToggleConfigurationPresenter;
	}
}
