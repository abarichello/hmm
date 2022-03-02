using System;
using Assets.Standard_Assets.Scripts.HMM.Swordfish;
using HeavyMetalMachines.Swordfish.API;
using Zenject;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishServicesInstaller : MonoInstaller<SwordfishServicesInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ISwordfishLogProvider>().To<SwordfishLogProvider>().AsTransient();
			base.Container.Bind<ICustomWsService>().To<CustomWsService>().AsTransient();
			base.Container.Bind<ISwordfishWsService>().To<SwordfishWsService>().AsTransient();
		}
	}
}
