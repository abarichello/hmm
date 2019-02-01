using System;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class SpectatorMenu : GameHubBehaviour
	{
		private void Awake()
		{
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.EnableNarrator))
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		private void Update()
		{
			if (this._clientApi == null)
			{
				return;
			}
			int num = 0;
			while (num++ < 100 && this._clientApi.GetMessageCount() > 0)
			{
				this._clientApi.ProcessNextMessage();
			}
		}

		public void ConnectAPI()
		{
			this._clientApi = new SwordfishClientApi(false, null);
			this._clientApi.BaseUrl = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFBaseUrl);
			this._clientApi.RequestTimeOut = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.SFTimeout);
			this._userService = new UserService(this._clientApi);
			this._userService.ServerLogin(null, GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFGameUser), GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFGamePass), new SwordfishClientApi.ParameterizedCallback<string>(this.OnServerConnected), new SwordfishClientApi.ErrorCallback(this.OnFailToConnect));
		}

		private void OnFailToConnect(object state, Exception exception)
		{
			SpectatorMenu.Log.Fatal("Failed to connect to SF", exception);
		}

		private void OnServerConnected(object state, string obj)
		{
		}

		public void RefreshList()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.FakeList();
				return;
			}
			if (!GameHubBehaviour.Hub.ClientApi.IsLogged)
			{
				return;
			}
			this._clientApi.cluster.GetGameServersRunning(null, GameHubBehaviour.Hub.ClientApi.GetCurrentRegionName(), new SwordfishClientApi.ParameterizedCallback<GameServerInfo[]>(this.OnServersTaken), new SwordfishClientApi.ErrorCallback(this.OnServersTakenError));
		}

		private void FakeList()
		{
			GameServerInfo gameServerInfo = new GameServerInfo
			{
				Ip = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.ServerIP),
				Port = new int?(GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.ServerPort))
			};
			ServerStatusBag serverStatusBag = new ServerStatusBag
			{
				BluTeam = new string[]
				{
					"-1",
					"-1",
					"-1",
					"-1"
				},
				RedTeam = new string[]
				{
					"123",
					"-1",
					"-1",
					"-1"
				},
				ServerPhase = ServerStatusBag.ServerPhaseKind.StartUp
			};
			serverStatusBag.SetDate(DateTime.UtcNow);
			gameServerInfo.GameServerStatus = serverStatusBag.ToString();
			this._servers = new GameServerInfo[UnityEngine.Random.Range(1, 4)];
			for (int i = 0; i < this._servers.Length; i++)
			{
				this._servers[i] = gameServerInfo;
			}
		}

		private void OnServersTakenError(object state, Exception exception)
		{
			SpectatorMenu.Log.Fatal("Failed to get game servers running", exception);
			this._servers = null;
		}

		private void OnServersTaken(object state, GameServerInfo[] obj)
		{
			this._servers = obj;
		}

		public void ConnectToServer(GameServerInfo info, TeamKind team)
		{
			if (info.Port == null)
			{
				SpectatorMenu.Log.ErrorFormat("Cannot connect to server={0}, server port not set status={1}", new object[]
				{
					info.Ip,
					info.GameServerStatus
				});
				return;
			}
			GameHubBehaviour.Hub.Server.ServerIp = info.Ip;
			GameHubBehaviour.Hub.Server.ServerPort = info.Port.Value;
			GameHubBehaviour.Hub.User.ConnectNarratorToServer(true, delegate
			{
				GameHubBehaviour.Hub.State.GotoState(GameHubBehaviour.Hub.State.Current, false);
			}, null);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SpectatorMenu));

		private GameServerInfo[] _servers;

		private SwordfishClientApi _clientApi;

		private UserService _userService;
	}
}
