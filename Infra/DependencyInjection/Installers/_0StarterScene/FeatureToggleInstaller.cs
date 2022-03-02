using System;
using HeavyMetalMachines.FeaturesToggle.Infra;
using HeavyMetalMachines.FeaturesToggle.View;
using HeavyMetalMachines.Presenting.ToggleableFeatures;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using UniRx;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers._0StarterScene
{
	public class FeatureToggleInstaller : MonoInstaller<FeatureToggleInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IInitializeToggleableFeatures>().To<InitializeToggleableFeatures>().AsTransient();
			base.Container.Bind<IInitializeRemotelyToggleableFeatures>().To<InitializeRemotelyToggleableFeatures>().AsTransient();
			base.Container.Bind<IIsFeatureToggled>().To<IsFeatureToggled>().AsTransient();
			base.Container.Bind<IFeatureToggleStorage>().To<FeaturesToggleStorage>().AsSingle();
			base.Container.Bind<IToggleableFeaturesProvider>().To<ConfigAccessToggleableFeaturesProvider>().AsTransient();
			base.Container.Bind<IDisableLabelForFeatureNotEnable>().To<DisableLabelForFeatureNotEnable>().AsTransient();
			base.Container.Bind<IOpenFeatureToggleConfiguration>().To<OpenFeatureToggleConfiguration>().AsTransient();
			base.Container.Bind<IFeaturesToggleConfigurationPresenter>().To<FeaturesToggleConfigurationPresenter>().AsTransient();
			base.Container.Bind<IRemotelyEnabledFeaturesNamesStorage>().To<RemotelyEnabledFeaturesNamesStorage>().AsSingle();
			IInitializeToggleableFeatures initializeToggleableFeatures = base.Container.Resolve<IInitializeToggleableFeatures>();
			ObservableExtensions.Subscribe<Unit>(initializeToggleableFeatures.Initialize());
			this.BindIRemotelyToggleableFeaturesProvider();
		}

		private void BindIRemotelyToggleableFeaturesProvider()
		{
			if (this._config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				base.Container.Bind<IRemotelyEnabledFeaturesProvider>().To<SkipSwordfishRemotelyEnabledFeaturesProvider>().AsTransient();
			}
			else
			{
				base.Container.Bind<IRemotelyEnabledFeaturesProvider>().To<EnvironmentConfigurationRemotelyFeaturesProvider>().AsTransient();
			}
		}

		[Inject]
		private IConfigLoader _config;
	}
}
