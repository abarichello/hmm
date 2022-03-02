using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClientAPI;
using ClientAPI.Exceptions;
using ClientAPI.Matchmaking;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.MessageHub;
using ClientAPI.Service.API.Interfaces.Custom;
using Pocketverse;
using Pocketverse.Util;
using UnityEngine;

namespace HeavyMetalMachines.Swordfish
{
	[Serializable]
	public class SwordfishMatchmaking : GameHubObject
	{
		public SwordfishMatchmaking(SwordfishClientApi clientApi)
		{
			this._client = clientApi.matchmakingClient;
			this._lobby = clientApi.lobby;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnClientDisconnectedEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnClientConnectedEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnMatchStartedEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnClientMatchMadeEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string[]> OnMatchAcceptedEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string[]> OnMatchCanceledEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string, double> OnQueueAvgTime;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<double> OnTimePredict;

		public Guid MatchId { get; private set; }

		public string MatchMadeQueue { get; private set; }

		public void Dispose()
		{
			this.Connected = false;
			this.WaitingForMatchResult = false;
			this.Undefined = false;
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			this._client.Connection -= new EventHandler<MatchConnectionArgs>(this.OnPlayerJoinedQueue);
			this._client.Disconnection -= new EventHandler<MatchmakingEventArgs>(this.OnPlayerLeftQueue);
			this._client.MatchMade -= this.OnMatchMade;
			this._client.MatchStarted -= this.OnMatchStarted;
			this._client.MatchConfirmed -= new EventHandler<MatchmakingEventArgs>(this.OnMatchConfirmed);
			this._client.MatchCanceled -= this.OnPlayerCanceledQueueOrMatchWasRefused;
			this._client.MatchAccepted -= this.OnMatchAccepted;
			this._client.QueueAverageTime -= this.OnQueueAverageTime;
			this._client.TimeToPlayPredicted -= this.OnTimeToPlayPredicted;
			this._client.QueueSize -= this.OnGetQueueSize;
			this._client.MatchError -= new EventHandlerEx<MatchmakingErrorEventArgs>(this.OnMatchError);
			this._client.NoServerAvailable -= new EventHandlerEx<MatchmakingEventArgs>(this.OnNoServerAvailable);
			this._lobby.LobbyFinished -= new EventHandlerEx<MatchmakingLobbyFinishedEventArgs>(this.OnCustomMatchLobbyFinished);
			GameHubObject.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
		}

		public void Initialize()
		{
			this._client.Connection += new EventHandler<MatchConnectionArgs>(this.OnPlayerJoinedQueue);
			this._client.Disconnection += new EventHandler<MatchmakingEventArgs>(this.OnPlayerLeftQueue);
			this._client.MatchMade += this.OnMatchMade;
			this._client.MatchStarted += this.OnMatchStarted;
			this._client.MatchConfirmed += new EventHandler<MatchmakingEventArgs>(this.OnMatchConfirmed);
			this._client.MatchCanceled += this.OnPlayerCanceledQueueOrMatchWasRefused;
			this._client.MatchAccepted += this.OnMatchAccepted;
			this._client.QueueAverageTime += this.OnQueueAverageTime;
			this._client.TimeToPlayPredicted += this.OnTimeToPlayPredicted;
			this._client.QueueSize += this.OnGetQueueSize;
			this._client.MatchError += new EventHandlerEx<MatchmakingErrorEventArgs>(this.OnMatchError);
			this._client.NoServerAvailable += new EventHandlerEx<MatchmakingEventArgs>(this.OnNoServerAvailable);
			this._lobby.LobbyFinished += new EventHandlerEx<MatchmakingLobbyFinishedEventArgs>(this.OnCustomMatchLobbyFinished);
			GameHubObject.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
		}

		private void ListenToStateChanged(GameState changedstate)
		{
			if (changedstate.StateKind != GameState.GameStateKind.MainMenu)
			{
				this.State = SwordfishMatchmaking.MatchmakingState.None;
			}
		}

		private void OnCustomMatchLobbyFinished(object sender, MatchmakingLobbyFinishedEventArgs eventArgs)
		{
			this.Connected = false;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			if (eventArgs.ErrorType == 5)
			{
				SwordfishMatchmaking.Log.Debug("OnLobbyFinished: Match started. Going to pick mode.");
				return;
			}
			SwordfishMatchmaking.Log.WarnFormat("OnLobbyFinished: Custom match won't go to Pick Screen. reason: {0}", new object[]
			{
				eventArgs.ErrorType
			});
			this.LastFailed = false;
		}

		private void OnTimeToPlayPredicted(object sender, MatchmakingTimePredicted e)
		{
			if (this.OnTimePredict != null)
			{
				this.OnTimePredict(e.TimePredicted);
			}
		}

		public void RequestQueueAvgTime(string queueName)
		{
			SwordfishMatchmaking.Log.InfoFormat("Matchmaking requesting queue avg time. QueueName={0}", new object[]
			{
				queueName
			});
			this._client.RequestQueueAverageTime(queueName);
		}

		private void OnQueueAverageTime(object sender, MatchmakingQueueAverageTime e)
		{
			SwordfishMatchmaking.Log.InfoFormat("Matchmaking OnQueueAverageTime. QueueName={0} AverageTime={1}", new object[]
			{
				e.QueueName,
				e.AverageTime
			});
			if (this.OnQueueAvgTime != null)
			{
				this.OnQueueAvgTime(e.QueueName, e.AverageTime);
			}
		}

		private void OnMatchAccepted(object sender, MatchAcceptedArgs e)
		{
			SwordfishMatchmaking.Log.InfoFormat("Matchmaking acceppted={0} sender={1} clients={2}", new object[]
			{
				e,
				sender,
				e.Clients.Length
			});
			if (this.OnMatchAcceptedEvent != null)
			{
				this.OnMatchAcceptedEvent(e.Clients);
			}
		}

		private void OnMatchError(object sender, EventArgs e)
		{
			MatchmakingEventArgs matchmakingEventArgs = (MatchmakingEventArgs)e;
			SwordfishMatchmaking.Log.ErrorFormat("Matchmaking OnMatchError arg={0} sender={1}", new object[]
			{
				matchmakingEventArgs.QueueName,
				sender
			});
			this.LastFailed = true;
			this.Connected = false;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			this.TryCallPlayRequestErrorActionAndClearIt();
		}

		private void OnNoServerAvailable(object sender, EventArgs e)
		{
			MatchmakingEventArgs matchmakingEventArgs = (MatchmakingEventArgs)e;
			SwordfishMatchmaking.Log.ErrorFormat("Matchmaking OnNoServerAvailable arg={0} sender={1}", new object[]
			{
				matchmakingEventArgs.QueueName,
				sender
			});
			this.LastFailed = true;
			this.Connected = false;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			this.TryCallPlayRequestErrorActionAndClearIt();
		}

		private void TryCallPlayRequestErrorActionAndClearIt()
		{
			if (this._lastPlayRequestErrorAction == null)
			{
				return;
			}
			this._lastPlayRequestErrorAction();
			this._lastPlayRequestErrorAction = null;
		}

		private void OnPlayerLeftQueue(object sender, EventArgs e)
		{
			SwordfishMatchmaking.Log.InfoFormat("Matchmaking disconnected={0} sender={1}", new object[]
			{
				e,
				sender
			});
			this.Connected = false;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
			if (this.OnClientDisconnectedEvent != null)
			{
				this.OnClientDisconnectedEvent();
			}
			this.TryUnregisterInstabilityCallback();
			this._lastPlayRequestErrorAction = null;
		}

		private void OnPlayerJoinedQueue(object sender, EventArgs e)
		{
			MatchmakingEventArgs matchmakingEventArgs = e as MatchmakingEventArgs;
			this.MatchMadeQueue = ((matchmakingEventArgs != null) ? matchmakingEventArgs.QueueName : string.Empty);
			SwordfishMatchmaking.Log.InfoFormat("Matchmaking connected queue={0}", new object[]
			{
				this.MatchMadeQueue
			});
			this.Connected = true;
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			this.Undefined = false;
			this.LastFailed = false;
			this.WaitingForMatchResult = false;
			GameHubObject.Hub.Swordfish.Log.BILogClient(8, true);
			this.TryRegisterInstabilityCallback();
			GameHubObject.Hub.Swordfish.Connection.SetJoinedQueue();
			this.SendJoinedMatchmakingQueueEvent();
			if (this.OnClientConnectedEvent != null)
			{
				this.OnClientConnectedEvent();
			}
		}

		private void SendJoinedMatchmakingQueueEvent()
		{
			if (GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.EnableHoplonTTEvent))
			{
				string value = GameHubObject.Hub.Config.GetValue(ConfigAccess.HoplonTTEventUrl);
				HoplonTrackingTool.JoinedMatchmakingQueue(value, GameHubObject.Hub.User.UserSF.UniversalID);
			}
		}

		private void OnGetQueueSize(object sender, MatchmakingQueueSize e)
		{
			Debug.Log(string.Concat(new object[]
			{
				"MATCHMAKING OnGetQueueSize - ",
				e.QueueName,
				" - ",
				e.Size
			}));
			if (this._getQueueCallback != null)
			{
				this._getQueueCallback(e);
			}
			this._getQueueCallback = null;
		}

		private void OnMatchStarted(object sender, MatchStartedEventArgs e)
		{
			SwordfishMatchmaking.Log.InfoFormat("Match started={0} sender={1} Host={2}:{3} MatchId={4}", new object[]
			{
				e,
				sender,
				e.Host,
				e.Port,
				e.MatchId
			});
			this.ServerHost = e.Host;
			this.ServerPort = e.Port;
			this.MatchId = e.MatchId;
			this.State = SwordfishMatchmaking.MatchmakingState.Started;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
			GameHubObject.Hub.Swordfish.Log.BILogClient(12, true);
			GameHubObject.Hub.PlayerPrefs.SetString("CURRENT_REGION", GameHubObject.Hub.ClientApi.GetCurrentRegionName());
			GameHubObject.Hub.PlayerPrefs.Save();
			if (this.OnMatchStartedEvent != null)
			{
				this.OnMatchStartedEvent();
			}
			this._lastPlayRequestErrorAction = null;
		}

		private void OnMatchMade(object sender, MatchMadeEventArgs evt)
		{
			if (!this.Connected)
			{
				SwordfishMatchmaking.Log.Warn("Received OnMatchMade event without being connected to the queue.");
				return;
			}
			this.MatchMadeQueue = string.Empty;
			this._numPlayersInMatchMade = 0;
			this._totalPlayersAndBotsInMatchMade = 0;
			this._matchAcceptTimeout = 0;
			if (evt != null)
			{
				this.MatchMadeQueue = evt.QueueName;
				this._numPlayersInMatchMade = evt.NumberOfPlayersLastMatch;
				this._totalPlayersAndBotsInMatchMade = evt.MatchSize;
				this._matchAcceptTimeout = evt.MatchAcceptTimeout;
				this.MatchId = evt.MatchId;
			}
			SwordfishMatchmaking.Log.InfoFormat("Match made={0} sender={1} queue={2} numPlayers={3} totalMatch={4} acceptTimout={5}, MatchId={6}", new object[]
			{
				evt,
				sender,
				this.MatchMadeQueue,
				this._numPlayersInMatchMade,
				this._totalPlayersAndBotsInMatchMade,
				this._matchAcceptTimeout,
				evt.MatchId
			});
			this.State = SwordfishMatchmaking.MatchmakingState.Made;
			this.Undefined = false;
			this.WaitingForMatchResult = true;
			GameHubObject.Hub.Swordfish.Log.BILogClient(11, true);
			if (this.OnClientMatchMadeEvent != null)
			{
				this.OnClientMatchMadeEvent();
			}
		}

		private void OnMatchConfirmed(object sender, EventArgs e)
		{
			SwordfishMatchmaking.Log.InfoFormat("Match confirmed={0} sender={1}", new object[]
			{
				e,
				sender
			});
			this.State = SwordfishMatchmaking.MatchmakingState.Made;
			this.Undefined = false;
			this.WaitingForMatchResult = true;
			GameHubObject.Hub.Swordfish.Log.BILogClient(10, true);
		}

		private void OnPlayerCanceledQueueOrMatchWasRefused(object sender, MatchCancelledArgs e)
		{
			if (this.State == SwordfishMatchmaking.MatchmakingState.None)
			{
				SwordfishMatchmaking.Log.InfoFormat("Match canceled={0} clients={1} sender={2}", new object[]
				{
					e,
					Arrays.ToStringWithComma(e.Clients),
					sender
				});
				GameHubObject.Hub.Swordfish.Log.BILogClient(9, true);
			}
			else if (this.State == SwordfishMatchmaking.MatchmakingState.Made)
			{
				bool flag = false;
				int num = -1;
				while (!flag && ++num < e.Clients.Length)
				{
					flag = (e.Clients[num] == GameHubObject.Hub.Swordfish.Msg.ConnectionId || this.PartyMembers.Contains(e.Clients[num]));
				}
				if (flag)
				{
					GameHubObject.Hub.Swordfish.Log.BILogClient(14, true);
					SwordfishMatchmaking.Log.InfoFormat("Match declined={0} clients={1} sender={2}", new object[]
					{
						e,
						Arrays.ToStringWithComma(e.Clients),
						sender
					});
				}
				else
				{
					GameHubObject.Hub.Swordfish.Log.BILogClient(13, true);
					SwordfishMatchmaking.Log.InfoFormat("Match fail={0} clients={1} sender={2}", new object[]
					{
						e,
						Arrays.ToStringWithComma(e.Clients),
						sender
					});
				}
			}
			else
			{
				SwordfishMatchmaking.Log.InfoFormat("Match canceled other={0} clients={1} sender={2}", new object[]
				{
					e,
					Arrays.ToStringWithComma(e.Clients),
					sender
				});
			}
			if (this.OnMatchCanceledEvent != null)
			{
				this.OnMatchCanceledEvent(e.Clients);
			}
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
		}

		public void StartMatch(string[] clientIds, string config, Action onError)
		{
			this._startMatchRequestData.ClientIds = clientIds;
			this._startMatchRequestData.Config = config;
			this._startMatchRequestData.OnError = onError;
			if (SingletonMonoBehaviour<RegionController>.Instance.RegionsDataPopulated)
			{
				this.StartMatchAfterRegionDataReceived();
			}
			else
			{
				SingletonMonoBehaviour<RegionController>.Instance.OnRefreshRegionList += this.OnRefreshRegionList;
			}
		}

		private void OnRefreshRegionList(Dictionary<string, RegionServerPing> regionDictionary)
		{
			SingletonMonoBehaviour<RegionController>.Instance.OnRefreshRegionList -= this.OnRefreshRegionList;
			if (regionDictionary.Count > 0)
			{
				this.StartMatchAfterRegionDataReceived();
			}
			else
			{
				this.OnRefreshRegionListError();
			}
			this._startMatchRequestData.Clear();
		}

		private void OnRefreshRegionListError()
		{
			Action onError = this._startMatchRequestData.OnError;
			if (onError != null)
			{
				onError();
			}
		}

		private void StartMatchAfterRegionDataReceived()
		{
			string[] clientIds = this._startMatchRequestData.ClientIds;
			string config = this._startMatchRequestData.Config;
			Action onError = this._startMatchRequestData.OnError;
			SingletonMonoBehaviour<RegionController>.Instance.UpdateCurrentRegionOnSFServer(true);
			this.LastFailed = false;
			this._lastPlayRequestErrorAction = onError;
			this.TryRegisterInstabilityCallback();
			this._client.PlayNow(onError, clientIds, config, new SwordfishClientApi.NetworkErrorCallback(this.OnPlayError));
		}

		public void StartMatch(string queueName, Action onError)
		{
			if (this._client.IsInGroup())
			{
				SwordfishMatchmaking.Log.Error("MATCHMAKING CLIENT IS IN GROUP, BUT IT SHOULDN'T BE!!! --> CALLING LeaveAllGroups!");
				GameHubObject.Hub.ClientApi.group.LeaveAllGroups();
			}
			this.ResetPlayRequestVarsAndTryRegisterInstabilityCallback(onError);
			this._client.PlaySolo(onError, queueName, new SwordfishClientApi.NetworkErrorCallback(this.OnPlayError));
			SwordfishMatchmaking.Log.InfoFormat("Start solo -> Queue: {0}", new object[]
			{
				queueName
			});
		}

		public void StartGroupMatch(Guid groupId, string[] users, string queueName, Action onError)
		{
			this.ResetPlayRequestVarsAndTryRegisterInstabilityCallback(onError);
			this._client.PlayGroup(onError, groupId, users, queueName, new SwordfishClientApi.NetworkErrorCallback(this.OnPlayError));
			SwordfishMatchmaking.Log.InfoFormat("Start group={0}:{1}:{2} -> Queue '{3}'", new object[]
			{
				groupId,
				users.Length,
				Arrays.ToStringWithComma(users),
				queueName
			});
		}

		private void ResetPlayRequestVarsAndTryRegisterInstabilityCallback(Action onError)
		{
			SingletonMonoBehaviour<RegionController>.Instance.UpdateCurrentRegionOnSFServer(true);
			this.LastFailed = false;
			this.Undefined = true;
			this._lastPlayRequestErrorAction = onError;
			this.TryRegisterInstabilityCallback();
		}

		private void TryRegisterInstabilityCallback()
		{
			if (this._instabilityCallbackInstalled)
			{
				return;
			}
			GameHubObject.Hub.ClientApi.hubClient.ConnectionInstability += new EventHandlerEx<ConnectionInstabilityMessage>(this.HubClientOnConnectionInstability);
			this._instabilityCallbackInstalled = true;
		}

		private void TryUnregisterInstabilityCallback()
		{
			if (!this._instabilityCallbackInstalled)
			{
				return;
			}
			GameHubObject.Hub.ClientApi.hubClient.ConnectionInstability -= new EventHandlerEx<ConnectionInstabilityMessage>(this.HubClientOnConnectionInstability);
			this._instabilityCallbackInstalled = false;
		}

		private void HubClientOnConnectionInstability(object sender, ConnectionInstabilityMessage eventArgs)
		{
			this.OnPlayError(this._lastPlayRequestErrorAction, new ConnectionException("Connection instability detected."));
			this._lastPlayRequestErrorAction = null;
			this.TryUnregisterInstabilityCallback();
		}

		private void OnPlayError(object state, ConnectionException e)
		{
			SwordfishMatchmaking.Log.Fatal("Connection with matchmaking service failed", e);
			this.LastFailed = true;
			this.Connected = false;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			Action action = state as Action;
			if (action != null)
			{
				action();
			}
		}

		public void Accept(string queueName)
		{
			this._client.Accept(queueName);
			SwordfishMatchmaking.Log.InfoFormat("Accepted queue:{0}", new object[]
			{
				queueName
			});
		}

		public void Fail()
		{
			if (this._client.IsWaitingInQueue())
			{
				this._client.CancelSearch(this.MatchMadeQueue);
				GameHubObject.Hub.Swordfish.Log.BILogClient(81, true);
			}
			this.LastFailed = true;
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
			SwordfishMatchmaking.Log.InfoFormat("Failed {0}", new object[]
			{
				this.MatchMadeQueue
			});
		}

		public void Decline()
		{
			this._client.Decline(this.MatchMadeQueue);
			SwordfishMatchmaking.Log.InfoFormat("Declining queue {0}", new object[]
			{
				this.MatchMadeQueue
			});
		}

		public void StopSearch()
		{
			this._client.CancelSearch(this.MatchMadeQueue);
			SwordfishMatchmaking.Log.InfoFormat("Stop searching -> Queue {0}", new object[]
			{
				this.MatchMadeQueue
			});
		}

		public int GetNumBotsInMatchmakingMade()
		{
			if (this.State != SwordfishMatchmaking.MatchmakingState.Made)
			{
				SwordfishMatchmaking.Log.ErrorFormat("GetNumBotsInMatchmakingMade in wrong state: {0}", new object[]
				{
					this.State
				});
				return 0;
			}
			if (this.MatchMadeQueue == "Ranked")
			{
				return 0;
			}
			return Mathf.Max(0, this._totalPlayersAndBotsInMatchMade - this._numPlayersInMatchMade);
		}

		public int GetMatchAcceptTimeout()
		{
			return this._matchAcceptTimeout;
		}

		public bool IsInQueue()
		{
			return this._client.IsInQueueState();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SwordfishMatchmaking));

		private SwordfishMatchmaking.StartMatchRequestData _startMatchRequestData;

		private MatchmakingClient _client;

		private ILobby _lobby;

		public bool Group;

		public Guid GroupId;

		public SwordfishMatchmaking.MatchmakingState State;

		public bool Connected;

		public bool Undefined;

		public bool WaitingForMatchResult;

		public bool LastFailed;

		public string ServerHost;

		public int ServerPort;

		public List<string> PartyMembers = new List<string>();

		private Action _lastPlayRequestErrorAction;

		private int _numPlayersInMatchMade;

		private int _totalPlayersAndBotsInMatchMade;

		private int _matchAcceptTimeout;

		private bool _instabilityCallbackInstalled;

		private Action<MatchmakingQueueSize> _getQueueCallback;

		public enum MatchmakingState
		{
			None,
			Made,
			Confirmed,
			Started,
			Reconnecting
		}

		private struct StartMatchRequestData
		{
			public void Clear()
			{
				this.ClientIds = null;
				this.Config = string.Empty;
				this.OnError = null;
			}

			public string[] ClientIds;

			public string Config;

			public Action OnError;
		}
	}
}
