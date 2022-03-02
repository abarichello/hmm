using System;
using HeavyMetalMachines.SkipSwordfish;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.ReportSystem.Injection
{
	public class SwordfishReportSystemInstaller : MonoInstaller<SwordfishReportSystemInstaller>
	{
		public override void InstallBindings()
		{
			if (this._config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				base.Container.Bind<IReportCreator>().To<SkipSwordfishReportCreator>().AsTransient();
				return;
			}
			base.Container.Bind<IReportCreator>().To<ReportCreator>().AsTransient();
		}

		[Inject]
		private IConfigLoader _config;
	}
}
