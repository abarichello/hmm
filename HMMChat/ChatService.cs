using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.HMM.GameStates.Game.ComponentContainer;
using Assets.Standard_Assets.Scripts.HMM.GameStates.Game.newChat.Component.Api;
using Assets.Standard_Assets.Scripts.HMM.GameStates.Game.newChat.Component.Impl;
using Assets.Standard_Assets.Scripts.HMM.GameStates.Game.newChat.Impl.Adapter;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.HMMChat
{
	[RemoteClass]
	public class ChatService : GameHubBehaviour, ICleanupListener, IChatService, IBitComponent
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<HudChatController.NewChatMsg> OnClientChatMessageReceived;

		private void Start()
		{
			if (GameHubBehaviour.Hub != null)
			{
				this.ChatMsgList = new List<ChatService.ChatMsg>();
			}
			this._chatComponent = HmmUnityLifeCicleAdapter.Instance.GetHmmComponent<IChatComponent>();
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.ChatComponentEnable))
			{
				((ChatComponent)this._chatComponent).SetChatService(this);
			}
		}

		public void ClientSendMessage(bool isGroup, string text)
		{
			this.DispatchReliable(new byte[0]).ReceiveMessage(isGroup, text);
		}

		public void SendMessage(bool isGroup, string message)
		{
			this.ClientSendMessage(isGroup, message);
		}

		[RemoteMethod]
		private void ReceiveMessage(bool group, string msg)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			ChatService.ChatMsg item = new ChatService.ChatMsg(msg, group, playerByAddress.PlayerAddress, ChatService.ChatMessageKind.PlayerMessage);
			this.ChatMsgList.Add(item);
			if (group)
			{
				TeamKind team = playerByAddress.Team;
				int group2 = (int)team;
				this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(group2)).ClientReceiveMessage(true, msg, this.Sender);
			}
			else
			{
				this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(0)).ClientReceiveMessage(false, msg, this.Sender);
			}
			ChatService.Log.InfoFormat("Player={0} Group={1} Message={2}", new object[]
			{
				playerByAddress.PlayerId,
				(!group) ? "All" : playerByAddress.Team.ToString(),
				msg
			});
		}

		public void ClientSystemMessage(string msg)
		{
			ChatService.ChatMsg chatMsg = new ChatService.ChatMsg(msg, false, 0, ChatService.ChatMessageKind.System);
			this.OnNewClientMessage(chatMsg);
			ChatService.Log.InfoFormat("Client System Message={0}", new object[]
			{
				msg
			});
		}

		public void SystemMessage(string msg)
		{
			ChatService.ChatMsg item = new ChatService.ChatMsg(msg, false, 0, ChatService.ChatMessageKind.PlayerMessage);
			this.ChatMsgList.Add(item);
			this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(0)).ClientReceiveMessage(false, msg, this.Sender);
			ChatService.Log.InfoFormat("System Message={0}", new object[]
			{
				msg
			});
		}

		[RemoteMethod]
		private void ClientReceiveMessage(bool group, string msg, byte playeraddress)
		{
			this.SetupPlayerMessage(group, msg, playeraddress, ChatService.ChatMessageKind.PlayerMessage);
		}

		public void SetupPlayerMessage(bool group, string msg, byte playeraddress, ChatService.ChatMessageKind messageKind)
		{
			if (messageKind != ChatService.ChatMessageKind.PlayerMessage && GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay)
			{
				return;
			}
			if (SpectatorController.IsSpectating && (group || messageKind == ChatService.ChatMessageKind.PlayerNotification))
			{
				return;
			}
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(playeraddress);
			if (playerByAddress != null && ManagerController.Get<ChatManager>().IsUserIgnored(playerByAddress.UserId))
			{
				return;
			}
			byte playerAddress = (!(playerByAddress == null)) ? playerByAddress.PlayerAddress : 0;
			ChatService.ChatMsg chatMsg = new ChatService.ChatMsg(msg, group, playerAddress, messageKind);
			this.ChatMsgList.Add(chatMsg);
			this.OnNewClientMessage(chatMsg);
		}

		public void ClientReceiveLogMessage(string msg)
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay)
			{
				return;
			}
			ChatService.ChatMsg chatMsg = new ChatService.ChatMsg(msg, false, 0, ChatService.ChatMessageKind.LogMessage);
			this.OnNewClientMessage(chatMsg);
		}

		private void OnNewClientMessage(ChatService.ChatMsg chatMsg)
		{
			switch (chatMsg.MessageKind)
			{
			case ChatService.ChatMessageKind.LogMessage:
				this.AddAndNotifyMessageReceived(chatMsg, chatMsg.Text);
				break;
			case ChatService.ChatMessageKind.PlayerMessage:
				this.CreateDefaultChatMessage(chatMsg);
				break;
			case ChatService.ChatMessageKind.PlayerNotification:
				this.AddAndNotifyMessageReceived(chatMsg, this.CreatePlayerNotificationChatMessage(chatMsg));
				break;
			case ChatService.ChatMessageKind.System:
				this.AddAndNotifyMessageReceived(chatMsg, string.Empty);
				break;
			}
		}

		private void AddAndNotifyMessageReceived(ChatService.ChatMsg chatMsg, string text)
		{
			HudChatController.NewChatMsg newChatMsg = new HudChatController.NewChatMsg();
			newChatMsg.text = text;
			this.ChatMsgList.Add(chatMsg);
			if (this.OnClientChatMessageReceived != null)
			{
				this.OnClientChatMessageReceived(newChatMsg);
			}
			this._chatComponent.AddReceivedMessage(chatMsg.Text);
		}

		private void CreateDefaultChatMessage(ChatService.ChatMsg chatMsg)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(chatMsg.PlayerAddress);
			string text = string.Format("[{0}] {1}: {2}", Language.Get((!chatMsg.IsGroup) ? "CHAT_SCOPE_ALL" : "CHAT_SCOPE_TEAM", TranslationSheets.Chat), this.GetColoredPlayerName(chatMsg.PlayerAddress), chatMsg.Text);
			if (playerByAddress != null && !playerByAddress.IsBot && !GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				HMMHub hub = GameHubBehaviour.Hub;
				TeamUtils.GetUserTagAsync(hub, playerByAddress.UserId, delegate(string teamTag)
				{
					string text = string.Format("[{0}] {1} {2}: {3}", new object[]
					{
						Language.Get((!chatMsg.IsGroup) ? "CHAT_SCOPE_ALL" : "CHAT_SCOPE_TEAM", TranslationSheets.Chat),
						teamTag,
						this.GetColoredPlayerName(chatMsg.PlayerAddress),
						chatMsg.Text
					});
					this.AddAndNotifyMessageReceived(chatMsg, text);
				}, delegate(Exception exception)
				{
					ChatService.Log.WarnFormat("Error on GetUserTagAsync. Exception:{0}", new object[]
					{
						exception
					});
					this.AddAndNotifyMessageReceived(chatMsg, text);
				});
			}
			else
			{
				this.AddAndNotifyMessageReceived(chatMsg, text);
			}
		}

		private string CreatePlayerNotificationChatMessage(ChatService.ChatMsg chatMsg)
		{
			return this.GetColoredPlayerName(chatMsg.PlayerAddress) + " " + chatMsg.Text;
		}

		private string GetColoredPlayerName(byte playerAddress)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(playerAddress);
			if (playerByAddress == null)
			{
				return " ";
			}
			Color chatColor = GUIColorsInfo.GetChatColor(playerByAddress.PlayerId, playerByAddress.Team);
			string arg = HudUtils.RGBToHex(chatColor);
			string arg2 = string.Empty;
			if (this.ShowCharacterName && playerByAddress.Character)
			{
				arg2 = string.Format(" ({0})", playerByAddress.Character.LocalizedName);
			}
			string arg3 = NGUIText.EscapeSymbols(playerByAddress.Name);
			if (!playerByAddress.IsNarrator)
			{
				return string.Format("[{0}]{1}{2}[-]", arg, arg3, arg2);
			}
			return string.Format("[{0}]{1} ({2})[-]", arg, arg3, Language.Get("CHAT_TAG_SPECTATOR", "Chat"));
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.ChatMsgList.Clear();
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

		public IChatServiceAsync Async()
		{
			return this.Async(0);
		}

		public IChatServiceAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new ChatServiceAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IChatServiceDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new ChatServiceDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IChatServiceDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new ChatServiceDispatch(this.OID);
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
			if (classId != 1024)
			{
				throw new Exception("Hierarchy in RemoteClass is not allowed!!! " + classId);
			}
			this._delayed = null;
			if (methodId == 4)
			{
				this.ReceiveMessage((bool)args[0], (string)args[1]);
				return null;
			}
			if (methodId != 7)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.ClientReceiveMessage((bool)args[0], (string)args[1], (byte)args[2]);
			return null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ChatService));

		public bool ShowCharacterName;

		public List<ChatService.ChatMsg> ChatMsgList;

		private IChatComponent _chatComponent;

		public const int StaticClassId = 1024;

		private Identifiable _identifiable;

		[ThreadStatic]
		private ChatServiceAsync _async;

		[ThreadStatic]
		private ChatServiceDispatch _dispatch;

		private IFuture _delayed;

		[Serializable]
		public class ChatMsg
		{
			public ChatMsg(string msg, bool isGroup, byte playerAddress, ChatService.ChatMessageKind messageKind)
			{
				this.Text = msg;
				this.IsGroup = isGroup;
				this.PlayerAddress = playerAddress;
				this.MessageKind = messageKind;
			}

			public string Text;

			public bool IsGroup;

			public byte PlayerAddress;

			public ChatService.ChatMessageKind MessageKind;
		}

		public enum ChatMessageKind
		{
			LogMessage,
			PlayerMessage,
			PlayerNotification,
			System
		}
	}
}
