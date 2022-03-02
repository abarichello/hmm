using System;
using HeavyMetalMachines.Common;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class ClientApplicationInstaller : MonoInstaller<ClientApplicationInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ICheckApplicationType>().To<ClientCheckApplicationType>().AsTransient();
		}
	}
}
