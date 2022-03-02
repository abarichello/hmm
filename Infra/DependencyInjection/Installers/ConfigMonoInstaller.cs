using System;
using HeavyMetalMachines.Configuring;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class ConfigMonoInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IInitializeConfigLoader>().To<InitializeConfigLoader>().AsTransient();
			base.Container.Bind<IConfigLoader>().To<ConfigLoader>().AsSingle();
			base.Container.Bind<IDevelopmentConfiguration>().FromMethod((InjectContext _) => (ConfigLoader)_.Container.Resolve<IConfigLoader>());
			base.Container.Resolve<IInitializeConfigLoader>().Initialize();
		}
	}
}
