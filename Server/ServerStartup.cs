using System;
using System.Collections;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI;
using ClientAPI.Objects;
using ClientAPI.Service;
using HeavyMetalMachines.CharacterSelection.Server;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Matches.API;
using HeavyMetalMachines.Swordfish;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Server
{
	[Serializable]
	public class ServerStartup : GameState, IObserveClientsConnection
	{
		protected override void OnStateEnabled()
		{
			try
			{
				this._initializeLocalization.Initialize();
				this._swordfishServices.Initialize();
				this._hub = GameHubBehaviour.Hub;
				if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
				{
					this.ConnectToSwordfish();
				}
			}
			catch (Exception e)
			{
				ServerStartup.Log.Fatal("Failed to initialize server, quitting", e);
				ServerEmergencyQuit.Quit();
			}
		}

		private void ConnectToSwordfish()
		{
			this._hub.ClientApi = new SwordfishClientApi(false, null);
			this._hub.ClientApi.BaseUrl = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFBaseUrl);
			this._hub.ClientApi.RequestTimeOut = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.SFTimeout);
			this._hub.UserService = new UserService(this._hub.ClientApi);
			ServerStartup.Log.DebugFormat("Will connect to swordfish:{0}", new object[]
			{
				this._hub.ClientApi.BaseUrl
			});
			this._hub.UserService.ServerLogin(null, GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFGameUser), GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFGamePass), new SwordfishClientApi.ParameterizedCallback<ServerLoginInfo>(this.OnServerConnected), new SwordfishClientApi.ErrorCallback(this.OnFailToConnect));
			this.Phase = ServerStartup.StartupPhase.ConnectingToSwordfish;
		}

		private void OnFailToConnect(object state, Exception exception)
		{
			ServerStartup.Log.Error("Failed to connect server to Swordfish, shutting down.", exception);
			GameHubBehaviour.Hub.Quit(13);
		}

		private void OnServerConnected(object state, ServerLoginInfo loginInfo)
		{
			this._hub.Swordfish.Connection.SessionId = loginInfo.WsToken;
			this._hub.Swordfish.Connection.RaiseConnected();
			ServerStartup.Log.DebugFormat("Server connected session={0}", new object[]
			{
				this._hub.Swordfish.Connection.SessionId
			});
			this.GetMatchData();
			this._hub.CheckClientAndServerSwordfishApiVersion(new Action(this.OnCheckClientAndServerSwordfishApiVersionMatching), new Action(this.OnCheckClientAndServerSwordfishApiVersionUnmatching), new SwordfishClientApi.ErrorCallback(this.OnCheckClientAndServerSwordfishApiVersionError));
		}

		private void OnCheckClientAndServerSwordfishApiVersionMatching()
		{
			ServerStartup.Log.Debug("Client and server swordfish api versions are matching.");
		}

		private void OnCheckClientAndServerSwordfishApiVersionUnmatching()
		{
			ServerStartup.Log.Debug("Client and server swordfish api versions are not matching.");
		}

		private void OnCheckClientAndServerSwordfishApiVersionError(object state, Exception exception)
		{
			ServerStartup.Log.ErrorStackTrace("OnCheckGameAndServerSwordfishApiVersionError");
		}

		private void GetMatchData()
		{
			if (!GameHubBehaviour.Hub.Swordfish.Connection.ServerConfiguredBySwordfish)
			{
				int intValue = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.PlayerCount);
				int intValue2 = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.ArenaIndex);
				PickMode pick = (PickMode)GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.PickMode, 1);
				GameHubBehaviour.Hub.Match.FeedData(intValue, intValue2, pick);
			}
			ServerStartup.Log.DebugFormat("Swordfish port taken={0}", new object[]
			{
				GameHubBehaviour.Hub.Server.ServerPort
			});
			this.OpenServer();
			this.Phase = ServerStartup.StartupPhase.AwaitingPlayers;
		}

		private void Update()
		{
			switch (this.Phase)
			{
			case ServerStartup.StartupPhase.None:
				base.StartCoroutine(this.FetchServerInfo());
				this.Phase = ServerStartup.StartupPhase.FetchInfo;
				break;
			case ServerStartup.StartupPhase.FetchInfo:
				if (this._serverReady)
				{
					this.Phase = ServerStartup.StartupPhase.AwaitingPlayers;
					this.OpenServer();
					if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
					{
						GameHubBehaviour.Hub.AuthMan.CreateBots();
					}
				}
				break;
			case ServerStartup.StartupPhase.AwaitingPlayers:
				if (GameHubBehaviour.Hub.AuthMan.GameReady())
				{
					if (this._hub.Match.LevelIsTutorial())
					{
						this._hub.Players.Players[0].GridIndex = 0;
						ItemTypeScriptableObject itemTypeScriptableObject = GameHubBehaviour.Hub.InventoryColletion.AllItemTypes[GameHubBehaviour.Hub.SharedConfigs.TutorialConfig.PlayerCharacterGuid];
						CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
						this._hub.Players.Players[0].SetCharacter(component.CharacterId, GameHubBehaviour.Hub.InventoryColletion);
						this._playersDispatcher.UpdatePlayers();
					}
					GameHubBehaviour.Hub.AuthMan.LogGameReadyReason();
					this.Phase = ServerStartup.StartupPhase.Countdown;
					this._countdownTime = Time.time + this.CountdownTime;
					ServerStartup.Log.DebugFormat("Match start in {0}s", new object[]
					{
						this.CountdownTime
					});
				}
				break;
			case ServerStartup.StartupPhase.Countdown:
				if (Time.time > this._countdownTime)
				{
					ServerStartup.Log.DebugFormat("Match start Now!", new object[]
					{
						this.CountdownTime
					});
					this.Phase = ServerStartup.StartupPhase.StartGameMode;
				}
				break;
			case ServerStartup.StartupPhase.StartGameMode:
				this.StartGameMode();
				this.Phase = ServerStartup.StartupPhase.Done;
				break;
			}
		}

		private void OpenServer()
		{
			NetworkServer networkServer = (NetworkServer)GameHubBehaviour.Hub.Net;
			ServerInfo server = this._hub.Server;
			networkServer.StartServer(server.ServerPort);
			this._hub.Match.State = ((!this._hub.Match.LevelIsTutorial()) ? MatchData.MatchState.AwaitingConnections : MatchData.MatchState.Tutorial);
		}

		private void StartGameMode()
		{
			if (this._hub.Match.LevelIsTutorial())
			{
				this._hub.Match.State = MatchData.MatchState.Tutorial;
				base.GoToState(this.ServerLoadingState, false);
			}
			else
			{
				this._proceedToServerCharacterSelectionState.Proceed();
			}
		}

		private IEnumerator FetchServerInfo()
		{
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				yield break;
			}
			ServerInfo server = this._hub.Server;
			this.TestPlayerCount = this._hub.Config.GetIntValue(ConfigAccess.PlayerCount);
			int arenaIndex = this._hub.Config.GetIntValue(ConfigAccess.ArenaIndex);
			PickMode mode = (PickMode)this._hub.Config.GetIntValue(ConfigAccess.PickMode, 1);
			if (this._hub.Config.GetBoolValue(ConfigAccess.IsDebug, false))
			{
				this._serverReady = true;
			}
			while (!this._serverReady)
			{
				yield return null;
			}
			server.ServerPort = this._hub.Config.GetIntValue(ConfigAccess.ServerPort, this.TestServerPort);
			int redTeamBotsCount = 0;
			int bluTeamBotsCount = 0;
			if (arenaIndex > -1 && arenaIndex < this._hub.ArenaConfig.GetNumberOfArenas() && !this._hub.ArenaConfig.GetArenaByIndex(arenaIndex).SceneName.Equals(this._hub.SharedConfigs.TutorialConfig.TutorialSceneName))
			{
				redTeamBotsCount = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.RedTeamBotsCount);
				bluTeamBotsCount = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.BluTeamBotsCount);
			}
			this._hub.Match.FeedData(this.TestPlayerCount + redTeamBotsCount + bluTeamBotsCount, arenaIndex, mode);
			this._serverReady = true;
			yield break;
		}

		public IObservable<MatchClient> OnClientDisconnected()
		{
			throw new NotImplementedException();
		}

		public IObservable<MatchClient> OnClientReconnected()
		{
			throw new NotImplementedException();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ServerStartup));

		[SerializeField]
		private SwordfishServices _swordfishServices;

		[Inject]
		private IMatchPlayersDispatcher _playersDispatcher;

		[Inject]
		private IInitializeLocalization _initializeLocalization;

		[Inject]
		private IProceedToServerCharacterSelectionState _proceedToServerCharacterSelectionState;

		public ServerGame ServerGameState;

		public LoadingState ServerLoadingState;

		public ServerRelay Relay;

		private HMMHub _hub;

		private bool _serverReady;

		public ServerStartup.StartupPhase Phase;

		private float _countdownTime;

		public float CountdownTime = 5f;

		public int TestServerPort = 9696;

		public int TestPlayerCount = 1;

		public enum StartupPhase
		{
			None,
			ConnectingToSwordfish,
			FetchInfo,
			AwaitingPlayers,
			Countdown,
			StartGameMode,
			Done
		}
	}
}
