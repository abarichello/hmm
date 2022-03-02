using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Assets.Standard_Assets.Scripts.HMM.Swordfish.Services;
using Assets.Standard_Assets.Scripts.Infra;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Objects.Custom;
using ClientAPI.Service;
using HeavyMetalMachines.DataTransferObjects.Server;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameServer;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches.DataTransferObjects;
using HeavyMetalMachines.MatchMakingQueue.Infra;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Regions.Business;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishConnection : GameHubObject
	{
		public SwordfishConnection(ISetServerRegion setServerRegion, IPublisher publisher, IGetParentalControlSettings getParentalControlSettings, IParseGameServerStartRequest parseGameServerStartRequest, IGameServerStartRequestStorage gameServerStartRequestStorage, DiContainer diContainer) : this(Environment.GetCommandLineArgs(), setServerRegion, publisher, getParentalControlSettings, parseGameServerStartRequest, gameServerStartRequestStorage, diContainer)
		{
		}

		public SwordfishConnection(string[] args, ISetServerRegion setServerRegion, IPublisher publisher, IGetParentalControlSettings getParentalControlSettings, IParseGameServerStartRequest parseGameServerStartRequest, IGameServerStartRequestStorage gameServerStartRequestStorage, DiContainer container)
		{
			this._getParentalControlSettings = getParentalControlSettings;
			this._container = container;
			this._setServerRegion = setServerRegion;
			this._publisher = publisher;
			this.InitializeClientApi();
			if (GameHubObject.Hub.Net.IsClient())
			{
				return;
			}
			this.InitializeMatchData(args);
			if (args != null)
			{
				GameServerStartRequest gameServerStartRequest = parseGameServerStartRequest.Parse(args);
				gameServerStartRequestStorage.Set(gameServerStartRequest);
			}
			SwordfishConnection.Log.DebugFormat("Clustered={0} jobId={1} matchId={2}", new object[]
			{
				this.Clustered,
				this.JobId,
				this._serverMatchId
			});
			this.InitializeSwordfishComm();
		}

		public bool Connected
		{
			get
			{
				return this._connected;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event SwordfishConnection.OnSwordfishConnected ListenToSwordfishConnected;

		public bool Clustered
		{
			get
			{
				return this._clustered;
			}
		}

		public SwordfishClientApi ClientApi
		{
			get
			{
				return GameHubObject.Hub.ClientApi;
			}
		}

		public IGroupService GroupService
		{
			get
			{
				return GameHubObject.Hub.GroupService;
			}
		}

		[Obsolete("When on Client: use IGetBackendSession instead.")]
		public string SessionId
		{
			get
			{
				return this._sessionId;
			}
			set
			{
				this._sessionId = value;
				this._connected = !string.IsNullOrEmpty(value);
			}
		}

		public Guid ServerMatchId
		{
			get
			{
				return this._serverMatchId;
			}
		}

		public long TournamentStepId { get; private set; }

		public string QueueName { get; private set; }

		public string RegionName { get; private set; }

		public bool IsFirstLogin { get; private set; }

		public bool PlayerEverJoinedQueue { get; private set; }

		public void RaiseConnected()
		{
			if (this._connected && this.ListenToSwordfishConnected != null)
			{
				this.ListenToSwordfishConnected();
			}
		}

		private void InitializeMatchData(string[] args)
		{
			long tournamentStepId = -1L;
			string regionName = null;
			string queueName = null;
			GameHubObject.Hub.Match.ArenaIndex = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.ArenaIndex);
			GameHubObject.Hub.Match.Kind = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.MatchKind);
			MatchKind kind = GameHubObject.Hub.Match.Kind;
			string[] array;
			string[] array2;
			string[] array3;
			SwordfishConnectionArgsParser.ParseMatchArgs(args, out array, out array2, out array3, ref this.JobId, ref this._serverMatchId, ref this.ServerConfiguredBySwordfish, ref GameHubObject.Hub.Server.ServerIp, ref GameHubObject.Hub.Server.ServerPort, ref kind, ref GameHubObject.Hub.Match.ArenaIndex, ref tournamentStepId, ref regionName, ref queueName);
			this._setServerRegion.SetRegionName(regionName);
			this.RegionName = regionName;
			this.QueueName = queueName;
			this.TournamentStepId = tournamentStepId;
			GameHubObject.Hub.Match.Kind = kind;
			this._clustered = (this.JobId > 0L);
			if (kind == 6)
			{
				array2 = SwordfishConnectionArgsParser.AutoCompleteTeamArrayWithBots(array2, 4);
				array = SwordfishConnectionArgsParser.AutoCompleteTeamArrayWithBots(array, 4);
			}
			List<SwordfishConnection.MatchUser> list = new List<SwordfishConnection.MatchUser>();
			if (array2.Length > 0)
			{
				this.AddUsers(array2, TeamKind.Blue, ref list, "Blue players");
			}
			if (array.Length > 0)
			{
				this.AddUsers(array, TeamKind.Red, ref list, "Red players");
			}
			if (array3.Length > 0)
			{
				this.AddUsers(array3, TeamKind.Neutral, ref list, "Spectators");
			}
			this.Users = list.ToArray();
			GameHubObject.Hub.Match.UserCount = this.Users.Length;
		}

		private void AddUsers(string[] teamMembers, TeamKind team, ref List<SwordfishConnection.MatchUser> users, string logMsg)
		{
			for (int i = 0; i < teamMembers.Length; i++)
			{
				users.Add(new SwordfishConnection.MatchUser
				{
					Id = teamMembers[i],
					Team = team,
					Slot = i
				});
			}
			SwordfishConnection.Log.InfoFormat("{0}={1}", new object[]
			{
				logMsg,
				teamMembers.Length
			});
		}

		private void InitializeClientApi()
		{
			bool flag = GameHubObject.Hub.Net.IsClient();
			SwordfishBadNetworkConditionConfig badNetConfig = SwordfishConnection.CreateClientApiLagConfiguration();
			SwordfishClientApiConfig swordfishClientApiConfig = new SwordfishClientApiConfig
			{
				FileNamePrefixForLog = "clientApi",
				DirPathForLog = Platform.Current.GetLogsDirectory(),
				UseConsoleForLog = false,
				RolloverFileForLog = false,
				PublisherIoC = this._publisher.GetPublisherIoC(),
				BadNetConfig = badNetConfig,
				ConfigFileDirectoryPath = ((!Platform.Current.IsConsole()) ? Platform.Current.GetPersistentDataDirectory() : Platform.Current.GetTemporaryDataDirectory()),
				GameTitleId = Platform.Current.GetApplicationId(),
				GameImagePath = Platform.Current.GetSessionImagePath(),
				MaxRetriesAttempt = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.SFMaxRetries)
			};
			GameHubObject.Hub.ClientApi = new SwordfishClientApi(flag, swordfishClientApiConfig);
			string text = Language.CurrentLanguage.ToString().ToLower();
			GameHubObject.Hub.ClientApi.SetLanguageCode(text);
			string text2 = (!flag) ? string.Empty : this._publisher.GetGameClientBaseUrl();
			GameHubObject.Hub.ClientApi.BaseUrl = ((!string.IsNullOrEmpty(text2)) ? text2 : GameHubObject.Hub.Config.GetValue(ConfigAccess.SFBaseUrl));
			GameHubObject.Hub.ClientApi.RequestTimeOut = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.SFTimeout);
			GameHubObject.Hub.ClientApi.RequestConnection = this._publisher.GetRequestConnection();
			if (GameHubObject.Hub.ClientApi.group != null)
			{
				GameHubObject.Hub.GroupService = new SwordfishGroupService(GameHubObject.Hub.ClientApi.group);
			}
			if (flag)
			{
				SwordfishMatchmakingWrapper matchmakingClient = new SwordfishMatchmakingWrapper(GameHubObject.Hub.ClientApi.matchmakingClient);
				SwordfishHubClientWrapper swordfishHubClient = new SwordfishHubClientWrapper(GameHubObject.Hub.ClientApi.hubClient);
				GameHubObject.Hub.MatchmakingService = new MatchmakingService(matchmakingClient, (NetworkClient)GameHubObject.Hub.Net, swordfishHubClient);
			}
			GameHubObject.Hub.UserService = new UserService(GameHubObject.Hub.ClientApi);
			SwordfishConnection.Log.InfoFormat("Language:{0} GameVersion:{1} Swordfish URL={2} PID={3}", new object[]
			{
				text,
				"Release.15.00.250",
				GameHubObject.Hub.ClientApi.BaseUrl,
				Process.GetCurrentProcess().Id
			});
			if (flag)
			{
				this.InitializeClientApiClientSide();
			}
		}

		private void InitializeClientApiClientSide()
		{
			SwordfishConnection.InitializeInternetTimeout();
			this.InitializeParentalControl();
		}

		private void InitializeParentalControl()
		{
			ParentalControlSettings parentalControlSettings = this._getParentalControlSettings.Get();
			AgeRestriction ageRestriction = default(AgeRestriction);
			ageRestriction.DefaultAge = parentalControlSettings.MinimumAgeRequiredToPlay;
			AgeRestriction ageRestrictionSync = ageRestriction;
			GameHubObject.Hub.ClientApi.parentalcontrol.SetAgeRestrictionSync(ageRestrictionSync);
		}

		private static void InitializeInternetTimeout()
		{
			GameHubObject.Hub.ClientApi.internet.SetCycleIntervalToDetectOffline(GameHubObject.Hub.Config.GetIntValue(ConfigAccess.SFCycleIntervalToDetectOffline));
			GameHubObject.Hub.ClientApi.internet.SetNumberOfRetryToDetectOffline(GameHubObject.Hub.Config.GetIntValue(ConfigAccess.SFNumberOfRetryToDetectOffline));
			GameHubObject.Hub.ClientApi.internet.SetWebRequestTimeoutInMillis(GameHubObject.Hub.Config.GetIntValue(ConfigAccess.SFWebRequestTimeoutInMillis));
			GameHubObject.Hub.ClientApi.internet.StartMonitoring();
		}

		private static SwordfishBadNetworkConditionConfig CreateClientApiLagConfiguration()
		{
			return new SwordfishBadNetworkConditionConfig();
		}

		private void InitializeSwordfishComm()
		{
			if (this.Clustered)
			{
				this._comm = new SwordfishComm(this.JobId);
			}
		}

		private void ShowConnectionLostBecauseWentOffline()
		{
			this.ShowConnectionLost("SwordfishConnection Lost");
		}

		public void ShowConnectionLost(string reason)
		{
			SwordfishConnection.Log.Warn("CONNECTION LOST");
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				IsStackable = false,
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("LostMessageHubConnection", TranslationContext.GUI),
				OkButtonText = Language.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					GameHubObject.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					GameHubObject.Hub.EndSession(reason);
				}
			};
			GameHubObject.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void RegisterConnectionMonitoring()
		{
			SwordfishConnection.Log.Debug("Registering Connection Monitoring");
			GameHubObject.Hub.ClientApi.internet.Offline += this.ShowConnectionLostBecauseWentOffline;
		}

		public void DeregisterConnectionMonitoring()
		{
			SwordfishConnection.Log.Debug("Deregistering Connection Monitoring");
			GameHubObject.Hub.ClientApi.internet.Offline -= this.ShowConnectionLostBecauseWentOffline;
		}

		public void Update()
		{
			int num = 0;
			int num2 = 0;
			if (this._nextLog < Time.time)
			{
				SwordfishConnection.Log.Debug("Swordfish connection running");
				this._nextLog = Time.time + this.LogPeriod;
			}
			while (num++ < this.MsgsPerFrame && GameHubObject.Hub.ClientApi.GetMessageCount() > 0)
			{
				GameHubObject.Hub.ClientApi.ProcessNextMessage();
				num2 = GameHubObject.Hub.ClientApi.GetMessageCount();
			}
			if (!this.Connected)
			{
				return;
			}
			if (GameHubObject.Hub.Net.IsClient())
			{
				GameHubObject.Hub.ClientApi.UpdateFrame();
			}
			if (GameHubObject.Hub.Net.IsServer())
			{
				this.CheckUpdateStatus();
				if (this._clustered)
				{
					this._comm.Tick();
				}
			}
			if (num2 > 0)
			{
				SwordfishConnection.Log.WarnFormat("Could not process all messages this frame! Missing={0}", new object[]
				{
					GameHubObject.Hub.ClientApi.GetMessageCount()
				});
			}
		}

		public void OnPlayersLoaded(Team redTeam, Team blueTeam)
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<string> list3 = new List<string>();
			List<string> list4 = new List<string>();
			List<long> list5 = new List<long>();
			List<long> list6 = new List<long>();
			List<long> list7 = new List<long>();
			List<long> list8 = new List<long>();
			for (int i = 0; i < this.Users.Length; i++)
			{
				SwordfishConnection.MatchUser matchUser = this.Users[i];
				PlayerData playerOrBot = GameHubObject.Hub.Players.GetPlayerOrBot(matchUser.Team, matchUser.Slot);
				TeamKind team = matchUser.Team;
				if (team != TeamKind.Red)
				{
					if (team == TeamKind.Blue)
					{
						list2.Add(matchUser.Id);
						list4.Add(playerOrBot.Name);
						list6.Add(playerOrBot.PlayerTag);
						list8.Add(playerOrBot.PlayerId);
					}
				}
				else
				{
					list.Add(matchUser.Id);
					list3.Add(playerOrBot.Name);
					list5.Add(playerOrBot.PlayerTag);
					list7.Add(playerOrBot.PlayerId);
				}
			}
			this._serverBag.RedTeam = list.ToArray();
			this._serverBag.BluTeam = list2.ToArray();
			this._serverBag.RedTeamPlayerNames = list3.ToArray();
			this._serverBag.BluTeamPlayerNames = list4.ToArray();
			this._serverBag.RedTeamPlayerTags = list5.ToArray();
			this._serverBag.BluTeamPlayerTags = list6.ToArray();
			this._serverBag.RedTeamPlayerIds = list7.ToArray();
			this._serverBag.BluTeamPlayerIds = list8.ToArray();
			this._serverBag.RedTeamName = ((redTeam != null) ? redTeam.Name : string.Empty);
			this._serverBag.BluTeamName = ((blueTeam != null) ? blueTeam.Name : string.Empty);
			this._serverBag.RedTeamIconURL = ((redTeam != null) ? redTeam.ImageUrl : string.Empty);
			this._serverBag.BluTeamIconURL = ((blueTeam != null) ? blueTeam.ImageUrl : string.Empty);
			this.UpdateStatusBag();
		}

		public void OnPlayerAuthentication()
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			List<int> list3 = new List<int>();
			List<int> list4 = new List<int>();
			for (int i = 0; i < this.Users.Length; i++)
			{
				SwordfishConnection.MatchUser matchUser = this.Users[i];
				PlayerData playerOrBot = GameHubObject.Hub.Players.GetPlayerOrBot(matchUser.Team, matchUser.Slot);
				TeamKind team = matchUser.Team;
				if (team != TeamKind.Red)
				{
					if (team == TeamKind.Blue)
					{
						list2.Add(playerOrBot.PublisherUserName);
						list4.Add(playerOrBot.PublisherId);
					}
				}
				else
				{
					list.Add(playerOrBot.PublisherUserName);
					list3.Add(playerOrBot.PublisherId);
				}
			}
			this._serverBag.RedTeamPublisherUserNames = list.ToArray();
			this._serverBag.BluTeamPublisherUserNames = list2.ToArray();
			this._serverBag.RedTeamPublisherIds = list3.ToArray();
			this._serverBag.BluTeamPublisherIds = list4.ToArray();
			this.UpdateStatusBag();
		}

		private void CheckUpdateStatus()
		{
			if (!this.Connected)
			{
				return;
			}
			bool flag = false;
			if (this._serverBag == null)
			{
				flag = true;
				this._serverBag = new ServerStatusBag
				{
					ServerPhase = 0,
					MatchId = this.ServerMatchId
				};
				this._serverBag.SetDate(DateTime.UtcNow);
			}
			ServerStatusBag.ServerPhaseKind serverPhase = this._serverBag.ServerPhase;
			GameState.GameStateKind stateKind = GameHubObject.Hub.State.Current.StateKind;
			if (stateKind != GameState.GameStateKind.Pick)
			{
				if (stateKind != GameState.GameStateKind.Game)
				{
					if (stateKind == GameState.GameStateKind.GameWrapUp)
					{
						this._serverBag.ServerPhase = 3;
					}
				}
				else
				{
					this._serverBag.ServerPhase = 2;
				}
			}
			else
			{
				this._serverBag.ServerPhase = 1;
			}
			if (serverPhase != this._serverBag.ServerPhase)
			{
				this._serverBag.SetDate(DateTime.UtcNow);
				flag = true;
			}
			if (GameHubObject.Hub.Players.Narrators.Count != this._serverBag.StorytellerCount)
			{
				this._serverBag.StorytellerCount = GameHubObject.Hub.Players.Narrators.Count;
				flag = true;
			}
			if (flag)
			{
				this.UpdateStatusBag();
			}
		}

		private void UpdateStatusBag()
		{
			if (!GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				this._comm.UpdateGameServerStatus(this._serverBag);
			}
			SwordfishConnection.Log.InfoFormat("StatusBag={0}", new object[]
			{
				this._serverBag
			});
		}

		public string GetIp()
		{
			if (this.Clustered)
			{
				return GameHubObject.Hub.Server.ServerIp;
			}
			string result = "?";
			IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
			for (int i = 0; i < hostEntry.AddressList.Length; i++)
			{
				IPAddress ipaddress = hostEntry.AddressList[i];
				if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
				{
					result = ipaddress.ToString();
				}
			}
			return result;
		}

		public int GetPort()
		{
			if (this.Clustered)
			{
				return this._comm.GetPort();
			}
			return GameHubObject.Hub.Config.GetIntValue(ConfigAccess.ServerPort);
		}

		public long GetRegionId()
		{
			if (this.Clustered)
			{
				return this._comm.GetRegionId();
			}
			return -1L;
		}

		public void RaisePriority()
		{
			if (this.Clustered)
			{
				this._comm.RaisePriority();
			}
		}

		public void LowerPriority()
		{
			if (this.Clustered)
			{
				this._comm.LowerPriority();
			}
		}

		public void SetOnlinePlayersInGame(int count)
		{
			this._comm.UpdateOnlineUsers(count);
		}

		public void SetIsFirstLogin(bool isFirstLogin)
		{
			this.IsFirstLogin = isFirstLogin;
		}

		public void SetJoinedQueue()
		{
			this.PlayerEverJoinedQueue = true;
		}

		public void JobDone()
		{
			if (this._comm != null)
			{
				this._comm.JobDone();
			}
		}

		public void Dispose()
		{
			if (this._disposed)
			{
				return;
			}
			this._disposed = true;
			if (this.Clustered)
			{
				this._comm.Dispose();
			}
		}

		~SwordfishConnection()
		{
			this.Dispose();
		}

		private readonly IGetParentalControlSettings _getParentalControlSettings;

		private readonly DiContainer _container;

		private ISetServerRegion _setServerRegion;

		private IPublisher _publisher;

		public static readonly BitLogger Log = new BitLogger(typeof(SwordfishConnection));

		public int MsgsPerFrame = 100;

		private bool _clustered;

		public long JobId;

		private SwordfishComm _comm;

		private bool _connected;

		private string _sessionId;

		private Guid _serverMatchId;

		public SwordfishConnection.MatchUser[] Users;

		public bool ServerConfiguredBySwordfish;

		private float _nextLog;

		private float LogPeriod = 180f;

		private ServerStatusBag _serverBag;

		private volatile bool _disposed;

		public delegate void OnSwordfishConnected();

		public struct MatchUser
		{
			public string Id;

			public TeamKind Team;

			public int Slot;
		}
	}
}
