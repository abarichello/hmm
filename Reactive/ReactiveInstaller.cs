using System;
using Hoplon.Reactive;
using Zenject;

namespace HeavyMetalMachines.Reactive
{
	public class ReactiveInstaller : MonoInstaller<ReactiveInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IGameObservable>().To<GameObservable>().AsSingle();
		}
	}
}
