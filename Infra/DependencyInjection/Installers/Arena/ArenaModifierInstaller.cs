using System;
using HeavyMetalMachines.Arena.Business;
using HeavyMetalMachines.Arena.Infra;
using HeavyMetalMachines.Combat.Business;
using HeavyMetalMachines.Combat.Infra;
using HeavyMetalMachines.SensorSystem;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers.Arena
{
	public class ArenaModifierInstaller : ServerMonoInstaller<ArenaModifierInstaller>
	{
		protected override void Bind()
		{
			base.Container.Bind<ICombatStorage>().To<CombatStorage>().AsSingle().Lazy();
			base.Container.Bind<ICombatControllerStorage>().To<CombatControllerStorage>().AsSingle().Lazy();
			base.Container.Bind<ICombatControllerStorageInitializer>().To<CombatControllerStorageInitializer>().AsTransient();
			base.Container.Bind<IArenaModifierStorage>().To<ArenaModifierStorage>().AsSingle().Lazy();
			base.Container.Bind<IArenaModifierInitializer>().To<ArenaModifierInitializer>().AsTransient();
			base.Container.Bind<IModifierService>().To<ModifierService>().AsTransient();
			base.Container.Bind<ISensorContextProvider>().To<SensorContextProviderFromHub>().AsTransient();
		}
	}
}
