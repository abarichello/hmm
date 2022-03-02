using System;
using HeavyMetalMachines.Swordfish.Session;
using Zenject;

namespace HeavyMetalMachines.Swordfish
{
	public class LoginSessionInstaller : MonoInstaller<LoginSessionInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ILoginSessionIdProvider>().To<LoginSessionIdProvider>().AsTransient();
		}
	}
}
