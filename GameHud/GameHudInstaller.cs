using System;
using Assets.Standard_Assets.Scripts.HMM.GameStates.Game.newChat.Impl.Adapter;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;
using HeavyMetalMachines.RadialMenu.View;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.GameHud
{
	public class GameHudInstaller : ClientMonoInstaller<GameHudInstaller>
	{
		protected override void Bind()
		{
			base.Container.Bind<IEmotesMenuPresenter>().To<EmotesMenuPresenter>().AsTransient();
			base.Container.Bind<ICurrentPlayerController>().To<CurrentPlayerControllerWrapper>().AsTransient();
			base.Container.Bind<IChatService>().FromMethod((InjectContext context) => GameHubBehaviour.Hub.Chat).Lazy();
		}
	}
}
