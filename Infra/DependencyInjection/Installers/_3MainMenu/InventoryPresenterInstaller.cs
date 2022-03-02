using System;
using HeavyMetalMachines.Inventory.Presenter;
using HeavyMetalMachines.Inventory.Tab.Presenter;
using HeavyMetalMachines.Inventory.Tab.View;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers._3MainMenu
{
	public class InventoryPresenterInstaller : MonoInstaller<MainMenuInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IInventoryPresenter>().To<LegacyInventoryPresenter>().AsTransient();
			base.Container.Bind<IPortraitInventoryTabPresenter>().To<LegacyInventoryArtPreviewTabPresenter<IPortraitInventoryTabView>>().AsTransient();
			base.Container.Bind<IScoreInventoryTabPresenter>().To<LegacyInventoryArtPreviewTabPresenter<IScoreInventoryTabView>>().AsTransient();
			base.Container.Bind<IAvatarsInventoryTabPresenter>().To<LegacyInventoryArtPreviewTabPresenter<IAvatarsInventoryTabView>>().AsTransient();
			base.Container.Bind<IKillsInventoryTabPresenter>().To<LegacyInventoryArtPreviewTabPresenter<IKillsInventoryTabView>>().AsTransient();
			base.Container.Bind<ILoreInventoryTabPresenter>().To<LegacyInventoryArtPreviewTabPresenter<ILoreInventoryTabView>>().AsTransient();
			base.Container.Bind<IRespawnsInventoryTabPresenter>().To<LegacyInventoryArtPreviewTabPresenter<IRespawnsInventoryTabView>>().AsTransient();
			base.Container.Bind<ISkinsInventoryTabPresenter>().To<LegacyInventoryArtPreviewTabPresenter<ISkinsInventoryTabView>>().AsTransient();
			base.Container.Bind<ISpraysInventoryTabPresenter>().To<LegacyInventoryArtPreviewTabPresenter<ISpraysInventoryTabView>>().AsTransient();
			base.Container.Bind<ITakeoffInventoryTabPresenter>().To<LegacyInventoryArtPreviewTabPresenter<ITakeoffInventoryTabView>>().AsTransient();
			base.Container.Bind<IEmotesInventoryTabPresenter>().To<LegacyInventoryRadialMenuTabPresenter<IEmotesInventoryTabView>>().AsTransient();
		}
	}
}
