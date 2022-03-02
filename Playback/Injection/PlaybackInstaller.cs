using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Counselor;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.DependencyInjection.Installers;
using HeavyMetalMachines.Playback.Snapshot;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Playback.Injection
{
	public class PlaybackInstaller : GameMonoInstaller<PlaybackInstaller>
	{
		protected override void BindBoth()
		{
			base.Container.Bind<IServerPlaybackDispatcher>().To<PlaybackDispatcherLegacyAdapter>().AsSingle();
			base.Container.Bind<ICollisionDispatcher>().To<CollisionDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IBombDetonationDispatcher>().To<BombDetonationDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IGadgetEventDispatcher>().To<GadgetEventDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IGadgetLevelDispatcher>().To<GadgetLevelDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IBombInstanceDispatcher>().To<BombInstanceDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<ICombatFeedbackDispatcher>().To<CombatFeedbackDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<ICombatStatesDispatcher>().To<CombatStatesDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IModifierEventDispatcher>().To<ModifierEventDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<ITransformDispatcher>().To<TransformDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IStatsDispatcher>().To<StatsDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IScoreboardDispatcher>().To<ScoreboardDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<ICounselorDispatcher>().To<CounselorDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IEventManagerDispatcher>().To<EventManagerDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IMatchTeamsDispatcher>().To<MatchTeamsDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IMatchPlayersDispatcher>().To<MatchPlayersDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<IPlayerStatsFeature>().To<StatsDispatcher>().AsSingle().NonLazy();
			base.Container.Bind<ICombatStatesFeature>().To<CombatStatesDispatcher>().AsSingle().NonLazy();
			base.Container.QueueForInject(GameHubBehaviour.Hub.Players);
			base.Container.QueueForInject(GameHubBehaviour.Hub.TeamsDispatcher);
		}

		protected override void BindClient()
		{
			base.Container.Bind<IFrameProcessorFactory>().To<FrameProcessorFactory>().AsSingle().NonLazy();
			IPlayback instance;
			if (this._config.GetBoolValue(ConfigAccess.HORTA))
			{
				instance = base.gameObject.AddComponent<HORTAPlayback>();
				base.Container.Bind<IFeatureSnapshot>().To<ScoreboardSnapshot>().AsSingle();
				base.Container.Bind<IFeatureSnapshot>().To<MatchTeamsSnapshot>().AsSingle();
				base.Container.Bind<IFeatureSnapshot>().To<MatchPlayersSnapshot>().AsSingle();
				base.Container.Bind<IFeatureSnapshot>().To<CombatFeedbacksSnapshot>().AsSingle();
				base.Container.Bind<IFeatureSnapshot>().To<StatsSnapshot>().AsSingle();
				base.Container.Bind<IFeatureSnapshot>().To<CombatStatesSnapshot>().AsSingle();
				base.Container.Bind<IFeatureSnapshot>().To<GadgetLevelSnapshot>().AsSingle();
				base.Container.Bind<IFeatureSnapshot>().To<TransformStatesSnapshot>().AsSingle();
			}
			else
			{
				instance = base.gameObject.AddComponent<GamePlayback>();
			}
			base.Container.Bind<IPlayback>().FromInstance(instance);
			base.Container.QueueForInject(instance);
		}

		protected override void BindServer()
		{
			base.Container.Bind<IFrameProcessorFactory>().To<ServerEmptyProcessorFactory>().AsSingle();
			base.Container.Bind<IPlayback>().To<ServerEmptyPlayback>().AsSingle();
		}

		protected override void BindGameTestMode()
		{
			this.BindClient();
			this.BindBoth();
		}

		[Inject]
		private IConfigLoader _config;
	}
}
