using System;
using HeavyMetalMachines.Options.Presenting;
using Zenject;

namespace HeavyMetalMachines.Options.Installers
{
	public class FakeOptionsInstaller : MonoInstaller<FakeOptionsInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IOptionsPresenter>().To<FakeOptionsPresenter>().AsSingle();
		}
	}
}
