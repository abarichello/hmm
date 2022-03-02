using System;
using System.Collections;
using System.Diagnostics;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.CompetitiveMode.DataTransferObjects.Players;
using HeavyMetalMachines.CompetitiveMode.Extensions;
using HeavyMetalMachines.CompetitiveMode.Matchmaking;
using HeavyMetalMachines.CompetitiveMode.Players;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback;
using Hoplon.Serialization;
using Pocketverse;
using Pocketverse.MuralContext;
using Pocketverse.Util;
using UniRx;
using UnityEngine;
using Zenject;

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
				this._playback.Init();
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
		private void SetPlayerCompetitiveState(string state)
		{
			SerializablePlayerCompetitiveState state2 = (SerializablePlayerCompetitiveState)((JsonSerializeable<!0>)state);
			PlayerCompetitiveState playerCompetitiveState = state2.ToPlayerCompetitiveState();
			ServerInfo.Log.DebugFormat("Received updated player competitive state from server. Status = {0} | Score = {1} | State = {2}", new object[]
			{
				playerCompetitiveState.Status,
				playerCompetitiveState.Rank.CurrentRank.Score,
				state
			});
			this._competitiveP2PService.SetPlayerCompetitiveState(playerCompetitiveState);
		}

		[RemoteMethod]
		private void SetPlayerRewards(string rewardString)
		{
			ServerInfo.Log.DebugFormat("Reward pre set to={0}", new object[]
			{
				rewardString
			});
			if (this.RewardSetEvent != null)
			{
				this.RewardSetEvent(JsonSerializeable<RewardsBag>.Deserialize(rewardString));
			}
			ServerInfo.Log.DebugFormat("Reward set to={0}", new object[]
			{
				rewardString
			});
		}

		public void GetEvents(Action whenLoaded, Action whenExec)
		{
			ServerInfo.Log.DebugFormat("LOADING: Will GetEvents IsRunningReplay:{0}", new object[]
			{
				this._playback.IsRunningReplay
			});
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
			ServerInfo.Log.DebugFormat("LOADING: PlaybackReady gameStartedRealTime={0}", new object[]
			{
				playbackStartTime
			});
			GameHubBehaviour.Hub.GameTime.SetTimeZero(playbackStartTime, lastSynchTimeScaleChange, accumulatedSynchDelay, timeScale);
		}

		[RemoteMethod]
		public void FullDataSent()
		{
			ServerInfo.Log.Debug("LOADING: Received all previous data for playback");
			if (this._whenExecutedCallback != null)
			{
				this._whenExecutedCallback();
			}
		}

		[RemoteMethod]
		public void ServerSet()
		{
			ServerInfo.Log.Debug("LOADING: ServerSet");
			this.Ready = true;
			if (this.ShouldTrackPlayerCompetitiveStateProgress())
			{
				IInitializeAndWatchMyPlayerCompetitiveStateProgress initializeAndWatchMyPlayerCompetitiveStateProgress = this._diContainer.Resolve<IInitializeAndWatchMyPlayerCompetitiveStateProgress>();
				ObservableExtensions.Subscribe<Unit>(initializeAndWatchMyPlayerCompetitiveStateProgress.InitializeAndWatch());
			}
		}

		public void ServerSetNotReadyLocal()
		{
			this.Ready = false;
		}

		private bool ShouldTrackPlayerCompetitiveStateProgress()
		{
			IShouldTrackPlayerCompetitiveStateProgress shouldTrackPlayerCompetitiveStateProgress = this._diContainer.Resolve<IShouldTrackPlayerCompetitiveStateProgress>();
			return shouldTrackPlayerCompetitiveStateProgress.Check();
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
			ServerInfo.Log.DebugFormat("Spreading={0} to={1}", new object[]
			{
				GameHubBehaviour.Hub.Match,
				Arrays.ToStringWithComma(GameHubBehaviour.Hub.AddressGroups.GetGroup(0))
			});
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
			ServerInfo.Log.DebugFormat("LOADING: Player getting ready. Address: {0}", new object[]
			{
				playerAddress
			});
			PlayerData player = GameHubBehaviour.Hub.Players.GetPlayerByAddress(playerAddress);
			this._playersDispatcher.SendPlayers(playerAddress);
			this._teamsDispatcher.SendTeams(playerAddress);
			IServerInfoAsync async = this.Async(playerAddress);
			async.CallbackTimeoutMillis = 60000;
			IFuture sendInfo = async.SetInfo(GameHubBehaviour.Hub.Match);
			async.CallbackTimeoutMillis = 0;
			while (!sendInfo.IsDone)
			{
				yield return null;
			}
			ServerInfo.Log.DebugFormat("LOADING: Sending info for reconnecting client. Address: {0}", new object[]
			{
				playerAddress
			});
			PauseController.Instance.SendCurrentPauseData(playerAddress);
			MatchLogWriter.UserAction(5, player.UserId, "LevelLoaded");
			if (player.IsNarrator)
			{
				ServerInfo.Log.DebugFormat("LOADING: Narrator waiting playerCarFactoryFinish. Address: {0}", new object[]
				{
					playerAddress
				});
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
			ServerInfo.Log.DebugFormat("LOADING: Waiting for character. Address: {0}", new object[]
			{
				playerAddress
			});
			while (!player.IsNarrator && player.CharacterInstance == null)
			{
				yield return null;
			}
			ServerInfo.Log.DebugFormat("LOADING: Sending event buffer. Address: {0}", new object[]
			{
				playerAddress
			});
			player.AddOnline();
			ServerInfo.Log.DebugFormat("LOADING: Hub.Clock.PlaybackStartTime: {0}", new object[]
			{
				GameHubBehaviour.Hub.GameTime.PlaybackStartTime
			});
			while (GameHubBehaviour.Hub.GameTime.PlaybackStartTime < 0L)
			{
				yield return null;
			}
			this.DispatchReliable(new byte[]
			{
				playerAddress
			}).PlaybackReady(GameHubBehaviour.Hub.GameTime.PlaybackStartTime, GameHubBehaviour.Hub.GameTime.LastSynchTimeScaleChange, GameHubBehaviour.Hub.GameTime.AccumulatedSynchDelay, Time.timeScale);
			this._scoreboardDispatcher.SendFull(playerAddress);
			this._eventManagerDispatcher.SendFullFrame(playerAddress);
			this._combatStatesDispatcher.SendFullData(playerAddress);
			this._combatFeedbackDispatcher.SendFullData(playerAddress);
			this._statsDispatcher.SendFullUpdate(playerAddress);
			this._gadgetLevelDispatcher.SendFullData(playerAddress);
			this._gadgetEventDispatcher.SendAllEvents(playerAddress);
			this._bombInstanceDispatcher.UpdateDataTo(playerAddress, GameHubBehaviour.Hub.BombManager.ActiveBomb);
			this.DispatchReliable(new byte[]
			{
				playerAddress
			}).FullDataSent();
			ServerInfo.Log.DebugFormat("LOADING: Done. Address: {0}", new object[]
			{
				playerAddress
			});
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
			ServerInfo.Log.DebugFormat("LOADING: Player ready. Address: {0} - Remaining: {1} {2}", new object[]
			{
				sender,
				num,
				this._playersReadySentEvent
			});
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
				this.TryStartPlayersReadyEventTimeout();
				return;
			}
			ServerInfo.Log.Debug("LOADING: All players ready.");
			this.SendServerSetEventToAllClients();
		}

		public void TryStartPlayersReadyEventTimeout()
		{
			if (!this._playersReadyCoroutineTimeout)
			{
				this._playersReadyCoroutineTimeout = true;
				base.StartCoroutine(this.CheckPlayersReadyEventTimeout());
			}
		}

		private IEnumerator CheckPlayersReadyEventTimeout()
		{
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime((float)this._playersReadyTotalTimeout));
			int seconds = 0;
			while (!GameHubBehaviour.Hub.UpdateManager.IsRunning())
			{
				seconds++;
				yield return UnityUtils.WaitForOneSecond;
			}
			if (this._playersReadySentEvent)
			{
				yield break;
			}
			ServerInfo.Log.DebugFormat("LOADING: Timeout while waiting for all players to be ready before sending signal to all clients. {0}", new object[]
			{
				seconds
			});
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
			ServerInfo.Log.DebugFormat("LOADING: Sending signal to clients @={0}", new object[]
			{
				GameHubBehaviour.Hub.GameTime.PlaybackStartTime
			});
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
			this.ServerSendAFKTimeUpdate(this.Sender, this._afkManager.GetAFKTime(this.Sender));
		}

		[RemoteMethod]
		public void ServerPlayerInputPressed()
		{
			this._afkManager.InputChanged(GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender));
		}

		[RemoteMethod]
		public void ServerLeaverWarningCallback(bool timedOut)
		{
			this._afkManager.LeaverWarningCallback(timedOut, this.Sender);
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
				ServerInfo.Log.Warn("Swordfish connection is null.");
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

		public object Invoke(int classId, short methodId, object[] args, BitStream bitstream = null)
		{
			this._delayed = null;
			switch (methodId)
			{
			case 1:
				this.SetInfo((MatchData)args[0]);
				return null;
			default:
				switch (methodId)
				{
				case 26:
					this.ServerPlayerLoadingUpdate((float)args[0]);
					return null;
				case 28:
					this.ServerReloadAFKTime();
					return null;
				case 29:
					this.ServerPlayerInputPressed();
					return null;
				case 30:
					this.ServerLeaverWarningCallback((bool)args[0]);
					return null;
				}
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			case 3:
				this.SetPlayerCompetitiveState((string)args[0]);
				return null;
			case 4:
				this.SetPlayerRewards((string)args[0]);
				return null;
			case 6:
				this.PlaybackReady((long)args[0], (int)args[1], (int)args[2], (float)args[3]);
				return null;
			case 7:
				this.FullDataSent();
				return null;
			case 8:
				this.ServerSet();
				return null;
			case 11:
				this.ServerPlayerLoadingInfo((long)args[0], (float)args[1]);
				return null;
			case 13:
				this.ServerPlayerDisconnectInfo();
				return null;
			case 17:
				this.ClientPlayerAFKTimeUpdate((float)args[0]);
				return null;
			case 20:
				this.ServerEventRequest();
				return null;
			case 22:
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

		[Inject]
		private IPlayback _playback;

		[Inject]
		private IGadgetEventDispatcher _gadgetEventDispatcher;

		[Inject]
		private IGadgetLevelDispatcher _gadgetLevelDispatcher;

		[Inject]
		private IBombInstanceDispatcher _bombInstanceDispatcher;

		[Inject]
		private ICombatFeedbackDispatcher _combatFeedbackDispatcher;

		[Inject]
		private ICombatStatesDispatcher _combatStatesDispatcher;

		[Inject]
		private IStatsDispatcher _statsDispatcher;

		[Inject]
		private IScoreboardDispatcher _scoreboardDispatcher;

		[Inject]
		private IEventManagerDispatcher _eventManagerDispatcher;

		[Inject]
		private IMatchTeamsDispatcher _teamsDispatcher;

		[Inject]
		private IMatchPlayersDispatcher _playersDispatcher;

		[InjectOnClient]
		private ICompetitiveP2pService _competitiveP2PService;

		[InjectOnClient]
		private DiContainer _diContainer;

		[InjectOnServer]
		private IAFKManager _afkManager;

		private Action _whenLoadedCallback;

		private Action _whenExecutedCallback;

		private bool _playersReadySentEvent;

		private bool _playersReadyCoroutineTimeout;

		private int _playersReadyTotalTimeout;

		public const int StaticClassId = 1017;

		private Identifiable _identifiable;

		[ThreadStatic]
		private ServerInfoAsync _async;

		[ThreadStatic]
		private ServerInfoDispatch _dispatch;

		private IFuture _delayed;

		public delegate void MatchStateChanged(MatchData.MatchState newMatchstate);
	}
}
