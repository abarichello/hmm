using System;
using HeavyMetalMachines.LegacyStorage;
using HeavyMetalMachines.PeriodicRefresh;
using HeavyMetalMachines.PeriodicRefresh.API;
using HeavyMetalMachines.PeriodicRefresh.Infra;
using HeavyMetalMachines.ReportSystem.Infra;
using HeavyMetalMachines.SkipSwordfish;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishMainMenuInstaller : MonoInstaller<SwordfishMainMenuInstaller>
	{
		public override void InstallBindings()
		{
			if (this._config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				base.Container.Bind<IMarkAsReadPlayerFeedbacks>().To<SkipSwordfishMarkAsReadFeedback>().AsTransient();
				base.Container.Bind<IPeriodicRefreshDataService>().To<SkipSwordfishPeriodicRefreshDataService>().AsTransient();
			}
			else
			{
				base.Container.Bind<IMarkAsReadPlayerFeedbacks>().To<MarkAsReadPlayerFeedbacks>().AsTransient();
				base.Container.Bind<IPeriodicRefreshDataService>().To<SwordfishPeriodicRefreshDataService>().AsTransient();
			}
			base.Container.Bind<IPeriodicRefreshService>().To<PeriodicRefreshService>().AsTransient();
			base.Container.Bind<MainMenuDataStorage>().AsSingle();
		}

		[Inject]
		private IConfigLoader _config;
	}
}
