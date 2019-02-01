using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishConnection : GameHubObject
	{
		public SwordfishConnection() : this(Environment.GetCommandLineArgs())
		{
		}

		public SwordfishConnection(string[] args)
		{
			this.InitializeClientApi();
			if (GameHubObject.Hub.Net.IsClient())
			{
				return;
			}
			NativePlugins.Instance.BeforeAppFinish += this.Dispose;
			this.InitializeMatchData(args);
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
				if (this._connected && this.ListenToSwordfishConnected != null)
				{
					this.ListenToSwordfishConnected();
				}
			}
		}

		public Guid ServerMatchId
		{
			get
			{
				return this._serverMatchId;
			}
		}

		private void InitializeMatchData(string[] args)
		{
			GameHubObject.Hub.Match.ArenaIndex = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.ArenaIndex);
			MatchData.MatchKind kind = GameHubObject.Hub.Match.Kind;
			string[] array;
			string[] array2;
			string[] array3;
			SwordfishConnectionArgsParser.ParseMatchArgs(args, out array, out array2, out array3, ref this.JobId, ref this._serverMatchId, ref this.ServerConfiguredBySwordfish, ref GameHubObject.Hub.Server.ServerIp, ref GameHubObject.Hub.Server.ServerPort, ref kind, ref GameHubObject.Hub.Match.ArenaIndex);
			GameHubObject.Hub.Match.Kind = kind;
			this._clustered = (this.JobId > 0L);
			List<SwordfishConnection.MatchUser> list = new List<SwordfishConnection.MatchUser>();
			if (array2 != null)
			{
				this.AddUsers(array2, TeamKind.Blue, ref list, "blue players");
			}
			if (array != null)
			{
				this.AddUsers(array, TeamKind.Red, ref list, "Red players");
			}
			if (array3 != null)
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
			SwordfishBadNetworkConditionConfig badNetConfig = SwordfishConnection.CreateClientApiLagConfiguration();
			GameHubObject.Hub.ClientApi = new SwordfishClientApi(GameHubObject.Hub.Net.IsClient(), badNetConfig);
			string text = Language.CurrentLanguage().ToString().ToLower();
			GameHubObject.Hub.ClientApi.SetLanguageCode(text);
			GameHubObject.Hub.ClientApi.BaseUrl = GameHubObject.Hub.Config.GetValue(ConfigAccess.SFBaseUrl);
			GameHubObject.Hub.ClientApi.RequestTimeOut = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.SFTimeout);
			GameHubObject.Hub.ClientApi.RequestMatchmakingTimeOut = GameHubObject.Hub.Config.GetIntValue(ConfigAccess.SFMatchMakingTimeout);
			GameHubObject.Hub.ClientApi.UserAccessControlCallback += this.ClientApiOnUserAccessControlCallback;
			GameHubObject.Hub.UserService = new UserService(GameHubObject.Hub.ClientApi);
			SwordfishConnection.Log.InfoFormat("Language:{0} GameVersion:{1} Swordfish URL={2} PID={3}", new object[]
			{
				text,
				"2.07.972",
				GameHubObject.Hub.ClientApi.BaseUrl,
				Process.GetCurrentProcess().Id
			});
			if (GameHubObject.Hub.Net.IsClient())
			{
				SwordfishConnection.InitializeClientApiClientSide();
			}
		}

		private static void InitializeClientApiClientSide()
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

		private void ConnectionLost()
		{
			SwordfishConnection.Log.Warn("CONNECTION LOST");
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = string.Format(Language.Get("LostMessageHubConnection", TranslationSheets.GUI), new object[0]),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					GameHubObject.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					GameHubObject.Hub.Quit();
				}
			};
			GameHubObject.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void RegisterConnectionMonitoring()
		{
			GameHubObject.Hub.ClientApi.internet.Offline += this.ConnectionLost;
		}

		public void DeregisterConnectionMonitoring()
		{
			GameHubObject.Hub.ClientApi.internet.Offline -= this.ConnectionLost;
		}

		public void Update()
		{
			int num = 0;
			int num2 = 0;
			if (this._nextLog < Time.time)
			{
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
					ServerPhase = ServerStatusBag.ServerPhaseKind.StartUp
				};
			}
			ServerStatusBag.ServerPhaseKind serverPhase = this._serverBag.ServerPhase;
			GameState.GameStateKind stateKind = GameHubObject.Hub.State.Current.StateKind;
			if (stateKind != GameState.GameStateKind.Pick)
			{
				if (stateKind != GameState.GameStateKind.Game)
				{
					if (stateKind == GameState.GameStateKind.GameWrapUp)
					{
						this._serverBag.ServerPhase = ServerStatusBag.ServerPhaseKind.WrapUp;
					}
				}
				else
				{
					this._serverBag.ServerPhase = ServerStatusBag.ServerPhaseKind.GameRunning;
				}
			}
			else
			{
				this._serverBag.ServerPhase = ServerStatusBag.ServerPhaseKind.PickMode;
			}
			flag |= (serverPhase != this._serverBag.ServerPhase);
			if (this._serverBag.RedTeam == null || this._serverBag.BluTeam == null)
			{
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				for (int i = 0; i < this.Users.Length; i++)
				{
					SwordfishConnection.MatchUser matchUser = this.Users[i];
					TeamKind team = matchUser.Team;
					if (team != TeamKind.Red)
					{
						if (team == TeamKind.Blue)
						{
							list2.Add(matchUser.Id);
						}
					}
					else
					{
						list.Add(matchUser.Id);
					}
				}
				this._serverBag.RedTeam = list.ToArray();
				this._serverBag.BluTeam = list2.ToArray();
				flag = true;
			}
			if (flag)
			{
				this._serverBag.SetDate(DateTime.UtcNow);
				this._comm.UpdateGameServerStatus(this._serverBag);
			}
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
			return GameHubObject.Hub.Config.GetIntValue(ConfigAccess.ServerPort, 9696);
		}

		public void RaisePriority()
		{
			if (this.Clustered)
			{
				this._comm.RaisePriority();
			}
		}

		public void SetOnlinePlayersInGame(int count)
		{
			SwordfishComm.UpdateOnlineUsers(this.JobId, count);
		}

		public void Dispose()
		{
			if (this._disposed)
			{
				return;
			}
			if (this.Clustered)
			{
				this._comm.Dispose();
			}
			this._disposed = true;
		}

		~SwordfishConnection()
		{
			this.Dispose();
		}

		private void ClientApiOnUserAccessControlCallback(UserAccessControlMessage uacmessage)
		{
			GameHubObject.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(uacmessage.Message, Language.Get("Ok", "GUI"), delegate()
			{
				try
				{
					if (!GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
					{
						GameHubObject.Hub.Swordfish.Msg.Cleanup();
					}
				}
				catch (Exception ex)
				{
				}
				GameHubObject.Hub.Quit();
			});
		}

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

		private bool _disposed;

		public delegate void OnSwordfishConnected();

		public struct MatchUser
		{
			public string Id;

			public TeamKind Team;

			public int Slot;
		}
	}
}
