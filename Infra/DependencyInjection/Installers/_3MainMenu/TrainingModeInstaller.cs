using System;
using HeavyMetalMachines.Matchmaking.Queue;
using HeavyMetalMachines.Training.Business;
using HeavyMetalMachines.Training.Presenter;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers._3MainMenu
{
	public class TrainingModeInstaller : MonoInstaller<TrainingModeInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<ITrainingModesBusinessFactory>().To<TrainingModesBusinessFactory>().AsTransient();
			base.Container.Bind<IMatchmakingMatchCreate>().To<MatchmakingMatchCreate>().AsTransient();
			base.Container.Bind<ITrainingSelectionScreenPresenter>().To<TrainingSelectionScreenPresenter>().AsTransient();
			base.Container.Bind<ITrainingPopUpPresenter>().To<TrainingPopUpPresenter>().AsTransient();
			base.Container.Bind<ITrainingPopUpPresenterV3>().To<TrainingPopUpPresenterV3>().AsTransient();
			base.Container.Bind<ITrainingPopUpRules>().To<TrainingPopUpRules>().AsTransient();
			base.Container.Bind<ITrainingPopUpRulesV3>().To<TrainingPopUpRulesV3>().AsTransient();
		}
	}
}
