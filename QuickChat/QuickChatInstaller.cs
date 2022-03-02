using System;
using HeavyMetalMachines.RadialMenu.View;
using Zenject;

namespace HeavyMetalMachines.QuickChat
{
	public class QuickChatInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IQuickChatMenuPresenter>().To<QuickChatMenuPresenter>().AsTransient();
			base.Container.Bind<ISendGadgetInputCommand>().To<SendGadgetInputCommand>().AsTransient();
			base.Container.Bind<ICanShowInGameOverlay>().To<CanShowInGameOverlay>().AsTransient();
		}
	}
}
