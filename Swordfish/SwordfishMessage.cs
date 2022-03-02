using System;
using System.Runtime.CompilerServices;
using ClientAPI;
using ClientAPI.Chat;
using ClientAPI.Chat.Api;
using ClientAPI.MessageHub;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Swordfish
{
	public class SwordfishMessage : GameHubObject
	{
		public SwordfishMessage(IIsFeatureToggled isFeatureToggled)
		{
			this._isFeatureToggled = isFeatureToggled;
			this.Ready = false;
			this._msgHub = GameHubObject.Hub.ClientApi.hubClient;
			this._matchmaking = new SwordfishMatchmaking(GameHubObject.Hub.ClientApi);
			if (GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				return;
			}
			this._msgHub.BalanceChanged += new EventHandlerEx<BalanceMessage>(GameHubObject.Hub.Store.ReloadBalance);
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
				SwordfishMessage.<>f__mg$cache2 = new EventHandlerEx<SerializationErrorWrapper>(SwordfishMessage.OnSerializationError);
			}
			msgHub2.SerializationError += SwordfishMessage.<>f__mg$cache2;
			AbstractHubClient msgHub3 = this._msgHub;
			if (SwordfishMessage.<>f__mg$cache3 == null)
			{
				SwordfishMessage.<>f__mg$cache3 = new EventHandlerEx<SerializationErrorWrapper>(SwordfishMessage.OnDeserializationError);
			}
			msgHub3.DeserializationError += SwordfishMessage.<>f__mg$cache3;
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
			SwordfishMessage.Log.DebugFormat("Connected={0} sender={1}", new object[]
			{
				e,
				sender
			});
			this.ConnectionId = this._msgHub.Id.ToString();
		}

		private void OnDisconnect(object sender, DisconnectionReasonWrapper e)
		{
			SwordfishMessage.Log.DebugFormat("MsgHubDisconnected Reason={0} sender={1} Exception={2}", new object[]
			{
				e.GetReason(),
				sender,
				e.GetException()
			});
			string msg = string.Format("Reason={0} Exception={1}", e.GetReason(), e.GetException().Message.Replace(' ', '.').Replace('=', '-'));
			GameHubObject.Hub.Swordfish.Log.BILogClientMsg(43, msg, true);
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
			if (!eventargs.ToSerializedString().Contains("heartbeat"))
			{
				SwordfishMessage.Log.DebugFormat("Raw Message Received={0} S={1} sender={2}", new object[]
				{
					eventargs.ToSerializedString(),
					eventargs.ToString(),
					sender
				});
			}
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

		public void ConnectToMatch()
		{
			GameHubObject.Hub.Server.ServerIp = this._matchmaking.ServerHost;
			GameHubObject.Hub.Server.ServerPort = this._matchmaking.ServerPort;
			this.ClientMatchId = this._matchmaking.MatchId;
			GameHubObject.Hub.User.ConnectToServer(false, new Action(this.ConnectFailed), null);
		}

		public void ConnectNarratorToMatch()
		{
			GameHubObject.Hub.Server.ServerIp = this._matchmaking.ServerHost;
			GameHubObject.Hub.Server.ServerPort = this._matchmaking.ServerPort;
			this.ClientMatchId = this._matchmaking.MatchId;
			GameHubObject.Hub.User.ConnectNarratorToServer(false, new Action(this.ConnectFailed), null);
		}

		private void ConnectFailed()
		{
			GameHubObject.Hub.EndSession("ConnectToServer failed");
		}

		public Guid ClientMatchId { get; set; }

		private readonly IIsFeatureToggled _isFeatureToggled;

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
		private static EventHandlerEx<SerializationErrorWrapper> <>f__mg$cache2;

		[CompilerGenerated]
		private static EventHandlerEx<SerializationErrorWrapper> <>f__mg$cache3;
	}
}
