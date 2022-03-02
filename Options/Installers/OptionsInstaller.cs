using System;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.ShopPopup.Presenting.Business;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Options.Installers
{
	public class OptionsInstaller : MonoInstaller<OptionsInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IOptionsPresenter>().FromMethod((InjectContext context) => GameHubBehaviour.Hub.GuiScripts.Esc).Lazy();
			base.Container.Bind<IGetShopPopupVisibility>().To<GetShopPopupVisibility>().AsSingle();
		}
	}
}
