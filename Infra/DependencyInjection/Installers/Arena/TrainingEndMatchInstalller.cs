using System;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Training.Presenter;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers.Arena
{
	public class TrainingEndMatchInstalller : ClientMonoInstaller<TrainingEndMatchInstalller>
	{
		protected override void Bind()
		{
			base.Container.Bind<ITrainingMatchResultPresenter>().To<TrainingMatchResultPresenter>().AsTransient();
			base.Container.Bind<IMatchWinProvider>().To<MatchWinProvideFromHub>().AsTransient();
		}
	}
}
