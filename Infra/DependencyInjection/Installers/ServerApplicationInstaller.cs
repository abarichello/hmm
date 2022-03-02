using System;
using HeavyMetalMachines.Common;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class ServerApplicationInstaller : MonoInstaller<ServerApplicationInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ICheckApplicationType>().To<ServerCheckApplicationType>().AsTransient();
		}
	}
}
