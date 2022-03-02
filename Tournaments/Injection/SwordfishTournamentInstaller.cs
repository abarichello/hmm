using System;
using HeavyMetalMachines.SkipSwordfish;
using HeavyMetalMachines.Tournaments.API;
using HeavyMetalMachines.Tournaments.Infra;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Tournaments.Injection
{
	public class SwordfishTournamentInstaller : MonoInstaller<SwordfishTournamentInstaller>
	{
		public override void InstallBindings()
		{
			if (this._config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				base.Container.Bind<ITournamentRankingProvider>().To<SkipSwordfishTournamentRankingProvider>().AsTransient();
				base.Container.Bind<ITournamentProvider>().To<SkipSwordfishTournamentProvider>().AsTransient();
				base.Container.Bind<ITournamentCheckFirstTimeSeeingService>().To<SkipSwordfishTournamentCheckFirstTimeSeeingService>().AsTransient();
				base.Container.Bind<ITeamTournamentSubscriptionService>().To<SkipSwordfishTeamTournamentSubscriptionService>().AsTransient();
				base.Container.Bind<ITournamentTiersProvider>().To<SkipSwordfishTournamentTiersProvider>().AsTransient();
				base.Container.Bind<ITournamentSeasonProvider>().To<SkipSwordfishTournamentSeasonProvider>().AsTransient();
				return;
			}
			if (this._config.GetBoolValue(ConfigAccess.EnableFakeRankingDataHack))
			{
				base.Container.Bind<ITournamentRankingProvider>().To<RandomTournamentRankingProvider>().AsTransient();
			}
			else
			{
				base.Container.Bind<ITournamentRankingProvider>().To<SwordfishTournamentRankingProvider>().AsTransient();
			}
			base.Container.Bind<ITournamentProvider>().To<SwordfishTournamentProvider>().AsTransient();
			base.Container.Bind<ITournamentCheckFirstTimeSeeingService>().To<TournamentCheckFirstTimeSeeingService>().AsTransient();
			base.Container.Bind<ITeamTournamentSubscriptionService>().To<SwordfishTeamTournamentSubscriptionService>().AsTransient();
			base.Container.Bind<ITournamentTiersProvider>().To<SwordfishTournamentTiersProvider>().AsTransient();
			base.Container.Bind<ITournamentSeasonProvider>().To<SwordfishTournamentSeasonProvider>().AsTransient();
		}

		[Inject]
		private IConfigLoader _config;
	}
}
