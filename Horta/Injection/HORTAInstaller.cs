using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Horta.View;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Horta.Injection
{
	public class HORTAInstaller : ClientMonoInstaller<HORTAInstaller>
	{
		protected override void Bind()
		{
			base.Container.Bind<HORTAComponent>().AsSingle();
			base.Container.Bind<IHORTATimelinePresenter>().To<HORTATimelinePresenter>().AsTransient();
			if (this._config.GetBoolValue(ConfigAccess.HORTA))
			{
			}
		}

		[Inject]
		private IConfigLoader _config;
	}
}
