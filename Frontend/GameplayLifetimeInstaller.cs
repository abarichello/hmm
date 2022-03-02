using System;
using HeavyMetalMachines.Gameplay;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class GameplayLifetimeInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IObserveGameplayLifetimeState>().To<LegacyObserveGameplayLifetimeState>().AsTransient();
		}
	}
}
