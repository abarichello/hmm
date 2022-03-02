using System;
using HeavyMetalMachines.Arenas;
using HeavyMetalMachines.CharacterSelection.Client.Presenting;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.API;
using HeavyMetalMachines.CharacterSelection.Client.Skins;
using HeavyMetalMachines.CharacterSelection.Server;
using HeavyMetalMachines.CharacterSelection.Server.API;
using HeavyMetalMachines.CharacterSelection.Server.Bots;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.CharacterSelection.Server.Swordfish;
using HeavyMetalMachines.DependencyInjection;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Login;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Matches.API;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Publishing;
using Hoplon.Input.Business;
using Infra;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class EditorCharacterSelectionClientInstaller : MonoInstaller<EditorCharacterSelectionClientInstaller>
	{
		public override void InstallBindings()
		{
			ZenjectInjectionBinder zenjectInjectionBinder = new ZenjectInjectionBinder(base.Container);
			new CharacterSelectionServerModule(zenjectInjectionBinder).Bind();
			new CharacterSelectionServerBotsModule(zenjectInjectionBinder).Bind();
			new CharacterSelectionLocalCommunicationModule(zenjectInjectionBinder, new ZenjectInjectionResolver(base.Container)).Bind();
			base.Container.Bind<IExecuteCharacterSelection>().To<ExecuteServerCharacterSelection>().AsTransient();
			DefaultCharacterSelectionConfigurationProvider instance = new DefaultCharacterSelectionConfigurationProvider(TimeSpan.FromSeconds((double)this._banStepResultDurationSeconds), TimeSpan.FromSeconds((double)this._banStepDurationSeconds), TimeSpan.FromSeconds((double)this._simultaneousPickStepDurationSeconds), TimeSpan.FromSeconds((double)this._alternatePickStepDurationSeconds), TimeSpan.FromSeconds((double)this._trainingPickStepDurationSeconds), TimeSpan.FromSeconds((double)this._wrapupStepDurationSeconds));
			EditorCharacterSelectionClientInstaller.ManualObserveClientsConnection instance2 = new EditorCharacterSelectionClientInstaller.ManualObserveClientsConnection();
			base.Container.Bind<IObserveClientsConnection>().FromInstance(instance2);
			base.Container.Bind<EditorCharacterSelectionClientInstaller.ManualObserveClientsConnection>().FromInstance(instance2);
			base.Container.Bind<IInitializeClientsCommunication>().To<EditorCharacterSelectionClientInstaller.EditorClientsInitialization>().AsTransient();
			base.Container.Bind<ICharacterSelectionConfigurationsProvider>().FromInstance(instance);
			base.Container.Bind<IValidatePlayerChoicesAndPersist>().To<SkipValidatePlayerChoicesAndPersist>().AsTransient();
			base.Container.Bind<IGetLocalPlayer>().To<EditorCharacterSelectionClientInstaller.EditorGetLocalPlayer>().AsTransient();
			base.Container.Bind<IProceedToClientGameState>().To<EditorCharacterSelectionClientInstaller.EditorProceedToClientGameState>().AsTransient();
			base.Container.Bind<IEndSession>().To<EditorCharacterSelectionClientInstaller.EmptyEndSession>().AsTransient();
			base.Container.Bind<IGetCurrentMatchCharactersAvailability>().To<EditorCharacterSelectionServerInstaller.GetRandomCurrentMatchCharactersAvailability>().AsTransient();
			base.Container.Bind<IHMMPlayerPrefs>().To<EmptyHmmPlayerPrefs>().AsTransient();
			base.Container.Bind<IExecuteLocalClientCharacterSelection>().To<EditorCharacterSelectionClientInstaller.EditorInitializeCharacterSelection>().AsSingle();
			base.Container.Bind<ExecuteLocalClientCharacterSelection>().To<ExecuteLocalClientCharacterSelection>().AsSingle();
			base.Container.Bind<ManualInitializeLocalization>().AsSingle();
			base.Container.Bind<CharacterSelectionBotsConfiguration>().FromMethod((InjectContext _) => new CharacterSelectionBotsConfiguration
			{
				VoteForRandomBans = this._shouldBotsVoteForBans
			});
			PreStart.InitializeNativePlugins();
			base.Container.Bind<IInitializable>().To<EditorCharacterSelectionClientInstaller.EditorInputInitializable>().AsSingle();
			base.Container.Bind<IGetLocalEquippedSkin>().To<LegacyGetLocalEquippedSkin>().AsTransient();
			base.Container.Bind<IGetArenaData>().FromInstance(new GetArenaData(this._gameArenaConfig));
			base.Container.Bind<IGetCurrentPublisher>().To<ForcedPublisherService>().AsTransient();
			base.Container.Bind<IGetFormattedPlayerTag>().To<GetFormattedPlayerTag>().AsTransient();
		}

		[SerializeField]
		private float _banStepDurationSeconds = 10f;

		[SerializeField]
		private float _banStepResultDurationSeconds = 5f;

		[SerializeField]
		private float _simultaneousPickStepDurationSeconds = 10f;

		[SerializeField]
		private float _alternatePickStepDurationSeconds = 10f;

		[SerializeField]
		private float _trainingPickStepDurationSeconds = 9999f;

		[SerializeField]
		private float _wrapupStepDurationSeconds = 10f;

		[SerializeField]
		private bool _shouldBotsVoteForBans = true;

		[SerializeField]
		private GameArenaConfig _gameArenaConfig;

		public class EmptyEndSession : IEndSession
		{
			public void End(string reason)
			{
			}

			public void End(string reason, IObservable<Unit> additionalLoading)
			{
			}
		}

		public class ManualObserveClientsConnection : IObserveClientsConnection
		{
			public IObservable<MatchClient> OnClientDisconnected()
			{
				return this._onClientDisconnectedSubject;
			}

			public IObservable<MatchClient> OnClientReconnected()
			{
				return this._onClientReconnectedSubject;
			}

			public void SimulateClientDisconnected(MatchClient client)
			{
				this._onClientDisconnectedSubject.OnNext(client);
			}

			public void SimulateClientReconnected(MatchClient client)
			{
				this._onClientReconnectedSubject.OnNext(client);
			}

			private readonly Subject<MatchClient> _onClientDisconnectedSubject = new Subject<MatchClient>();

			private readonly Subject<MatchClient> _onClientReconnectedSubject = new Subject<MatchClient>();
		}

		public class EditorInputInitializable : IInitializable
		{
			public EditorInputInitializable(IInputInitializer inputInitializer, ManualInitializeLocalization manualInitializeLocalization)
			{
				this._inputInitializer = inputInitializer;
				this._manualInitializeLocalization = manualInitializeLocalization;
			}

			public void Initialize()
			{
				this._inputInitializer.Initialize();
				this._manualInitializeLocalization.Language = LanguageCode.PT;
				this._manualInitializeLocalization.Initialize();
			}

			private readonly IInputInitializer _inputInitializer;

			private readonly ManualInitializeLocalization _manualInitializeLocalization;
		}

		private class EditorClientsInitialization : IInitializeClientsCommunication
		{
			public IDisposable Initialize()
			{
				return Disposable.Empty;
			}
		}

		private class EditorInitializeCharacterSelection : IExecuteLocalClientCharacterSelection
		{
			public EditorInitializeCharacterSelection(IInitializeCharacterSelectionConfiguration initializeCharacterSelectionConfiguration, IExecuteCharacterSelection executeCharacterSelection, IExecuteBotsCharacterSelection executeBotsCharacterSelection, ExecuteLocalClientCharacterSelection executeLocalClientCharacterSelection, DiContainer container)
			{
				this._initializeCharacterSelectionConfiguration = initializeCharacterSelectionConfiguration;
				this._executeCharacterSelection = executeCharacterSelection;
				this._executeBotsCharacterSelection = executeBotsCharacterSelection;
				this._executeLocalClientCharacterSelection = executeLocalClientCharacterSelection;
				this._container = container;
			}

			public IObservable<Unit> Initialize()
			{
				return Observable.ReturnUnit();
			}

			public IObservable<Unit> Execute()
			{
				GameObject gameObject = new GameObject("EditorCharacterSelectionInput");
				EditorLocalClientSelectionInput input = this._container.InstantiateComponent<EditorLocalClientSelectionInput>(gameObject);
				IDisposable localClientDisposable = null;
				CharacterSelectionBotsConfiguration characterSelectionBotsConfiguration = this._container.Resolve<CharacterSelectionBotsConfiguration>();
				characterSelectionBotsConfiguration.ForceDisconnect = input.OnBotForceDisconnect;
				characterSelectionBotsConfiguration.ForceReconnect = input.OnBotForceReconnect;
				return Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(this._initializeCharacterSelectionConfiguration.Initialize(), delegate(Unit _)
				{
					localClientDisposable = ObservableExtensions.Subscribe<Unit>(this._executeLocalClientCharacterSelection.Execute());
				}), Observable.Merge<Unit>(new IObservable<Unit>[]
				{
					Observable.AsUnitObservable<CharacterSelectionResult>(this._executeCharacterSelection.Execute()),
					this._executeBotsCharacterSelection.Execute(characterSelectionBotsConfiguration),
					Observable.Repeat<Unit>(Observable.Do<Unit>(Observable.ContinueWith<Unit, Unit>(Observable.Do<Unit>(Observable.First<Unit>(input.OnConnectionToggled), delegate(Unit _)
					{
						localClientDisposable.Dispose();
					}), (Unit _) => Observable.First<Unit>(input.OnConnectionToggled)), delegate(Unit _)
					{
						localClientDisposable = ObservableExtensions.Subscribe<Unit>(this._executeLocalClientCharacterSelection.Execute());
					}))
				}));
			}

			private readonly IInitializeCharacterSelectionConfiguration _initializeCharacterSelectionConfiguration;

			private readonly IExecuteCharacterSelection _executeCharacterSelection;

			private readonly IExecuteBotsCharacterSelection _executeBotsCharacterSelection;

			private readonly ExecuteLocalClientCharacterSelection _executeLocalClientCharacterSelection;

			private readonly DiContainer _container;
		}

		private class EditorProceedToClientGameState : IProceedToClientGameState
		{
			public IObservable<Unit> Proceed()
			{
				return Observable.ReturnUnit();
			}
		}

		private class EditorGetLocalPlayer : IGetLocalPlayer
		{
			public IPlayer Get()
			{
				return new Player
				{
					PlayerId = 1L
				};
			}
		}
	}
}
