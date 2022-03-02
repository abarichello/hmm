using System;
using HeavyMetalMachines.Swordfish.Session;
using Zenject;

namespace HeavyMetalMachines.Swordfish
{
	public class FakeLoginSessionInstaller : MonoInstaller<FakeLoginSessionInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ILoginSessionIdProvider>().To<FakeLoginSessionIdProvider>().AsTransient();
		}
	}
}
