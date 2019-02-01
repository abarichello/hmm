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
			this._client.Connection += new EventHandler<MatchConnectionArgs>(this.OnClientConnected);
			this._client.Disconnection += new EventHandler<MatchmakingEventArgs>(this.OnClientDisconnected);
			this._client.MatchMade += new EventHandler<MatchmakingEventArgs>(this.OnMatchmakingMade);
			this._client.MatchStarted += this.OnMatchmakingStarted;
			this._client.MatchConfirmed += new EventHandler<MatchmakingEventArgs>(this.OnMatchConfirmed);
			this._client.MatchCanceled += this.OnMatchmakingCanceled;
			this._client.MatchAccepted += this.OnMatchmakingAccepted;
			this._client.QueueAverageTime += this.OnQueueAverageTime;
			this._client.TimeToPlayPredicted += this.OnTimeToPlayPredicted;
			this._client.QueueSize += this.OnGetQueueSize;
			this._client.MatchError += new EventHandlerEx<MatchmakingEventArgs>(this.OnMatchError);
			this._client.NoServerAvailable += new EventHandlerEx<MatchmakingEventArgs>(this.OnNoServerAvailable);
			this._lobby = clientApi.lobby;
			this._lobby.LobbyFinished += this.OnLobbyFinished;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event System.Action OnClientDisconnectedEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event System.Action OnClientConnectedEvent;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event System.Action OnMatchStartedEvent;

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
			this._client.Connection -= new EventHandler<MatchConnectionArgs>(this.OnClientConnected);
			this._client.Disconnection -= new EventHandler<MatchmakingEventArgs>(this.OnClientDisconnected);
			this._client.MatchMade -= new EventHandler<MatchmakingEventArgs>(this.OnMatchmakingMade);
			this._client.MatchStarted -= this.OnMatchmakingStarted;
			this._client.MatchConfirmed -= new EventHandler<MatchmakingEventArgs>(this.OnMatchConfirmed);
			this._client.MatchCanceled -= this.OnMatchmakingCanceled;
			this._client.MatchAccepted -= this.OnMatchmakingAccepted;
			this._client.QueueAverageTime -= this.OnQueueAverageTime;
			this._client.TimeToPlayPredicted -= this.OnTimeToPlayPredicted;
			this._client.QueueSize -= this.OnGetQueueSize;
			this._client.MatchError -= new EventHandlerEx<MatchmakingEventArgs>(this.OnMatchError);
			this._client.NoServerAvailable -= new EventHandlerEx<MatchmakingEventArgs>(this.OnNoServerAvailable);
			this._client = null;
			this._lobby.LobbyFinished -= this.OnLobbyFinished;
			this._lobby = null;
		}

		private void OnLobbyFinished(object sender, MatchmakingLobbyFinishedEventArgs eventArgs)
		{
			SwordfishMatchmaking.Log.InfoFormat("Matchmmaking disconnected={0} sender={1}", new object[]
			{
				eventArgs,
				sender
			});
			this.Connected = false;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			if (eventArgs.ErrorType == LobbyMatchmakingMessage.LobbyMessageErrorType.InMatch)
			{
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
			SwordfishMatchmaking.Log.InfoFormat("Matchmmaking requesting queue avg time. QueueName={0}", new object[]
			{
				queueName
			});
			this._client.RequestQueueAverageTime(queueName);
		}

		private void OnQueueAverageTime(object sender, MatchmakingQueueAverageTime e)
		{
			SwordfishMatchmaking.Log.InfoFormat("Matchmmaking OnQueueAverageTime. QueueName={0} AverageTime={1}", new object[]
			{
				e.QueueName,
				e.AverageTime
			});
			if (this.OnQueueAvgTime != null)
			{
				this.OnQueueAvgTime(e.QueueName, e.AverageTime);
			}
		}

		private void OnMatchmakingAccepted(object sender, MatchAcceptedArgs e)
		{
			SwordfishMatchmaking.Log.InfoFormat("Matchmmaking acceppted={0} sender={1} clients={2}", new object[]
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
			SwordfishMatchmaking.Log.ErrorFormat("Matchmmaking OnMatchError arg={0} sender={1}", new object[]
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
			SwordfishMatchmaking.Log.ErrorFormat("Matchmmaking OnNoServerAvailable arg={0} sender={1}", new object[]
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

		private void OnClientDisconnected(object sender, EventArgs e)
		{
			SwordfishMatchmaking.Log.InfoFormat("Matchmmaking disconnected={0} sender={1}", new object[]
			{
				e,
				sender
			});
			this.Connected = false;
			this.Undefined = false;
			if (this.OnClientDisconnectedEvent != null)
			{
				this.OnClientDisconnectedEvent();
			}
			GameHubObject.Hub.ClientApi.hubClient.ConnectionInstability -= this.HubClientOnConnectionInstability;
			this._lastPlayRequestErrorAction = null;
		}

		private void OnClientConnected(object sender, EventArgs e)
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
			GameHubObject.Hub.Swordfish.Log.BILogClient(ClientBITags.MatchmakingStart, true);
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
			UnityEngine.Debug.Log(string.Concat(new object[]
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

		private void OnMatchmakingStarted(object sender, MatchStartedEventArgs e)
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
			GameHubObject.Hub.Swordfish.Log.BILogClient(ClientBITags.MatchmakingSuccess, true);
			GameHubObject.Hub.Config.SetSetting("CURRENT_REGION", GameHubObject.Hub.ClientApi.GetCurrentRegionName());
			GameHubObject.Hub.Config.SaveSettings();
			this._isPlayNowRequested = false;
			if (this.OnMatchStartedEvent != null)
			{
				this.OnMatchStartedEvent();
			}
			this._lastPlayRequestErrorAction = null;
		}

		private void OnMatchmakingMade(object sender, EventArgs e)
		{
			MatchmakingEventArgs matchmakingEventArgs = e as MatchmakingEventArgs;
			this.MatchMadeQueue = string.Empty;
			this._numPlayersInMatchMade = 0;
			this._totalPlayersAndBotsInMatchMade = 0;
			this._matchAcceptTimeout = 0;
			if (matchmakingEventArgs != null)
			{
				this.MatchMadeQueue = matchmakingEventArgs.QueueName;
				this._numPlayersInMatchMade = matchmakingEventArgs.NumberOfPlayersLastMatch;
				this._totalPlayersAndBotsInMatchMade = matchmakingEventArgs.MatchSize;
				this._matchAcceptTimeout = matchmakingEventArgs.MatchAcceptTimeout;
			}
			SwordfishMatchmaking.Log.InfoFormat("Match made={0} sender={1} queue={2} numPlayers={3} totalMatch={4} acceptTimout={5}", new object[]
			{
				e,
				sender,
				this.MatchMadeQueue,
				this._numPlayersInMatchMade,
				this._totalPlayersAndBotsInMatchMade,
				this._matchAcceptTimeout
			});
			this.State = SwordfishMatchmaking.MatchmakingState.Made;
			this.Undefined = false;
			this.WaitingForMatchResult = true;
			GameHubObject.Hub.Swordfish.Log.BILogClient(ClientBITags.MatchmakingMade, true);
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
			GameHubObject.Hub.Swordfish.Log.BILogClient(ClientBITags.MatchmakingFound, true);
		}

		private void OnMatchmakingCanceled(object sender, MatchCancelledArgs e)
		{
			if (this.State == SwordfishMatchmaking.MatchmakingState.None)
			{
				SwordfishMatchmaking.Log.InfoFormat("Match canceled={0} clients={1} sender={2}", new object[]
				{
					e,
					Arrays.ToStringWithComma(e.Clients),
					sender
				});
				GameHubObject.Hub.Swordfish.Log.BILogClient(ClientBITags.MatchmakingCanceled, true);
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
					GameHubObject.Hub.Swordfish.Log.BILogClient(ClientBITags.MatchmakingDeclined, true);
					SwordfishMatchmaking.Log.InfoFormat("Match declined={0} clients={1} sender={2}", new object[]
					{
						e,
						Arrays.ToStringWithComma(e.Clients),
						sender
					});
				}
				else
				{
					GameHubObject.Hub.Swordfish.Log.BILogClient(ClientBITags.MatchmakingFail, true);
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

		public void StartMatch(string[] clientIds, string config, System.Action onError)
		{
			SingletonMonoBehaviour<RegionController>.Instance.UpdateCurrentRegionOnSFServer(true);
			SwordfishMatchmaking.Log.InfoFormat("Start Custom match group={0} - config={1}", new object[]
			{
				Arrays.ToStringWithComma(clientIds),
				config
			});
			this.LastFailed = false;
			this._lastPlayRequestErrorAction = onError;
			GameHubObject.Hub.ClientApi.hubClient.ConnectionInstability += this.HubClientOnConnectionInstability;
			this._isPlayNowRequested = true;
			this._client.PlayNow(onError, clientIds, config, new SwordfishClientApi.NetworkErrorCallback(this.OnPlayError));
		}

		public void StartMatch(string queueName, System.Action onError)
		{
			SingletonMonoBehaviour<RegionController>.Instance.UpdateCurrentRegionOnSFServer(true);
			this.LastFailed = false;
			this.Undefined = true;
			this._lastPlayRequestErrorAction = onError;
			if (this._client.IsInGroup())
			{
				SwordfishMatchmaking.Log.Error("MATCHMAKING CLIENT IS IN GROUP, BUT IT SHOULDN'T BE!!! --> INCONSISTENT STATE!");
			}
			GameHubObject.Hub.ClientApi.hubClient.ConnectionInstability += this.HubClientOnConnectionInstability;
			this._client.PlaySolo(onError, queueName, new SwordfishClientApi.NetworkErrorCallback(this.OnPlayError));
			SwordfishMatchmaking.Log.InfoFormat("Start solo -> Queue: {0}", new object[]
			{
				queueName
			});
		}

		private void HubClientOnConnectionInstability(object sender, ConnectionInstabilityMessage eventArgs)
		{
			this.OnPlayError(this._lastPlayRequestErrorAction, new ConnectionException("Connection instability detected."));
			this._lastPlayRequestErrorAction = null;
			GameHubObject.Hub.ClientApi.hubClient.ConnectionInstability -= this.HubClientOnConnectionInstability;
		}

		public void StartGroupMatch(Guid groupId, string[] users, string queueName, System.Action onError)
		{
			SingletonMonoBehaviour<RegionController>.Instance.UpdateCurrentRegionOnSFServer(true);
			this.LastFailed = false;
			this.Undefined = true;
			this._lastPlayRequestErrorAction = onError;
			this._client.PlayGroup(onError, groupId, users, queueName, new SwordfishClientApi.NetworkErrorCallback(this.OnPlayError));
			SwordfishMatchmaking.Log.InfoFormat("Start group={0}:{1}:{2} -> Queue '{3}'", new object[]
			{
				groupId,
				users.Length,
				Arrays.ToStringWithComma(users),
				queueName
			});
		}

		private void OnPlayError(object state, ConnectionException e)
		{
			SwordfishMatchmaking.Log.Fatal("Connection with matchmaking service failed", e);
			this.LastFailed = true;
			this.Connected = false;
			this.Undefined = false;
			this.WaitingForMatchResult = false;
			this.State = SwordfishMatchmaking.MatchmakingState.None;
			System.Action action = state as System.Action;
			if (action != null)
			{
				action();
			}
			if (this._isPlayNowRequested)
			{
				this._isPlayNowRequested = false;
				this.TryCallPlayRequestErrorActionAndClearIt();
			}
		}

		public void Accept()
		{
			this._client.Accept(this.MatchMadeQueue);
			SwordfishMatchmaking.Log.InfoFormat("Accepted queue:{0}", new object[]
			{
				this.MatchMadeQueue
			});
		}

		public void Fail()
		{
			if (this._client.IsWaitingInQueue())
			{
				this._client.CancelSearch(this.MatchMadeQueue);
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
			return Mathf.Max(0, this._totalPlayersAndBotsInMatchMade - this._numPlayersInMatchMade);
		}

		public int GetMatchAcceptTimeout()
		{
			return this._matchAcceptTimeout;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(SwordfishMatchmaking));

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

		private bool _isPlayNowRequested;

		private System.Action _lastPlayRequestErrorAction;

		private int _numPlayersInMatchMade;

		private int _totalPlayersAndBotsInMatchMade;

		private int _matchAcceptTimeout;

		private Action<MatchmakingQueueSize> _getQueueCallback;

		public enum MatchmakingState
		{
			None,
			Made,
			Confirmed,
			Started,
			Reconnecting
		}
	}
}
