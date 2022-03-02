using System;
using HeavyMetalMachines.Customizations.Skins;
using Zenject;

namespace HeavyMetalMachines.Customizations.Installers
{
	public class CustomizationsSkinsInstaller : MonoInstaller<CustomizationsSkinsInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IGetLocalPlayerAvailableSkins>().To<GetLocalPlayerAvailableSkins>().AsTransient();
		}
	}
}
