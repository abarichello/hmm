using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Assets.ClientApiObjects;
using Assets.Customization;
using Assets.Standard_Assets.Scripts.HMM.Swordfish.Services;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using FMod;
using HeavyMetalMachines.AI;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Counselor;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.DataTransferObjects.Server;
using HeavyMetalMachines.DataTransferObjects.Tournament;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.HMMChat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.Counselor;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Login;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.MatchMakingQueue.Infra;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.SensorSystem;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.UpdateStream;
using Hoplon.Reactive;
using Hoplon.Serialization;
using NativePlugins;
using Pocketverse;
using Pocketverse.MuralContext;
using Pocketverse.NLS;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Zenject;

namespace HeavyMetalMachines
{
	public class HMMHub : GameHub, ICleanupListener
	{
		public MainMenu MainMenuState
		{
			get
			{
				return this._mainMenuState;
			}
		}

		public ReconnectMenu ReconnectState
		{
			get
			{
				return this._reconnectState;
			}
		}

		public Profile CreateProfileState
		{
			get
			{
				return this._createProfileState;
			}
		}

		public MatchmakingTutorial MatchmakingTutorialState
		{
			get
			{
				return this._matchmakingTutorialState;
			}
		}

		public IConfigLoader Config
		{
			get
			{
				return this._config;
			}
		}

		public static bool IsCodeRunningOnMainThread()
		{
			return HMMHub._mainThread == null || HMMHub._mainThread == Thread.CurrentThread;
		}

		public static bool IsEditorLeavingPlayMode()
		{
			return false;
		}

		private void OnApplicationQuit()
		{
			Debug.Log("[HMMHub] OnApplicationQuit");
			MatchLogWriter.Release();
		}

		private static void ConfigureUnityLoggerStackTrace()
		{
			Application.SetStackTraceLogType(3, 0);
			Application.SetStackTraceLogType(1, 0);
			Application.SetStackTraceLogType(0, 0);
			Application.SetStackTraceLogType(2, 0);
			Application.SetStackTraceLogType(4, 1);
		}

		private void UnityLogCallback(string condition, string stackTrace, LogType type)
		{
			this.AddUnityLog(condition, stackTrace, type);
		}

		private void AddUnityLog(string condition, string stackTrace, LogType type)
		{
			string message = (!string.IsNullOrEmpty(stackTrace)) ? string.Format("{0}{1}Stack:{2}{3}", new object[]
			{
				condition,
				Environment.NewLine,
				Environment.NewLine,
				stackTrace
			}) : condition;
			switch (type)
			{
			case 0:
				this._unityLog.Error(message);
				break;
			case 1:
				this._unityLog.Info(message);
				break;
			case 2:
				this._unityLog.Warn(message);
				break;
			case 3:
				this._unityLog.Debug(message);
				break;
			case 4:
				this._unityLog.Error(message);
				this.ShowExceptionBox();
				break;
			default:
				throw new ArgumentException("Unknown LogType", "type");
			}
		}

		[Conditional("AllowHacks")]
		private static void DisableDevelopmentConsole()
		{
			Debug.developerConsoleVisible = false;
		}

		private void ShowExceptionBox()
		{
		}

		public IMatchTeams Teams
		{
			get
			{
				return this._teams;
			}
		}

		public IMatchTeamsServer ServerTeams
		{
			get
			{
				return this._teams;
			}
		}

		public IMatchTeamsDispatcher TeamsDispatcher
		{
			get
			{
				return this._teams;
			}
		}

		public IHMMContext GetContext()
		{
			if (this._context == null)
			{
				this._context = new HMMContext(this._gameCamera);
			}
			return this._context;
		}

		public virtual void BILogClient(ClientBITags biTag, bool forceSendLogs)
		{
			if (this.Swordfish == null)
			{
				return;
			}
			if (this.Swordfish.Log == null)
			{
				return;
			}
			this.Swordfish.Log.BILogClient(biTag, forceSendLogs);
		}

		private void BILogClientMsg(ClientBITags biTag, string msg, bool forceSendLogs)
		{
			if (this.Swordfish == null)
			{
				return;
			}
			if (this.Swordfish.Log == null)
			{
				return;
			}
			this.Swordfish.Log.BILogClientMsg(biTag, msg, forceSendLogs);
		}

		public virtual bool IsWaitingInQueue()
		{
			return this.ClientApi.matchmakingClient.IsWaitingInQueue();
		}

		protected void Awake()
		{
			HMMHub._mainThread = Thread.CurrentThread;
			this._gameQuitHandler = Platform.Current.CreateQuitHandler(this.Swordfish, this.Net.IsServer());
			GameHubObject.Init(this);
			GameHubScriptableObject.Init(this);
			GameHubBehaviour.Init(this);
			SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
			Application.logMessageReceived += new Application.LogCallback(this.UnityLogCallback);
			HMMHub.ConfigureUnityLoggerStackTrace();
			this.ForceSingleGameInstance();
		}

		private void ForceSingleGameInstance()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (!Platform.Current.CheckSingleApplicationInstance())
			{
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = this._confirmWindowGuid,
					QuestionText = Language.Get("MULTIPLE_CLIENT_WARNING_DESCRIPTION", TranslationContext.MainMenuGui),
					OkButtonText = Language.Get("MULTIPLE_CLIENT_CONFIRM", TranslationContext.MainMenuGui),
					OnOk = delegate()
					{
						this._gameQuitHandler.Quit(24, string.Empty);
					}
				};
				this.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			}
		}

		private void LateUpdate()
		{
			if (this.Net.IsClient())
			{
				this._updateAudio.Update();
			}
			this.Rpc.Update();
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.MatchMan = null;
			this.Match = new MatchData
			{
				State = MatchData.MatchState.Nothing
			};
			this.Players.OnCleanup();
			this._teams.OnCleanup();
		}

		public void AddOnline(byte add, TeamKind team)
		{
			this._onlineTeams.AddAddress(add, (int)team);
		}

		public void RemoveOnline(byte add)
		{
			this.AddressGroups.RemAddress(add);
			this._onlineTeams.RemAddress(add);
		}

		public byte[] SendAll
		{
			get
			{
				return this._onlineTeams.GetGroup(0);
			}
		}

		public byte[] SendRed
		{
			get
			{
				return this._onlineTeams.GetGroup(1);
			}
		}

		public byte[] SendBlu
		{
			get
			{
				return this._onlineTeams.GetGroup(2);
			}
		}

		private void OnDestroy()
		{
			this._gameQuitHandler.Dispose();
			Application.logMessageReceived -= new Application.LogCallback(this.UnityLogCallback);
			SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
			if (this.Net.IsServer())
			{
				return;
			}
			FMODAudioManager.Instance.Dispose();
		}

		private void Start()
		{
			if (this.Net != null && this.Net.IsServer())
			{
				Application.targetFrameRate = this._config.GetIntValue(ConfigAccess.TargetFramerateServer);
				Time.fixedDeltaTime = this._config.GetFloatValue(ConfigAccess.TargetFixedStepServer);
				UnityInterface.Instance.SetIsServer(!this.Net.IsTest());
				this.ServerAnnoucer.Initialize();
			}
			else
			{
				this.InitializeOnSuspendOperations();
				Physics.autoSimulation = false;
				Time.fixedDeltaTime = this._config.GetFloatValue(ConfigAccess.TargetFixedStepClient);
				this.bombDropperAnimatorController = new BombDropperAnimatorController();
				this.bombDropperAnimatorController.Initialize();
				this.StartAudio();
				this.snapshotAudioController = new HMMSnapshotAudioController();
				this.snapshotAudioController.RegisterEvents();
				this.AnnouncerAudio.Initialize();
			}
			if (this._config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				List<ItemType> list = new List<ItemType>(this.InventoryColletion.ItemTypes.Length);
				for (int i = 0; i < this.InventoryColletion.ItemTypes.Length; i++)
				{
					ItemTypeScriptableObject itemTypeScriptableObject = this.InventoryColletion.ItemTypes[i];
					if (itemTypeScriptableObject.IsActive)
					{
						list.Add(itemTypeScriptableObject.ItemType);
					}
				}
				this.InventoryColletion.SetAllItemTypes(list.ToArray());
				Team team = new Team
				{
					Bag = new TeamBag().ToString(),
					Id = Guid.NewGuid(),
					Name = "Redders",
					Tag = "RED"
				};
				Team team2 = new Team
				{
					Bag = new TeamBag().ToString(),
					Id = Guid.NewGuid(),
					Name = "Bluers",
					Tag = "BLU"
				};
				this.ServerTeams.AddTeam("-10", team);
				this.ServerTeams.AddTeam("-11", team2);
				this.ServerTeams.SetGroupTeam(TeamKind.Red, team);
				this.ServerTeams.SetGroupTeam(TeamKind.Blue, team2);
			}
			UnityInterface.Instance.BeforeAppFinish += this.BeforeAppFinish;
		}

		private void StartAudio()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(ObservableExtensions.ContinueWith<Unit, Unit>(ObservableExtensions.ContinueWith<Unit, Unit>(this._initializeAudio.Initialize(), new Func<IObservable<Unit>>(this._loadAudio.LoadGenericAudio)), new Func<IObservable<Unit>>(this._loadAudio.LoadLanguageSpecificAudio)), delegate(Unit _)
			{
				this.StartOptionsAudio();
			}));
		}

		private void StartOptionsAudio()
		{
			this.Options.Audio.Setup();
			this.PlayerPrefs.ExecOnceOnPrefsLoaded(new Action(this.Options.Audio.LoadPrefs));
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
		{
			if (this.Net.IsServer())
			{
				Object[] array = Object.FindObjectsOfType(typeof(ParticleSystem));
				for (int i = 0; i < array.Length; i++)
				{
					((ParticleSystem)array[i]).gameObject.SetActive(false);
				}
			}
		}

		public void EndSession(string reason)
		{
			if (this.State.Current.StateKind == GameState.GameStateKind.Welcome)
			{
				HMMHub.Log.WarnFormat("Session ending before even starting. Reason={0}", new object[]
				{
					reason
				});
				return;
			}
			HMMHub.Log.WarnFormat("Ending Session. Reason: {0}", new object[]
			{
				reason
			});
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				this.BILogClientMsg(101, string.Format("Reason={0}", reason), true);
				((NetworkClient)GameHubBehaviour.Hub.Net).CloseConnection();
			}
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
			this.State.GotoState(this.State.getGameState(GameState.GameStateKind.Welcome), false);
		}

		public void Quit(GameQuitReason quitReason)
		{
			this._gameQuitHandler.Quit(quitReason, string.Empty);
		}

		private void BeforeAppFinish()
		{
			if (this.Net.IsClient())
			{
				this.Server.ClientSendPlayerDisconnectInfo();
			}
		}

		public void CheckClientAndServerSwordfishApiVersion(Action matchingCallback, Action unmatchingCallback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			VersionCustomWS.GetVersion(delegate(object state, string result)
			{
				this.OnCheckClientAndServerSwordfishApiVersionSuccess(state, result, matchingCallback, unmatchingCallback, errorCallback);
			}, delegate(object state, Exception exception)
			{
				this.OnCheckClientAndServerSwordfishApiVersionError(state, errorCallback);
			});
		}

		private void OnCheckClientAndServerSwordfishApiVersionSuccess(object state, string result, Action matchingCallback, Action unmatchingCallback, SwordfishClientApi.ErrorCallback errorCallback)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)result);
			if (!netResult.Success)
			{
				errorCallback.Invoke(state, new Exception(netResult.Msg));
				return;
			}
			VersionData versionData = (VersionData)((JsonSerializeable<!0>)netResult.Msg);
			Assembly assembly = this.ClientApi.GetType().Assembly;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
			string fileVersion = versionInfo.FileVersion;
			if (fileVersion != versionData.SWFClientAPIVersion)
			{
				unmatchingCallback();
				return;
			}
			if (!"Release.15.00.250".Contains(versionData.CustomWSVersion))
			{
				unmatchingCallback();
				return;
			}
			if (fileVersion != versionData.CustomWSClientAPIVersion)
			{
				unmatchingCallback();
				return;
			}
			matchingCallback();
		}

		private void OnCheckClientAndServerSwordfishApiVersionError(object state, SwordfishClientApi.ErrorCallback errorCallback)
		{
			errorCallback.Invoke(state, null);
		}

		private void InitializeOnSuspendOperations()
		{
			IGetBackendSession getBackendSession = this._diContainer.Resolve<IGetBackendSession>();
			IPlatformNotifications platformNotifications = this._diContainer.Resolve<IPlatformNotifications>();
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(Observable.SelectMany<Unit, Unit>(Observable.Where<Unit>(Observable.Do<Unit>(platformNotifications.OnResumedFromSuspension, delegate(Unit _)
			{
				Platform.Current.StopNotifyingSessionInvitations();
			}), (Unit _) => getBackendSession.Get().IsSome), (Unit _) => this.ProcessClientApiDlc())), this);
		}

		private IObservable<Unit> ProcessClientApiDlc()
		{
			return Observable.Do<Unit>(SwordfishObservable.FromSwordfishCall(delegate(SwordfishClientApi.Callback success, SwordfishClientApi.ErrorCallback error)
			{
				this.ClientApi.publisher.dlc.ProcessDlc(null, success, error);
			}), delegate(Unit _)
			{
				HMMHub.Log.Info("ProcessClientApiDlc end");
			});
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HMMHub));

		private readonly BitLogger _unityLog = new BitLogger("Unity");

		public Network Net;

		public Heartbeat Heart;

		public RPCObjectCollection ObjectCollection;

		public StateMachine State;

		private IGameQuitHandler _gameQuitHandler;

		[NonSerialized]
		public readonly HMMPlayerPrefs PlayerPrefs = new HMMPlayerPrefs();

		public Drawers Drawer;

		public AuthenticationManager AuthMan;

		public AddressGroupHelper AddressGroups = new TwoGroup();

		public RPCClassDictionary Classes = new RPCClassDictionary();

		public RPCFilter Rpc = new RPCFilter();

		[Inject]
		public IGameTime GameTime;

		public NetworkTime Clock;

		public BombManager BombManager;

		public ResourceLoader Resources;

		public NLSManager NLSMan;

		[InjectOnClient]
		public DiContainer _diContainer;

		[Inject]
		protected IConfigLoader _config;

		[SerializeField]
		private MainMenu _mainMenuState;

		[SerializeField]
		private ReconnectMenu _reconnectState;

		[SerializeField]
		private Profile _createProfileState;

		[SerializeField]
		private MatchmakingTutorial _matchmakingTutorialState;

		private static Thread _mainThread;

		public MatchData Match = new MatchData
		{
			State = MatchData.MatchState.Nothing
		};

		public MatchPlayers Players = new MatchPlayers();

		private readonly MatchTeams _teams = new MatchTeams();

		public MatchController MatchMan;

		public CustomizationAssetsScriptableObject CustomizationAssets;

		public CursorManager CursorManager;

		public BotAIHub BotAIHub;

		public TutorialHub TutorialHub;

		public UserInfo User;

		public ServerInfo Server;

		public CharacterService Characters;

		public InputManager Input;

		public UpdateStreamManager UpdateManager;

		public UpdateStreamAccess Stream;

		public EventManager Events;

		public EffectsManager Effects;

		public ScrapBank ScrapBank;

		public ScrapLevels ScrapLevel;

		public AnnouncerManager Announcer;

		public AnnouncerAudio AnnouncerAudio;

		private HMMSnapshotAudioController snapshotAudioController;

		public AudioSettings AudioSettings;

		public IUser UserService;

		public SwordfishClientApi ClientApi;

		public ChatService Chat;

		public SwordfishServices Swordfish;

		public GuiScripts GuiScripts;

		[InjectOnClient]
		public HeavyMetalMachines.Options.Options Options;

		[InjectOnClient]
		private IGameCamera _gameCamera;

		public SwordfishStore Store;

		public HackManager HackManager;

		[FormerlySerializedAs("afkController")]
		public AFKController OldAfkController;

		public SharedConfigs SharedConfigs;

		public MatchHistoryController MatchHistory;

		public BombDropperAnimatorController bombDropperAnimatorController;

		[Obsolete]
		public GameArenaConfig ArenaConfig;

		public IGroupService GroupService;

		public IMatchmakingService MatchmakingService;

		public CounselorConfig CounselorConfig;

		public VersionChecker VersionChecker;

		public CollectionScriptableObject InventoryColletion;

		[NonSerialized]
		public readonly ServerAnnouncer ServerAnnoucer = new ServerAnnouncer();

		[NonSerialized]
		public readonly GlobalStates Global = new GlobalStates();

		public PlaybackManager PlaybackManager;

		public PlayerTechnicalExperienceManager PlayerExperienceBI;

		public ClientCounselorController ClientCounselorController;

		public HMMSensorController SensorController;

		private IHMMContext _context;

		private readonly Guid _confirmWindowGuid = Guid.NewGuid();

		private readonly TwoGroup _onlineTeams = new TwoGroup();

		public BotAIMatchRules BotAIMatchRules;

		[InjectOnClient]
		private IInitializeAudio _initializeAudio;

		[InjectOnClient]
		private ILoadAudio _loadAudio;

		[InjectOnClient]
		private IUpdateAudio _updateAudio;
	}
}
