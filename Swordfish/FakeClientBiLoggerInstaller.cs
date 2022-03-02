using System;
using HeavyMetalMachines.BI;
using Zenject;

namespace HeavyMetalMachines.Swordfish
{
	public class FakeClientBiLoggerInstaller : MonoInstaller<FakeClientBiLoggerInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IClientBILogger>().To<FakeClientBILogger>().AsTransient().Lazy();
			base.Container.Bind<IClientButtonBILogger>().To<FakeClientButtonBILogger>().AsSingle().Lazy();
		}
	}
}
