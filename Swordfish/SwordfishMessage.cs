using System;
using System.Runtime.CompilerServices;
using ClientAPI;
using ClientAPI.Chat;
using ClientAPI.Chat.Api;
using ClientAPI.MessageHub;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishMessage : GameHubObject
	{
		public SwordfishMessage()
		{
			this.Ready = false;
			this._msgHub = GameHubObject.Hub.ClientApi.hubClient;
			this._matchmaking = new SwordfishMatchmaking(GameHubObject.Hub.ClientApi);
			if (GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				return;
			}
			this._msgHub.BalanceChanged += GameHubObject.Hub.Store.ReloadBalance;
			this._msgHub.Connected += this.OnConnect;
			this._msgHub.Disconnected += this.OnDisconnect;
			AbstractHubClient msgHub = this._msgHub;
			if (SwordfishMessage.<>f__mg$cache0 == null)
			{
				SwordfishMessage.<>f__mg$cache0 = new EventHandlerEx<string>(SwordfishMessage.OnError);
			}
			msgHub.Error += SwordfishMessage.<>f__mg$cache0;
			IChatClient chatClient = GameHubObject.Hub.ClientApi.chatClient;
			if (SwordfishMessage.<>f__mg$cache1 == null)
			{
				SwordfishMessage.<>f__mg$cache1 = new EventHandlerEx<ChatMessage>(SwordfishMessage.OnChatMessageReceived);
			}
			chatClient.ChatMessageReceived += SwordfishMessage.<>f__mg$cache1;
			AbstractHubClient msgHub2 = this._msgHub;
			if (SwordfishMessage.<>f__mg$cache2 == null)
			{
				SwordfishMessage.<>f__mg$cache2 = new EventHandlerEx<PresenceMessage>(SwordfishMessage.OnPresenceReceived);
			}
			msgHub2.PresenceReceived += SwordfishMessage.<>f__mg$cache2;
			AbstractHubClient msgHub3 = this._msgHub;
			if (SwordfishMessage.<>f__mg$cache3 == null)
			{
				SwordfishMessage.<>f__mg$cache3 = new EventHandlerEx<Exception>(SwordfishMessage.OnConnectionError);
			}
			msgHub3.ConnectionError += SwordfishMessage.<>f__mg$cache3;
			AbstractHubClient msgHub4 = this._msgHub;
			if (SwordfishMessage.<>f__mg$cache4 == null)
			{
				SwordfishMessage.<>f__mg$cache4 = new EventHandlerEx<SerializationErrorWrapper>(SwordfishMessage.OnSerializationError);
			}
			msgHub4.SerializationError += SwordfishMessage.<>f__mg$cache4;
			AbstractHubClient msgHub5 = this._msgHub;
			if (SwordfishMessage.<>f__mg$cache5 == null)
			{
				SwordfishMessage.<>f__mg$cache5 = new EventHandlerEx<SerializationErrorWrapper>(SwordfishMessage.OnDeserializationError);
			}
			msgHub5.DeserializationError += SwordfishMessage.<>f__mg$cache5;
			this.Ready = true;
		}

		public AbstractHubClient MsgHub
		{
			get
			{
				return this._msgHub;
			}
		}

		public SwordfishMatchmaking Matchmaking
		{
			get
			{
				return this._matchmaking;
			}
		}

		public bool Ready
		{
			get
			{
				return this._ready;
			}
			set
			{
				this._ready = value;
			}
		}

		~SwordfishMessage()
		{
			this.Cleanup();
		}

		public void Cleanup()
		{
			if (this._msgHub != null)
			{
				this._msgHub.Stop();
			}
			if (this._matchmaking != null)
			{
				this._matchmaking.Dispose();
			}
			this._msgHub = null;
			this._matchmaking = null;
			this.Ready = false;
		}

		private void OnConnect(object sender, EventArgs e)
		{
			this.ConnectionId = this._msgHub.Id.ToString();
		}

		private static void OnConnectionError(object sender, Exception exception)
		{
			SwordfishMessage.Log.Fatal("MsgHubConnectionError, exception:", exception);
			string msg = string.Format("Exception={0}", exception.Message.Replace(' ', '.').Replace('=', '-'));
			GameHubObject.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.MsgHubConnectionError, msg, true);
			for (Exception innerException = exception.InnerException; innerException != null; innerException = innerException.InnerException)
			{
				SwordfishMessage.Log.Fatal("Inner exception:", innerException);
			}
		}

		private void OnDisconnect(object sender, DisconnectionReasonWrapper e)
		{
			string msg = string.Format("Reason={0} Exception={1}", e.GetReason(), e.GetException().Message.Replace(' ', '.').Replace('=', '-'));
			GameHubObject.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.MsgHubDisconnected, msg, true);
		}

		private static void OnError(object sender, string eventargs)
		{
			SwordfishMessage.Log.ErrorFormat("Message hub error={0} sender={1}", new object[]
			{
				eventargs,
				sender
			});
		}

		private static void OnRawMessageReceived(object sender, Message eventargs)
		{
			if (!eventargs.ToXmlString().Contains("heartbeat"))
			{
			}
		}

		private static void OnPresenceReceived(object sender, PresenceMessage eventArgs)
		{
		}

		private static void OnChatMessageReceived(object sender, ChatMessage eventargs)
		{
		}

		private static void OnSerializationError(object sender, SerializationErrorWrapper eventArgs)
		{
			SwordfishMessage.Log.ErrorFormat("OnSerializationError = msg={0}, exception={1}", new object[]
			{
				eventArgs.RawMessage,
				eventArgs.Exception.Message
			});
		}

		private static void OnDeserializationError(object sender, SerializationErrorWrapper eventArgs)
		{
			SwordfishMessage.Log.ErrorFormat("OnDeserializationError = msg={0}, exception={1}", new object[]
			{
				eventArgs.RawMessage,
				eventArgs.Exception.Message
			});
		}

		public void Connect()
		{
			this.InternalConnect();
		}

		private void InternalConnect()
		{
			SwordfishMessage.Log.InfoFormat("Connecting Region={0} Token={1}", new object[]
			{
				SingletonMonoBehaviour<RegionController>.Instance.CurrentRegionServerPing,
				GameHubObject.Hub.ClientApi.Token
			});
			try
			{
				this._msgHub.Connect();
			}
			catch (Exception ex)
			{
				Debug.LogError("swordfihmessage - InternalConnect: error: " + ex.Message);
			}
		}

		public void ConnectToMatch(GameState fallbackStateOnError, System.Action onErrorAction = null)
		{
			GameHubObject.Hub.Server.ServerIp = this._matchmaking.ServerHost;
			GameHubObject.Hub.Server.ServerPort = this._matchmaking.ServerPort;
			this.ClientMatchId = this._matchmaking.MatchId;
			GameHubObject.Hub.User.ConnectToServer(false, delegate
			{
				this.ConnectFailed(fallbackStateOnError, onErrorAction);
			}, null);
		}

		public void ConnectNarratorToMatch(GameState fallbackStateOnError)
		{
			GameHubObject.Hub.Server.ServerIp = this._matchmaking.ServerHost;
			GameHubObject.Hub.Server.ServerPort = this._matchmaking.ServerPort;
			this.ClientMatchId = this._matchmaking.MatchId;
			GameHubObject.Hub.User.ConnectNarratorToServer(false, delegate
			{
				GameHubObject.Hub.State.GotoState(fallbackStateOnError, false);
			}, null);
		}

		private void ConnectFailed(GameState fallbackStateOnError, System.Action onErrorAction)
		{
			GameHubObject.Hub.State.GotoState(fallbackStateOnError, false);
			if (onErrorAction != null)
			{
				onErrorAction();
			}
		}

		public Guid ClientMatchId { get; set; }

		public static readonly BitLogger Log = new BitLogger(typeof(SwordfishMessage));

		private AbstractHubClient _msgHub;

		private SwordfishMatchmaking _matchmaking;

		private bool _ready;

		public string ConnectionId;

		public string To;

		public string Msg;

		public string GroupId;

		[CompilerGenerated]
		private static EventHandlerEx<string> <>f__mg$cache0;

		[CompilerGenerated]
		private static EventHandlerEx<ChatMessage> <>f__mg$cache1;

		[CompilerGenerated]
		private static EventHandlerEx<PresenceMessage> <>f__mg$cache2;

		[CompilerGenerated]
		private static EventHandlerEx<Exception> <>f__mg$cache3;

		[CompilerGenerated]
		private static EventHandlerEx<SerializationErrorWrapper> <>f__mg$cache4;

		[CompilerGenerated]
		private static EventHandlerEx<SerializationErrorWrapper> <>f__mg$cache5;
	}
}
