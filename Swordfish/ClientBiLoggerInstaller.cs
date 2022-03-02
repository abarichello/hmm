using System;
using HeavyMetalMachines.BI;
using Zenject;

namespace HeavyMetalMachines.Swordfish
{
	public class ClientBiLoggerInstaller : MonoInstaller<ClientBiLoggerInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IClientBILogger>().To<ClientBILogger>().AsTransient().Lazy();
			base.Container.Bind<IUnidentifiedPlayerBiLogger>().To<UnidentifiedPlayerBiLogger>().AsTransient().Lazy();
			base.Container.Bind<IClientButtonBILogger>().To<ClientButtonBILogger>().AsSingle().Lazy();
			base.Container.Bind<IClientShopBILogger>().To<ClientShopBILogger>().AsSingle().Lazy();
		}
	}
}
