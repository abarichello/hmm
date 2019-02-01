using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Assets.ClientApiObjects;
using Assets.Customization;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using ClientAPI;
using ClientAPI.Browser;
using ClientAPI.Objects;
using ClientAPI.Service;
using Commons.Swordfish.Server;
using FMod;
using FMOD.Studio;
using HeavyMetalMachines.AI;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Audio.Music;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Counselor;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.HMMChat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.Counselor;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.SensorSystem;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.UpdateStream;
using Pocketverse;
using Pocketverse.MuralContext;
using Pocketverse.NLS;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines
{
	public class HMMHub : GameHub, ICleanupListener
	{
		public ConfigLoader Config
		{
			get
			{
				if (this._config == null)
				{
					this._config = new ConfigLoader(Application.dataPath);
					HMMHub.Log.InfoFormat("Loaded config from:{0}", new object[]
					{
						this._config.fullPath
					});
					HMMHub.Log.InfoFormat("Swordfish URL: @={0}", new object[]
					{
						this._config.GetValue(ConfigAccess.SFBaseUrl)
					});
				}
				return this._config;
			}
		}

		public IGameTime GameTime { get; private set; }

		public static bool IsCodeRunningOnMainThread()
		{
			return HMMHub._mainThread == null || HMMHub._mainThread == Thread.CurrentThread;
		}

		public static bool IsEditorLeavingPlayMode()
		{
			return false;
		}

		public void SetGameTimer(IGameTime gameTime)
		{
			this.GameTime = ((gameTime == null) ? this.Clock : gameTime);
		}

		private void OnApplicationQuit()
		{
			MatchLogWriter.Release();
		}

		private static void ConfigureUnityLoggerStackTrace()
		{
			Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
			Application.SetStackTraceLogType(UnityEngine.LogType.Assert, StackTraceLogType.None);
			Application.SetStackTraceLogType(UnityEngine.LogType.Error, StackTraceLogType.None);
			Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);
			Application.SetStackTraceLogType(UnityEngine.LogType.Exception, StackTraceLogType.ScriptOnly);
		}

		private void UnityLogCallback(string condition, string stackTrace, UnityEngine.LogType type)
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
			case UnityEngine.LogType.Error:
				this._unityLog.Error(message);
				break;
			case UnityEngine.LogType.Assert:
				this._unityLog.Info(message);
				break;
			case UnityEngine.LogType.Warning:
				this._unityLog.Warn(message);
				break;
			case UnityEngine.LogType.Log:
				break;
			case UnityEngine.LogType.Exception:
				this._unityLog.Error(message);
				this.ShowExceptionBox();
				break;
			default:
				throw new ArgumentException("Unknown LogType", "type");
			}
		}

		private void ShowExceptionBox()
		{
		}

		public AutoMatch AutoMatch
		{
			get
			{
				if (null == this._autoMatch && this.Net.IsClient())
				{
					this._autoMatch = base.gameObject.AddComponent<AutoMatch>();
				}
				return this._autoMatch;
			}
		}

		public IHMMContext GetContext()
		{
			if (this._context == null)
			{
				this._context = new HMMContext();
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

		public virtual bool IsWaitingInQueue()
		{
			return this.ClientApi.matchmakingClient.IsWaitingInQueue();
		}

		protected void Awake()
		{
			HMMHub._mainThread = Thread.CurrentThread;
			this.GameTime = this.Clock;
			GameHubObject.Init(this);
			GameHubScriptableObject.Init(this);
			GameHubBehaviour.Init(this);
			this.Scheduler = new Scheduler();
			ManagerController.TryCreateAndInit(this);
			SceneManager.sceneLoaded += this.OnSceneLoaded;
			if (this.Net.IsClient() || this.Net.IsTest())
			{
				this.playbackSystem = base.gameObject.GetComponent<PlaybackSystem>();
				this.AudioManager = new FMODAudioManager();
				if (this.playbackSystem == null)
				{
					this.playbackSystem = base.gameObject.AddComponent<PlaybackSystem>();
				}
			}
			Application.logMessageReceived += this.UnityLogCallback;
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
			bool flag = false;
			this.m_oSingleClientMutex = new Mutex(false, "Global\\Hoplon_HMM_Handle", ref flag);
			if (!flag)
			{
				ConfirmWindowProperties confirmWindowProperties = new ConfirmWindowProperties();
				confirmWindowProperties.Guid = this._confirmWindowGuid;
				confirmWindowProperties.QuestionText = Language.Get("MULTIPLE_CLIENT_WARNING_DESCRIPTION", TranslationSheets.MainMenuGui);
				confirmWindowProperties.OkButtonText = Language.Get("MULTIPLE_CLIENT_CONFIRM", TranslationSheets.MainMenuGui);
				confirmWindowProperties.OnOk = delegate()
				{
					Application.Quit();
				};
				ConfirmWindowProperties properties = confirmWindowProperties;
				this.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			}
		}

		private void LateUpdate()
		{
			this.Scheduler.CheckForPendingEvents();
			if (this.Net.IsClient())
			{
				this.AudioManager.Update();
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
			Application.logMessageReceived -= this.UnityLogCallback;
			SceneManager.sceneLoaded -= this.OnSceneLoaded;
			this.Scheduler.Release();
		}

		private void Start()
		{
			if (this.Net != null && this.Net.IsServer())
			{
				QualitySettings.vSyncCount = 1;
				Application.targetFrameRate = this.Config.GetIntValue(ConfigAccess.TargetFramerateServer);
				Time.fixedDeltaTime = this.Config.GetFloatValue(ConfigAccess.TargetFixedStepServer);
				NativePlugins.Instance.IsServer = !this.Net.IsTest();
				this.ServerAnnoucer.Initialize();
			}
			else
			{
				Time.fixedDeltaTime = this.Config.GetFloatValue(ConfigAccess.TargetFixedStepClient);
				this.bombDropperAnimatorController = new BombDropperAnimatorController();
				this.bombDropperAnimatorController.Initialize();
				this._autoMatch = base.gameObject.AddComponent<AutoMatch>();
				this.StartAudio();
				this.snapshotAudioController = new HMMSnapshotAudioController();
				this.snapshotAudioController.RegisterEvents();
				MusicManager.PlayMusic(MusicManager.State.MainMenu);
				this.AnnouncerAudio.Initialize();
			}
			if (this.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				ClientAPI.Objects.ItemType[] array = new ClientAPI.Objects.ItemType[this.InventoryColletion.ItemTypes.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = this.InventoryColletion.ItemTypes[i].ItemType;
				}
				this.InventoryColletion.SetAllItemTypes(array);
			}
			NativePlugins.Instance.BeforeAppFinish += this.BeforeAppFinish;
		}

		private void StartAudio()
		{
			for (int i = 0; i < this.AudioSettings.SyncronousStartingBanks.Length; i++)
			{
				FMODAudioManager.LoadBank(this.AudioSettings.SyncronousStartingBanks[i], LOAD_BANK_FLAGS.NORMAL, false);
			}
			for (int j = 0; j < this.AudioSettings.AsyncronousStartingBanks.Length; j++)
			{
				FMODAudioManager.LoadBank(this.AudioSettings.AsyncronousStartingBanks[j], LOAD_BANK_FLAGS.NONBLOCKING, false);
			}
			FMODAudioManager.LoadBank(string.Format("VO_{0}.bank", GameHubBehaviour.Hub.AudioSettings.AsyncronousCounselorBankName), LOAD_BANK_FLAGS.NONBLOCKING, true);
			FMODAudioManager.LoadBank(string.Format("{0}_{1}.bank", GameHubBehaviour.Hub.AudioSettings.AsyncronousCounselorBankName, GameHubBehaviour.Hub.Options.Game.CounselorLanguage.ToString()), LOAD_BANK_FLAGS.NONBLOCKING, true);
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
		{
			if (this.Net.IsServer())
			{
				UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(ParticleSystem));
				for (int i = 0; i < array.Length; i++)
				{
					((ParticleSystem)array[i]).gameObject.SetActive(false);
				}
			}
		}

		public void ScheduleEvent(int timeOut, Action<object> callback, object parameter)
		{
			this.Scheduler.ScheduleEvent(timeOut, callback, parameter);
		}

		public void Quit()
		{
			HMMHub.Log.Info("Quit");
			try
			{
				if (this.Net.IsServer())
				{
					((NetworkServer)GameHubBehaviour.Hub.Net).StopServer();
				}
				if (this.Net.IsClient())
				{
					this.bombDropperAnimatorController.CleanUp();
				}
				if (this.Swordfish != null && this.Swordfish.Connection != null)
				{
					this.Swordfish.Connection.Dispose();
					HMMHub.Log.Info("Stopped SwordfishConnection and Heartbeat, will call Application.Quit now!");
				}
				Application.Quit();
			}
			catch (Exception ex)
			{
				try
				{
					HMMHub.Log.ErrorFormat("Exception trying to quit. Exception:{0}", new object[]
					{
						ex
					});
					Application.Quit();
				}
				catch (Exception)
				{
					Process.GetCurrentProcess().Kill();
				}
			}
		}

		private void BeforeAppFinish()
		{
			if (this.Net.IsClient())
			{
				this.Server.ClientSendPlayerDisconnectInfo();
			}
			if (this.ClientApiBrowser != null)
			{
				HMMHub.Log.Warn("The In-Game browser is not supposed to be live yet.");
			}
		}

		public void GetSWFVersion(System.Action onPlayerOk, SwordfishClientApi.ErrorCallback onGetError)
		{
			VersionCustomWS.GetVersion(delegate(object oState, string sResult)
			{
				NetResult netResult = (NetResult)((JsonSerializeable<T>)sResult);
				if (!netResult.Success)
				{
					onGetError(oState, new Exception(netResult.Msg));
					return;
				}
				VersionData versionData = (VersionData)((JsonSerializeable<T>)netResult.Msg);
				Assembly assembly = this.ClientApi.GetType().Assembly;
				FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
				string fileVersion = versionInfo.FileVersion;
				if (fileVersion != versionData.SWFClientAPIVersion)
				{
					onGetError(oState, new Exception(string.Format("SwordFish version are diferent   Client SwordFish Version: {0}  SwordFish Version: {1}", fileVersion, versionData.SWFClientAPIVersion)));
					return;
				}
				if (!"2.07.972".Contains(versionData.CustomWSVersion))
				{
					onGetError(oState, new Exception(string.Format("Game Version are diferent   Game Version: {0}  CustomWs Version: {1} ", "2.07.972", versionData.CustomWSVersion)));
					return;
				}
				if (fileVersion != versionData.CustomWSClientAPIVersion)
				{
					onGetError(oState, new Exception(string.Format("Client SwordFish Version and CustomWS Swordfish Version are diferent   Client SwordFish Version: {0}  CustomWS SwordFish Version: {1} ", fileVersion, versionData.CustomWSClientAPIVersion)));
					return;
				}
				onPlayerOk();
			}, delegate(object state, Exception exception)
			{
				if (onGetError != null)
				{
					HMMHub.Log.Error("GetVersion ERROR ", exception);
					onGetError(state, null);
				}
			});
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HMMHub));

		private readonly BitLogger _unityLog = new BitLogger("Unity");

		public Pocketverse.Network Net;

		public Heartbeat Heart;

		public RPCObjectCollection ObjectCollection;

		public StateMachine State;

		private ConfigLoader _config;

		[NonSerialized]
		public readonly HMMPlayerPrefs PlayerPrefs = new HMMPlayerPrefs();

		public Drawers Drawer;

		public AuthenticationManager AuthMan;

		public AddressGroupHelper AddressGroups = new TwoGroup();

		public RPCClassDictionary Classes = new RPCClassDictionary();

		public RPCFilter Rpc = new RPCFilter();

		public NetworkTime Clock;

		public BombManager BombManager;

		public ResourceLoader Resources;

		public NLSManager NLSMan;

		private static Thread _mainThread;

		public MatchData Match = new MatchData
		{
			State = MatchData.MatchState.Nothing
		};

		public MatchPlayers Players = new MatchPlayers();

		public MatchController MatchMan;

		public StaticContentCollection StaticContent;

		public CustomizationAssetsScriptableObject CustomizationAssets;

		public CursorManager CursorManager;

		public BotAIHub BotAIHub;

		public TutorialHub TutorialHub;

		public UserInfo User;

		public ServerInfo Server;

		public HeavyMetalMachines.Character.CharacterService Characters;

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

		public HeavyMetalMachines.Audio.AudioSettings AudioSettings;

		public UserService UserService;

		public SwordfishClientApi ClientApi;

		public Browser ClientApiBrowser;

		public ChatService Chat;

		public SwordfishServices Swordfish;

		public GuiScripts GuiScripts;

		public HeavyMetalMachines.Options.Options Options;

		public SwordfishStore Store;

		public HackManager HackManager;

		public AFKController afkController;

		public SharedConfigs SharedConfigs;

		public MatchHistoryController MatchHistory;

		public BombDropperAnimatorController bombDropperAnimatorController;

		public GameArenaConfig ArenaConfig;

		public CounselorConfig CounselorConfig;

		public VersionChecker VersionChecker;

		public CollectionScriptableObject InventoryColletion;

		[NonSerialized]
		public readonly ServerAnnouncer ServerAnnoucer = new ServerAnnouncer();

		[NonSerialized]
		public readonly GlobalStates Global = new GlobalStates();

		[NonSerialized]
		public FMODAudioManager AudioManager;

		private Scheduler Scheduler;

		public PlaybackSystem playbackSystem;

		public PlaybackManager PlaybackManager;

		public PlayerTechnicalExperienceManager PlayerExperienceBI;

		public ClientCounselorController ClientCounselorController;

		public HMMSensorController SensorController;

		private AutoMatch _autoMatch;

		private IHMMContext _context;

		private Mutex m_oSingleClientMutex;

		private Guid _confirmWindowGuid = Guid.NewGuid();

		private TwoGroup _onlineTeams = new TwoGroup();

		public BotAIMatchRules BotAIMatchRules;
	}
}
