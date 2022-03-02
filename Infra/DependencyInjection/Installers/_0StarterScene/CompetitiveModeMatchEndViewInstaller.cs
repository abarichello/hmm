using System;
using HeavyMetalMachines.CompetitiveMode.View.Matches;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers._0StarterScene
{
	public class CompetitiveModeMatchEndViewInstaller : MonoInstaller<CompetitiveModeMatchEndViewInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ICompetitiveMatchResultPresenter>().To<CompetitiveMatchResultPresenter>().AsTransient();
			base.Container.Bind<ICompetitiveUnlockPresenter>().To<CompetitiveUnlockPresenter>().AsTransient();
		}
	}
}
