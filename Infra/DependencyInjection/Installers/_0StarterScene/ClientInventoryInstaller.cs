using System;
using HeavyMetalMachines.Inventory.Business;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers._0StarterScene
{
	public class ClientInventoryInstaller : ClientMonoInstaller<ClientHubMonoInstaller>
	{
		protected override void Bind()
		{
			base.Container.Bind<IGetItemFromCollection>().To<LegacyGetItemFromCollection>().AsTransient();
		}
	}
}
