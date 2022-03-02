using System;
using HeavyMetalMachines.Tutorials;
using Standard_Assets.Scripts.HMM.Tutorial;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers
{
	public class TutorialInstaller : ClientMonoInstaller<TutorialInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ICheckPlayerHasDoneTutorial>().To<CheckPlayerHasDoneTutorial>().AsTransient();
			base.Container.Bind<IShouldPlayTutorial>().To<LegacyShouldPlayTutorial>().AsTransient();
		}
	}
}
