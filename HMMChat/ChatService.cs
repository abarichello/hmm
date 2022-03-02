using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Assets.Scripts.HMM.GameStates.Game.ComponentContainer;
using Assets.Standard_Assets.Scripts.HMM.GameStates.Game.newChat.Component.Api;
using Assets.Standard_Assets.Scripts.HMM.GameStates.Game.newChat.Component.Impl;
using Assets.Standard_Assets.Scripts.HMM.GameStates.Game.newChat.Impl.Adapter;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Chat.Business;
using HeavyMetalMachines.Chat.Filters;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.Social.Friends.Business.BlockedPlayers;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;
using Zenject;

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
			if (GameHubBehaviour.Hub.Match.Kind != 4 && SpectatorController.IsSpectating)
			{
				string msg = Language.Get("SPECTATOR_CHAT_FORBIDDEN", TranslationContext.HUDChat);
				this.SetupPlayerMessage(false, msg, byte.MaxValue, ChatService.ChatMessageKind.LogMessage, true);
				return;
			}
			this.DispatchReliable(new byte[0]).ReceiveMessage(isGroup, text);
		}

		public void SendMessage(bool isGroup, string message)
		{
			this.ClientSendMessage(isGroup, message);
		}

		public void SendDraftMessage(bool toTeam, string draft, ContextTag context, string[] messageParameters)
		{
			this.DispatchReliable(new byte[0]).ReceiveDraftMessage(toTeam, draft, (string)context, messageParameters);
		}

		[RemoteMethod]
		private void ReceiveMessage(bool group, string msg)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			if (!this.IsValidChatSender(playerByAddress))
			{
				return;
			}
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

		[RemoteMethod]
		private void ReceiveDraftMessage(bool toTeam, string draft, string context, string[] messageParameters)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			if (!this.IsValidChatSender(playerByAddress))
			{
				return;
			}
			ChatService.ChatMsg item = new ChatService.ChatMsg(draft, messageParameters, toTeam, playerByAddress.PlayerAddress, ChatService.ChatMessageKind.PlayerMessage);
			this.ChatMsgList.Add(item);
			if (toTeam)
			{
				TeamKind team = playerByAddress.Team;
				int group = (int)team;
				this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(group)).ClientReceiveDraftMessage(true, draft, context, messageParameters, this.Sender);
			}
			else
			{
				this.DispatchReliable(GameHubBehaviour.Hub.AddressGroups.GetGroup(0)).ClientReceiveDraftMessage(false, draft, context, messageParameters, this.Sender);
			}
			ChatService.Log.InfoFormat("Player={0} Group={1} Draft={2} MessageParameters={3}", new object[]
			{
				playerByAddress.PlayerId,
				(!toTeam) ? "All" : playerByAddress.Team.ToString(),
				draft,
				string.Join(";", messageParameters)
			});
		}

		private bool IsValidChatSender(PlayerData player)
		{
			return !player.IsNarrator || GameHubBehaviour.Hub.Match.Kind == 4;
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
			this.SetupPlayerMessage(group, msg, playeraddress, ChatService.ChatMessageKind.PlayerMessage, false);
		}

		[RemoteMethod]
		private void ClientReceiveDraftMessage(bool toTeam, string draft, string context, string[] messageParameters, byte playeraddress)
		{
			string formatted = Language.MainTranslatedLanguage.GetFormatted(draft, (ContextTag)context, messageParameters);
			this.SetupPlayerMessage(toTeam, formatted, playeraddress, ChatService.ChatMessageKind.PlayerMessage, true);
		}

		public void SetupPlayerMessage(bool group, string msg, byte playeraddress, ChatService.ChatMessageKind messageKind, bool isDraftMessage)
		{
			if (messageKind != ChatService.ChatMessageKind.PlayerMessage && GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.Replay)
			{
				return;
			}
			if (SpectatorController.IsSpectating && (group || messageKind == ChatService.ChatMessageKind.PlayerNotification))
			{
				return;
			}
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(playeraddress);
			if (playerByAddress != null)
			{
				if (this._blockedInGroupChat.IsBlocked(playerByAddress.ConvertToPlayer()))
				{
					return;
				}
				IIsPlayerBlocked isPlayerBlocked = this._diContainer.Resolve<IIsPlayerBlocked>();
				if (isPlayerBlocked.IsBlocked(playerByAddress.PlayerId))
				{
					return;
				}
				IIsPlayerRestrictedByTextChat isPlayerRestrictedByTextChat = this._diContainer.Resolve<IIsPlayerRestrictedByTextChat>();
				if (isPlayerRestrictedByTextChat.IsPlayerRestricted(playerByAddress.PlayerId))
				{
					return;
				}
				ITextChatRestriction textChatRestriction = this._diContainer.Resolve<ITextChatRestriction>();
				if (!isDraftMessage && textChatRestriction.IsEnabledByPlayer(playerByAddress.PlayerId))
				{
					return;
				}
			}
			byte playerAddress = (!(playerByAddress == null)) ? playerByAddress.PlayerAddress : 0;
			string msg2;
			this._chatMessageBadWordFilter.Filter(msg, ref msg2);
			ChatService.ChatMsg chatMsg = new ChatService.ChatMsg(msg2, group, playerAddress, messageKind);
			this.ChatMsgList.Add(chatMsg);
			this.OnNewClientMessage(chatMsg);
		}

		public void ClientReceiveLogMessage(string msg)
		{
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.Replay)
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
			StringBuilder stringBuilder = new StringBuilder(string.Empty);
			this.AppendScopeTag(stringBuilder, chatMsg.IsGroup);
			this.AppendColorBegin(stringBuilder, playerByAddress);
			this.AppendSpectatorTag(stringBuilder, playerByAddress);
			this.AppendPlayerName(stringBuilder, playerByAddress);
			this.AppendPublisherPlayerName(stringBuilder, playerByAddress);
			this.AppendColorEnd(stringBuilder, playerByAddress);
			stringBuilder.AppendFormat(": {0}", chatMsg.Text);
			this.AddAndNotifyMessageReceived(chatMsg, stringBuilder.ToString());
		}

		private void AppendScopeTag(StringBuilder stringBuilder, bool isGroup)
		{
			string arg = Language.Get((!isGroup) ? "CHAT_SCOPE_ALL" : "CHAT_SCOPE_TEAM", TranslationContext.Chat);
			stringBuilder.AppendFormat("[{0}] ", arg);
		}

		private void AppendSpectatorTag(StringBuilder stringBuilder, PlayerData playerData)
		{
			if (playerData.IsNarrator)
			{
				stringBuilder.AppendFormat("[{0}] ", Language.Get("CHAT_TAG_SPECTATOR", TranslationContext.Chat));
			}
		}

		private string CreatePlayerNotificationChatMessage(ChatService.ChatMsg chatMsg)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(chatMsg.PlayerAddress);
			StringBuilder stringBuilder = new StringBuilder(string.Empty);
			this.AppendColorBegin(stringBuilder, playerByAddress);
			this.AppendPlayerName(stringBuilder, playerByAddress);
			this.AppendPublisherPlayerName(stringBuilder, playerByAddress);
			this.AppendColorEnd(stringBuilder, playerByAddress);
			stringBuilder.AppendFormat(" {0}", chatMsg.Text);
			return stringBuilder.ToString();
		}

		private void AppendColorBegin(StringBuilder stringBuilder, PlayerData playerData)
		{
			if (playerData == null)
			{
				return;
			}
			Color chatColor = GUIColorsInfo.GetChatColor(playerData.PlayerId, playerData.Team, playerData.IsNarrator);
			string arg = HudUtils.RGBToHex(chatColor);
			stringBuilder.AppendFormat("[{0}]", arg);
		}

		private void AppendColorEnd(StringBuilder stringBuilder, PlayerData playerData)
		{
			if (playerData == null)
			{
				return;
			}
			stringBuilder.Append("[-]");
		}

		private void AppendPlayerName(StringBuilder stringBuilder, PlayerData playerData)
		{
			if (playerData == null)
			{
				return;
			}
			string text = NGUIText.EscapeSymbols(playerData.Name);
			string formattedNickNameWithPlayerTag = this._diContainer.Resolve<IGetDisplayableNickName>().GetFormattedNickNameWithPlayerTag(playerData.PlayerId, text, new long?(playerData.PlayerTag));
			stringBuilder.Append(formattedNickNameWithPlayerTag);
		}

		private void AppendPublisherPlayerName(StringBuilder stringBuilder, PlayerData playerData)
		{
			if (playerData == null)
			{
				return;
			}
			Publisher publisherById = Publishers.GetPublisherById(playerData.PublisherId);
			PublisherPresentingData publisherPresentingData = this._getPublisherPresentingData.Get(publisherById);
			if (!publisherPresentingData.ShouldShowPublisherUserName)
			{
				return;
			}
			string arg = NGUIText.EscapeSymbols(playerData.PublisherUserName);
			stringBuilder.AppendFormat(" ({0})", arg);
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

		public object Invoke(int classId, short methodId, object[] args, BitStream bitstream = null)
		{
			this._delayed = null;
			switch (methodId)
			{
			case 5:
				this.ReceiveMessage((bool)args[0], (string)args[1]);
				return null;
			case 6:
				this.ReceiveDraftMessage((bool)args[0], (string)args[1], (string)args[2], (string[])args[3]);
				return null;
			case 10:
				this.ClientReceiveMessage((bool)args[0], (string)args[1], (byte)args[2]);
				return null;
			case 11:
				this.ClientReceiveDraftMessage((bool)args[0], (string)args[1], (string)args[2], (string[])args[3], (byte)args[4]);
				return null;
			}
			throw new ScriptMethodNotFoundException(classId, (int)methodId);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ChatService));

		[Inject]
		private IMatchTeams _teams;

		[Inject]
		private DiContainer _diContainer;

		[InjectOnClient]
		private IChatMessageBadWordFilter _chatMessageBadWordFilter;

		[InjectOnClient]
		private IGetPublisherPresentingData _getPublisherPresentingData;

		[InjectOnClient]
		private IIsPlayerBlockedInGroupChat _blockedInGroupChat;

		public List<ChatService.ChatMsg> ChatMsgList;

		private IChatComponent _chatComponent;

		public const int StaticClassId = 1025;

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

			public ChatMsg(string draft, string[] messageParameters, bool toTeam, byte playerAddress, ChatService.ChatMessageKind messageKind)
			{
				this.Draft = draft;
				this.MessageParameters = messageParameters;
				this.IsGroup = toTeam;
				this.PlayerAddress = playerAddress;
				this.MessageKind = messageKind;
			}

			public string Text;

			public bool IsGroup;

			public byte PlayerAddress;

			public ChatService.ChatMessageKind MessageKind;

			public string Draft;

			public string[] MessageParameters;
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
