using System;
using HeavyMetalMachines.BackendCommunication;
using HeavyMetalMachines.DependencyInjection;
using HeavyMetalMachines.Login.DataTransferObjects;
using HeavyMetalMachines.Swordfish;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Login
{
	public class BackendLifeCycleInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			bool boolValue = this._config.GetBoolValue(ConfigAccess.SkipSwordfish);
			if (boolValue)
			{
				base.Container.Bind<IBackendLoginService>().To<SkipSwordfishBackendLoginService>().AsTransient();
				base.Container.Bind<IBackendLogoutService>().To<SkipSwordfishBackendLogoutService>().AsTransient();
			}
			else
			{
				base.Container.Bind<IBackendLoginService>().To<SwordfishBackendLoginService>().AsTransient();
				base.Container.Bind<IBackendLogoutService>().To<SwordfishBackendLogoutService>().AsTransient();
			}
			base.Container.Bind<IGenerateClientLoginRequest>().To<GenerateClientLoginRequest>().AsTransient();
			base.Container.Bind<IInitializeLegacyServices>().To<InitializeLegacyServices>().AsTransient();
			base.Container.Bind<IUserAccessControlService>().To<SwordfishUserAccessControlService>().AsTransient();
			ZenjectInjectionBinder zenjectInjectionBinder = new ZenjectInjectionBinder(base.Container);
			new BackendCommunicationModule(zenjectInjectionBinder).Bind();
			new BackendLifeCycleModule(zenjectInjectionBinder).Bind();
		}

		[Inject]
		private IConfigLoader _config;
	}
}
