using System;
using HeavyMetalMachines.HACKS.Presenters;
using Zenject;

namespace HeavyMetalMachines.HACKS.Installers
{
	public class InventoryInGameToolPresenterInstaller : MonoInstaller<InventoryInGameToolPresenterInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IInventoryInGameToolPresenter>().To<InventoryInGameToolPresenter>().AsTransient();
		}
	}
}
