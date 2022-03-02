using System;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.CompetitiveMode;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.DependencyInjection;
using HeavyMetalMachines.Localization.Business;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.MatchMaking;
using HeavyMetalMachines.Matchmaking.Queue;
using HeavyMetalMachines.OpenUrl;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.FeedbackWindow;
using HeavyMetalMachines.Presenting.GenericConfirmWindow;
using HeavyMetalMachines.Regions.Business;
using HeavyMetalMachines.Social.Teams.Business;
using HeavyMetalMachines.Storyteller.Presenting;
using HeavyMetalMachines.Tournament.View.MainWindow.Team;
using HeavyMetalMachines.Tournaments.API;
using HeavymetalMachines.Tournaments.Infra;
using HeavyMetalMachines.Tournaments.Presenting;
using HeavyMetalMachines.Tournaments.Presenting.Detailing;
using HeavyMetalMachines.Tournaments.Presenting.Detailing.Rankings;
using HeavyMetalMachines.Tournaments.Presenting.Detailing.Schedules;
using HeavyMetalMachines.Tournaments.Presenting.Informing;
using HeavyMetalMachines.Tournaments.Ranking;
using Hoplon.Input;
using Hoplon.Localization.TranslationTable;
using Hoplon.Logging;
using Zenject;

namespace HeavyMetalMachines.Tournaments.Injection
{
	public class TournamentInstaller : MonoInstaller<TournamentInstaller>
	{
		public override void InstallBindings()
		{
			ZenjectInjectionBinder zenjectBinder = new ZenjectInjectionBinder(base.Container);
			base.Container.Bind<StorytellerPresenter>().AsTransient();
			base.Container.Bind<IIsTeamSubscribedInTournamentService>().To<IsTeamSubscribedInTournamentService>().AsTransient();
			base.Container.Bind<ITournamentGetRanking>().To<TournamentGetRanking>().AsTransient();
			base.Container.Bind<ITournamentGetRankingPosition>().To<TournamentGetRankingPosition>().AsTransient();
			base.Container.Bind<ITournamentMarkSeen>().To<TournamentMarkSeen>().AsTransient();
			base.Container.Bind<ITournamentCheckFirstTimeSeeing>().To<TournamentCheckFirstTimeSeeing>().AsTransient();
			base.Container.Bind<IMatchmakingTournamentQueueJoin>().To<MatchmakingTournamentTournamentQueueJoin>().AsTransient();
			base.Container.Bind<TournamentPresenterParameters>().FromMethod((InjectContext context) => new TournamentPresenterParameters
			{
				ViewLoader = context.Container.Resolve<IViewLoader>(),
				ViewProvider = context.Container.Resolve<IViewProvider>(),
				Translation = context.Container.Resolve<ILocalizeKey>(),
				FeedbackWindowService = context.Container.Resolve<IFeedbackWindowService>(),
				CountdownDisplayGenerator = context.Container.Resolve<ICountdownDisplayGenerator>(),
				MainMenuPresenterTree = context.Container.Resolve<IMainMenuPresenterTree>(),
				HomePresenter = context.Container.Resolve<ITournamentHomePresenter>(),
				RankingPresenter = context.Container.Resolve<ITournamentRankingPresenter>(),
				InfoPresenter = context.Container.Resolve<ITournamentInfoPresenter>(),
				TeamPresenter = context.Container.Resolve<ITournamentTeamPresenter>(),
				CheckTournamentJoinability = context.Container.Resolve<ICheckTournamentJoinability>(),
				GetCurrentTournament = context.Container.Resolve<IGetCurrentTournament>(),
				GetTournamentStatus = context.Container.Resolve<IGetTournamentStatus>(),
				TournamentMarkSeen = context.Container.Resolve<ITournamentMarkSeen>(),
				TournamentCheckFirstTimeSeeing = context.Container.Resolve<ITournamentCheckFirstTimeSeeing>(),
				MatchmakingTournamentQueueJoin = context.Container.Resolve<IMatchmakingTournamentQueueJoin>(),
				GetTournamentTier = context.Container.Resolve<IGetTournamentTier>(),
				ButtonBiLogger = context.Container.Resolve<IClientButtonBILogger>(),
				CompetitiveDivisionsBadgeNameBuilder = context.Container.Resolve<ICompetitiveDivisionsBadgeNameBuilder>(),
				Logger = context.Container.Resolve<ILogger<TournamentPresenter>>(),
				StepsStatusViewUpdater = context.Container.Resolve<ITournamentStepsStatusViewUpdater>(),
				SubscribeToTournament = context.Container.Resolve<ISubscribeToTournament>(),
				UnsubscribeFromTournament = context.Container.Resolve<IUnsubscribeFromTournament>(),
				ConfirmWindowPresenter = context.Container.Resolve<IGenericConfirmWindowPresenter>(),
				OpenUrl = context.Container.Resolve<IOpenUrl>(),
				GetRegions = context.Container.Resolve<IGetRegions>(),
				GetLocalPlayer = context.Container.Resolve<IGetLocalPlayer>(),
				GetLocalPlayerTeam = context.Container.Resolve<IGetLocalPlayerTeam>(),
				ObserveLocalPlayerTeamChanges = context.Container.Resolve<IObserveLocalPlayerTeamChanges>(),
				GetThenObserveMatchmakingQueueState = context.Container.Resolve<IGetThenObserveMatchmakingQueueState>(),
				GetActiveDevice = context.Container.Resolve<IInputGetActiveDevicePoller>(),
				ObserveCrossplayChange = context.Container.Resolve<IObserveCrossplayChange>(),
				GetUgcRestrictionIsEnabled = context.Container.Resolve<IGetUGCRestrictionIsEnabled>(),
				UgcRestrictionDialogPresenter = context.Container.Resolve<IUGCRestrictionDialogPresenter>(),
				IsCrossplayEnabled = context.Container.Resolve<IIsCrossplayEnabled>(),
				GetLocalizedCrossplayActivation = context.Container.Resolve<IGetLocalizedCrossplayActivation>(),
				OpenUrlUgcRestricted = context.Container.Resolve<IOpenUrlUgcRestricted>()
			}).AsTransient();
			TournamentInstaller.BindTournamentSystem(zenjectBinder);
			this.LogInstallFinished();
		}

		private static void BindTournamentSystem(ZenjectInjectionBinder zenjectBinder)
		{
			TournamentSystem tournamentSystem = new TournamentSystem(zenjectBinder);
			tournamentSystem.Bind();
		}

		private void LogInstallFinished()
		{
			this._logger.Info("Finished installing TournamentInstaller.");
		}

		[Inject]
		private ILogger<TournamentInstaller> _logger;
	}
}
