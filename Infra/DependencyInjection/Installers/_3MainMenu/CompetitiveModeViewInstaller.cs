using System;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.View;
using HeavyMetalMachines.CompetitiveMode.View.Divisions;
using HeavyMetalMachines.CompetitiveMode.View.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.View.Prizes;
using HeavyMetalMachines.CompetitiveMode.View.Ranking;
using HeavyMetalMachines.Matchmaking.Queue;
using Zenject;

namespace HeavyMetalMachines.Infra.DependencyInjection.Installers._3MainMenu
{
	public class CompetitiveModeViewInstaller : MonoInstaller<CompetitiveModeViewInstaller>
	{
		public override void InstallBindings()
		{
			base.Container.Bind<IGetThenObservePlayerCompetitiveJoinability>().To<GetThenObservePlayerCompetitiveJoinability>().AsTransient();
			base.Container.Bind<IContinuouslyCheckAndCancelCompetitiveMatchSearch>().To<ContinuouslyCheckAndCancelCompetitiveMatchSearch>().AsTransient();
			base.Container.Bind<ICompetitiveModePresenter>().To<CompetitiveModePresenter>().AsTransient();
			base.Container.Bind<ICompetitiveRankingPresenter>().To<CompetitiveRankingPresenter>().AsTransient();
			base.Container.Bind<ICompetitiveRankingListPresenter>().To<CompetitiveRankingListPresenter>().AsTransient();
			base.Container.Bind<ICompetitiveDivisionsPresenter>().To<CompetitiveDivisionsPresenter>().AsTransient();
			base.Container.Bind<ICompetitiveQueuePeriodsPresenter>().To<CompetitiveQueuePeriodsPresenter>().AsTransient();
			base.Container.Bind<ICompetitiveQueueJoinPresenter>().To<CompetitiveQueueJoinPresenter>().AsTransient();
			base.Container.Bind<ICompetitiveRewardsPresenter>().To<CompetitiveRewardsPresenter>().AsTransient();
			base.Container.Bind<ICompetitiveSeasonRewardsCollectionPresenter>().To<CompetitiveSeasonRewardsCollectionPresenter>().AsTransient();
			base.Container.Bind<ISearchCompetitiveMatch>().To<LegacySearchCompetitiveMatch>().AsTransient();
		}
	}
}
