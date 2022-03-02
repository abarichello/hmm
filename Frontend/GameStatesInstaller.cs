using System;
using HeavyMetalMachines.ApplicationStates;
using HeavyMetalMachines.MainMenuView;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class GameStatesInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IProceedToMainMenu>().To<ProceedToMainMenu>().AsTransient();
			base.Container.Bind<IProceedToReconnectState>().To<ProceedToReconnectState>().AsTransient();
			base.Container.Bind<IProceedToCreateProfileState>().To<ProceedToCreateProfileState>().AsTransient();
			base.Container.Bind<IProceedToTutorialState>().To<ProceedToTutorialState>().AsTransient();
		}
	}
}
