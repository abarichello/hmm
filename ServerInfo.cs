using System;
using System.Collections;
using System.Diagnostics;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish.Logs;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using Pocketverse.MuralContext;
using Steamworks;
using UnityEngine;

namespace HeavyMetalMachines
{
	[RemoteClass]
	public class ServerInfo : GameHubBehaviour, ICleanupListener, IBitComponent
	{
		public bool Ready { get; private set; }

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ServerInfo.MatchStateChanged OnMatchStateChanged;

		[RemoteMethod]
		public void SetInfo(MatchData data)
		{
			ServerInfo.Log.InfoFormat("SetInfo:{0}", new object[]
			{
				data
			});
			if (this.ConnectStarted)
			{
				PlaybackSystem.Stop();
				this.ConnectStarted = false;
			}
			this.ClientSetInfo(data);
		}

		public void ClientSetInfo(MatchData data)
		{
			GameHubBehaviour.Hub.Match = data;
			ServerInfo.Log.InfoFormat("Received={0}", new object[]
			{
				GameHubBehaviour.Hub.Match
			});
			if (this._oldState != GameHubBehaviour.Hub.Match.State && this.OnMatchStateChanged != null)
			{
				this._oldState = GameHubBehaviour.Hub.Match.State;
				this.OnMatchStateChanged(GameHubBehaviour.Hub.Match.State);
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<RewardsBag> RewardSetEvent;

		[RemoteMethod]
		private void SetPlayerRewards(string rewardString)
		{
			if (this.RewardSetEvent != null)
			{
				this.RewardSetEvent((RewardsBag)((JsonSerializeable<T>)rewardString));
			}
		}

		public void LoadLevel(Action loadCallback)
		{
			int arenaIndex = GameHubBehaviour.Hub.Match.ArenaIndex;
			string sceneName = GameHubBehaviour.Hub.ArenaConfig.GetSceneName(arenaIndex);
			base.StartCoroutine(this.DoLoadLevel(sceneName, loadCallback));
			this.Ready = false;
		}

		public IEnumerator DoLoadLevel(string level, Action loadCallback)
		{
			AsyncOperation loading = Application.LoadLevelAsync(level);
			yield return loading;
			if (loadCallback != null)
			{
				loadCallback();
			}
			yield break;
		}

		public void GetEvents(Action whenLoaded, Action whenExec)
		{
			this._whenLoadedCallback = whenLoaded;
			this._whenExecutedCallback = whenExec;
			this.DispatchReliable(new byte[0]).ServerEventRequest();
		}

		[RemoteMethod]
		public void PlaybackReady(long playbackStartTime, int lastSynchTimeScaleChange, int accumulatedSynchDelay, float timeScale)
		{
			if (this._whenLoadedCallback != null)
			{
				this._whenLoadedCallback();
			}
			GameHubBehaviour.Hub.GameTime.SetTimeZero(playbackStartTime, lastSynchTimeScaleChange, accumulatedSynchDelay, timeScale);
		}

		[RemoteMethod]
		public void FullDataSent()
		{
			if (this._whenExecutedCallback != null)
			{
				this._whenExecutedCallback();
			}
		}

		[RemoteMethod]
		public void ServerSet()
		{
			this.Ready = true;
		}

		[RemoteMethod]
		public void ServerPlayerLoadingInfo(long playerId, float progress)
		{
			GameHubBehaviour.Hub.Match.LoadingProgress.UpdatePlayerProgress(playerId, progress);
		}

		public void ClientSendPlayerLoadingInfo(float progress)
		{
			if (GameHubBehaviour.Hub.User.IsNarrator)
			{
				return;
			}
			this.DispatchReliable(new byte[0]).ServerPlayerLoadingUpdate(progress);
		}

		[RemoteMethod]
		public void ServerPlayerDisconnectInfo()
		{
			GameHubBehaviour.Hub.Heart.DisconnectPlayerOnServer(this.Sender, Heartbeat.DisconnectReason.PlayerDisconnect);
		}

		public void ClientSendPlayerDisconnectInfo()
		{
			this.DispatchReliable(new byte[0]).ServerPlayerDisconnectInfo();
			if (GameHubBehaviour.Hub.Net is NetworkClient)
			{
				((NetworkClient)GameHubBehaviour.Hub.Net).CloseConnection();
			}
		}

		public void ClientLeaverWarningCallback(bool timedOut)
		{
			this.DispatchReliable(new byte[0]).ServerLeaverWarningCallback(timedOut);
		}

		public void ClientSendInputPressed()
		{
			this.DispatchReliable(new byte[0]).ServerPlayerInputPressed();
		}

		[RemoteMethod]
		public void ClientPlayerAFKTimeUpdate(float afkRemainingTime)
		{
			GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.AFKTimeUpdate(afkRemainingTime);
		}

		public void ClientReloadAFKTime()
		{
			this.DispatchReliable(new byte[0]).ServerReloadAFKTime();
		}

		public void SpreadInfo()
		{
			this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(0)).SetInfo(GameHubBehaviour.Hub.Match);
		}

		[RemoteMethod]
		public void ServerEventRequest()
		{
			base.StartCoroutine(this.ReconnectPlayer());
		}

		private IEnumerator ReconnectPlayer()
		{
			byte playerAddress = this.Sender;
			PlayerData player = GameHubBehaviour.Hub.Players.GetPlayerByAddress(playerAddress);
			GameHubBehaviour.Hub.Players.SendPlayers(playerAddress);
			IServerInfoAsync async = this.Async(playerAddress);
			async.CallbackTimeoutMillis = 60000;
			IFuture sendInfo = async.SetInfo(GameHubBehaviour.Hub.Match);
			async.CallbackTimeoutMillis = 0;
			while (!sendInfo.IsDone)
			{
				yield return null;
			}
			PauseController.Instance.SendCurrentPauseData(playerAddress);
			MatchLogWriter.PlayerAction(LogAction.GameServerMapLoadFinished, player.PlayerCarId, "LevelLoaded");
			if (player.IsNarrator)
			{
				bool waitingPlayerCarFactoryFinish = true;
				while (waitingPlayerCarFactoryFinish)
				{
					yield return null;
					waitingPlayerCarFactoryFinish = false;
					for (int i = 0; i < GameHubBehaviour.Hub.Players.PlayersAndBots.Count; i++)
					{
						waitingPlayerCarFactoryFinish |= (GameHubBehaviour.Hub.Players.PlayersAndBots[i].CharacterInstance == null);
					}
				}
			}
			while (!player.IsNarrator && player.CharacterInstance == null)
			{
				yield return null;
			}
			player.AddOnline();
			while (GameHubBehaviour.Hub.GameTime.PlaybackStartTime < 0L)
			{
				yield return null;
			}
			this.DispatchReliable(new byte[]
			{
				playerAddress
			}).PlaybackReady(GameHubBehaviour.Hub.GameTime.PlaybackStartTime, GameHubBehaviour.Hub.GameTime.LastSynchTimeScaleChange, GameHubBehaviour.Hub.GameTime.AccumulatedSynchDelay, Time.timeScale);
			PlaybackManager.Scoreboard.SendFull(playerAddress);
			GameHubBehaviour.Hub.Events.SendFullFrame(playerAddress);
			PlaybackManager.CombatStates.SendFullData(playerAddress);
			PlaybackManager.CombatFeedbacks.SendFullData(playerAddress);
			PlaybackManager.PlayerStats.SendFullUpdate(playerAddress);
			PlaybackManager.GadgetLevel.SendFullData(playerAddress);
			PlaybackManager.GadgetEvent.SendAllEvents(playerAddress);
			PlaybackManager.BombInstance.UpdateDataTo(playerAddress);
			this.DispatchReliable(new byte[]
			{
				playerAddress
			}).FullDataSent();
			yield break;
		}

		[RemoteMethod]
		private void OnServerPlayerReady()
		{
			byte sender = this.Sender;
			int num = 0;
			PlayerData anyByAddress = GameHubBehaviour.Hub.Players.GetAnyByAddress(sender);
			anyByAddress.Ready = true;
			for (int i = 0; i < GameHubBehaviour.Hub.Players.Players.Count; i++)
			{
				PlayerData playerData = GameHubBehaviour.Hub.Players.Players[i];
				if (!playerData.Ready)
				{
					num++;
				}
			}
			for (int j = 0; j < GameHubBehaviour.Hub.Players.Narrators.Count; j++)
			{
				PlayerData playerData2 = GameHubBehaviour.Hub.Players.Narrators[j];
				if (!playerData2.Ready)
				{
					num++;
				}
			}
			if (this._playersReadySentEvent)
			{
				this.DispatchReliable(new byte[]
				{
					sender
				}).ServerSet();
				return;
			}
			if (num > 0)
			{
				if (!this._playersReadyCoroutineTimeout)
				{
					this._playersReadyCoroutineTimeout = true;
					base.StartCoroutine(this.CheckPlayersReadyEventTimeout());
				}
				return;
			}
			this.SendServerSetEventToAllClients();
		}

		private IEnumerator CheckPlayersReadyEventTimeout()
		{
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime((float)this._playersReadyTotalTimeout));
			if (this._playersReadySentEvent)
			{
				yield break;
			}
			this.SendServerSetEventToAllClients();
			yield break;
		}

		private void SendServerSetEventToAllClients()
		{
			if (this._playersReadySentEvent)
			{
				return;
			}
			this._playersReadySentEvent = true;
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).ServerSet();
			GameHubBehaviour.Hub.Swordfish.Connection.RaisePriority();
			this.Ready = true;
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				GameHubBehaviour.Hub.MatchMan.CanStartMatch = true;
			}
		}

		public void SendPlayerLoadingInfoToAllPlayers(long playerId, float progress)
		{
			this.DispatchReliable(GameHubBehaviour.Hub.SendAll).ServerPlayerLoadingInfo(playerId, progress);
		}

		[RemoteMethod]
		public void ServerPlayerLoadingUpdate(float progress)
		{
			byte sender = this.Sender;
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(sender);
			this.SendPlayerLoadingInfoToAllPlayers(playerByAddress.PlayerId, progress);
		}

		public void ServerSendAFKTimeUpdate(byte playerAddress, float afkTime)
		{
			this.DispatchReliable(new byte[]
			{
				playerAddress
			}).ClientPlayerAFKTimeUpdate(afkTime);
		}

		[RemoteMethod]
		public void ServerReloadAFKTime()
		{
			this.ServerSendAFKTimeUpdate(this.Sender, GameHubBehaviour.Hub.afkController.GetAFKTime(this.Sender));
		}

		[RemoteMethod]
		public void ServerPlayerInputPressed()
		{
			GameHubBehaviour.Hub.afkController.InputChanged(GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender));
		}

		[RemoteMethod]
		public void ServerLeaverWarningCallback(bool timedOut)
		{
			GameHubBehaviour.Hub.afkController.LeaverWarningCallback(timedOut, this.Sender);
		}

		public void OnCleanup(CleanupMessage msg)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			this.ServerPort = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.ServerPort);
			this.ServerIp = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.ServerIP);
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this._playersReadyTotalTimeout = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.LoadingTimeoutSeconds);
			}
			if (GameHubBehaviour.Hub.Swordfish.Connection == null)
			{
				ServerInfo.Log.WarnFormat("Swordfish connection is null. Steam is not running: {0}", new object[]
				{
					SteamAPI.IsSteamRunning()
				});
				return;
			}
			if (GameHubBehaviour.Hub.Swordfish.Connection.Clustered)
			{
				return;
			}
			this.ServerPort = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.ServerPort);
			this.ServerIp = GameHubBehaviour.Hub.Config.GetValue(ConfigAccess.ServerIP);
		}

		private int OID
		{
			get
			{
				if (!this._identifiable)
				{
					this._identifiable = base.GetComponent<Identifiable>();
				}
				return this._identifiable.ObjId;
			}
		}

		public byte Sender { get; set; }

		public IServerInfoAsync Async()
		{
			return this.Async(0);
		}

		public IServerInfoAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new ServerInfoAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IServerInfoDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new ServerInfoDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IServerInfoDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new ServerInfoDispatch(this.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		protected IFuture Delayed
		{
			get
			{
				return this._delayed;
			}
		}

		protected void Delay(IFuture future)
		{
			this._delayed = future;
		}

		public object Invoke(int classId, short methodId, object[] args)
		{
			if (classId != 1016)
			{
				throw new Exception("Hierarchy in RemoteClass is not allowed!!! " + classId);
			}
			this._delayed = null;
			switch (methodId)
			{
			case 7:
				this.PlaybackReady((long)args[0], (int)args[1], (int)args[2], (float)args[3]);
				return null;
			case 8:
				this.FullDataSent();
				return null;
			case 9:
				this.ServerSet();
				return null;
			case 10:
				this.ServerPlayerLoadingInfo((long)args[0], (float)args[1]);
				return null;
			default:
				switch (methodId)
				{
				case 25:
					this.ServerPlayerLoadingUpdate((float)args[0]);
					return null;
				default:
					switch (methodId)
					{
					case 1:
						this.SetInfo((MatchData)args[0]);
						return null;
					case 3:
						this.SetPlayerRewards((string)args[0]);
						return null;
					}
					throw new ScriptMethodNotFoundException(classId, (int)methodId);
				case 27:
					this.ServerReloadAFKTime();
					return null;
				case 28:
					this.ServerPlayerInputPressed();
					return null;
				case 29:
					this.ServerLeaverWarningCallback((bool)args[0]);
					return null;
				}
				break;
			case 12:
				this.ServerPlayerDisconnectInfo();
				return null;
			case 16:
				this.ClientPlayerAFKTimeUpdate((float)args[0]);
				return null;
			case 19:
				this.ServerEventRequest();
				return null;
			case 21:
				this.OnServerPlayerReady();
				return null;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(ServerInfo));

		public string ServerIp;

		public int ServerPort;

		public RewardsInfo Rewards;

		public LevelInfo Level;

		public BoostersInfo BoosterInfo;

		public bool ConnectStarted;

		private MatchData.MatchState _oldState;

		private Action _whenLoadedCallback;

		private Action _whenExecutedCallback;

		private bool _playersReadySentEvent;

		private bool _playersReadyCoroutineTimeout;

		private int _playersReadyTotalTimeout;

		public const int StaticClassId = 1016;

		private Identifiable _identifiable;

		[ThreadStatic]
		private ServerInfoAsync _async;

		[ThreadStatic]
		private ServerInfoDispatch _dispatch;

		private IFuture _delayed;

		public delegate void MatchStateChanged(MatchData.MatchState newMatchstate);
	}
}
