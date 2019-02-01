using System;
using System.Collections;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI;
using ClientAPI.Service;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Server
{
	[Serializable]
	public class ServerStartup : GameState
	{
		protected override void OnStateEnabled()
		{
			this._hub = GameHubBehaviour.Hub;
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				return;
			}
			this.ConnectToSwordfish();
		}

		private void ConnectToSwordfish()
		{
			this._hub.ClientApi = new SwordfishClientApi(false, null);
			this._hub.ClientApi.BaseUrl = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFBaseUrl);
			this._hub.ClientApi.RequestTimeOut = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.SFTimeout);
			this._hub.UserService = new UserService(this._hub.ClientApi);
			this._hub.UserService.ServerLogin(null, GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFGameUser), GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.SFGamePass), new SwordfishClientApi.ParameterizedCallback<string>(this.OnServerConnected), new SwordfishClientApi.ErrorCallback(this.OnFailToConnect));
			this.Phase = ServerStartup.StartupPhase.ConnectingToSwordfish;
		}

		private void OnFailToConnect(object state, Exception exception)
		{
			ServerStartup.Log.Error("Failed to connect server to Swordfish, shutting down.", exception);
			GameHubBehaviour.Hub.Quit();
		}

		private void OnServerConnected(object state, string sessionId)
		{
			this._hub.Swordfish.Connection.SessionId = sessionId;
			this.GetMatchData();
			this._hub.GetSWFVersion(delegate
			{
			}, delegate(object o, Exception e)
			{
				ServerStartup.Log.Error(o, e);
			});
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
						this._hub.Players.Players[0].SetCharacter(component.MainAttributes.CharacterId);
						this._hub.Players.UpdatePlayers();
					}
					GameHubBehaviour.Hub.AuthMan.LogGameReadyReason();
					this.Phase = ServerStartup.StartupPhase.Countdown;
					this._countdownTime = Time.time + this.CountdownTime;
				}
				break;
			case ServerStartup.StartupPhase.Countdown:
				if (Time.time > this._countdownTime)
				{
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
				base.GoToState(this.PickModeState, false);
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
			GameArenaInfo[] arenas = this._hub.ArenaConfig.Arenas;
			if (arenaIndex > -1 && arenaIndex < arenas.Length && !arenas[arenaIndex].SceneName.Equals(this._hub.SharedConfigs.TutorialConfig.TutorialSceneName))
			{
				redTeamBotsCount = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.RedTeamBotsCount);
				bluTeamBotsCount = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.BluTeamBotsCount);
			}
			this._hub.Match.FeedData(this.TestPlayerCount + redTeamBotsCount + bluTeamBotsCount, arenaIndex, mode);
			this._serverReady = true;
			yield break;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ServerStartup));

		public PickModeServerSetup PickModeState;

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
