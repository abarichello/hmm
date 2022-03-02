using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using ClientAPI.Objects.Partial;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Social.Friends.Models;
using HeavyMetalMachines.Social.Groups.Models;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ChatOwnerContent
	{
		public ChatOwnerContent(IUser user, IGetDisplayableNickName getDisplayableNickName)
		{
			this._user = user;
			this._getDisplayableNickName = getDisplayableNickName;
			this.ChatKind = ChatOwnerContent.ChatContentKind.User;
			this._spamFilter = new SpamFilter(SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamMessageCountThreshold, SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamBlockedChatDuration);
		}

		public ChatOwnerContent(IPlayer player, IGetDisplayableNickName getDisplayableNickName)
		{
			this._player = player;
			this._getDisplayableNickName = getDisplayableNickName;
			this.ChatKind = ChatOwnerContent.ChatContentKind.Player;
			this._spamFilter = new SpamFilter(SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamMessageCountThreshold, SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamBlockedChatDuration);
		}

		public ChatOwnerContent(Group group, IGetDisplayableNickName getDisplayableNickName)
		{
			this._group = group;
			this._getDisplayableNickName = getDisplayableNickName;
			this.ChatKind = ChatOwnerContent.ChatContentKind.Group;
			this._spamFilter = new SpamFilter(SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamMessageCountThreshold, SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamBlockedChatDuration);
		}

		public ChatOwnerContent(Lobby lobby, IGetDisplayableNickName getDisplayableNickName)
		{
			this._lobby = lobby;
			this._getDisplayableNickName = getDisplayableNickName;
			this.ChatKind = ChatOwnerContent.ChatContentKind.CustomMatchLobby;
			this._spamFilter = new SpamFilter(SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamMessageCountThreshold, SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamBlockedChatDuration);
		}

		public ChatOwnerContent.ChatContentKind ChatKind { get; private set; }

		public string UniversalId
		{
			get
			{
				switch (this.ChatKind)
				{
				case ChatOwnerContent.ChatContentKind.Group:
					return this._group.Guid.ToString();
				case ChatOwnerContent.ChatContentKind.User:
					return this._user.UniversalID;
				case ChatOwnerContent.ChatContentKind.CustomMatchLobby:
					return this._lobby.Id.ToString();
				case ChatOwnerContent.ChatContentKind.Player:
					return this._player.UniversalId;
				default:
					return null;
				}
			}
		}

		public string OwnerName
		{
			get
			{
				switch (this.ChatKind)
				{
				case ChatOwnerContent.ChatContentKind.Group:
					return this.GetDisplayableOwnerNameAsGroup();
				case ChatOwnerContent.ChatContentKind.User:
					return this.GetDisplayableOwnerNameAsUser();
				case ChatOwnerContent.ChatContentKind.CustomMatchLobby:
					return this.GetDisplayableOwnerNameAsCustomMatchLobby();
				case ChatOwnerContent.ChatContentKind.Player:
					return this.GetDisplayableOwnerNameAsPlayer();
				default:
					return null;
				}
			}
		}

		private string GetDisplayableOwnerNameAsGroup()
		{
			return Language.Get("GROUP_TITLE", TranslationContext.Help);
		}

		private string GetDisplayableOwnerNameAsUser()
		{
			UserFriend userFriend = this._user as UserFriend;
			if (userFriend != null)
			{
				return this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(userFriend.PlayerId, userFriend.PlayerName, userFriend.PlayerNameTag);
			}
			GroupMember groupMember = this._user as GroupMember;
			if (groupMember != null)
			{
				return this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(groupMember.PlayerId, groupMember.PlayerName, groupMember.NameTag);
			}
			return null;
		}

		private string GetDisplayableOwnerNameAsPlayer()
		{
			return this._getDisplayableNickName.GetFormattedNickNameWithPlayerTag(this._player.PlayerId, this._player.Nickname, this._player.PlayerTag);
		}

		private string GetDisplayableOwnerNameAsCustomMatchLobby()
		{
			return Language.Get("CHAT_LOBBY_TITLE", TranslationContext.Chat);
		}

		public bool CanReceiveMessages
		{
			get
			{
				Friend friend = this._player as Friend;
				if (friend != null)
				{
					return friend.IsOnline;
				}
				return (this._user == null || !(this._user is UserFriend) || ((UserFriend)this._user).State != 1) && (this.ChatKind != ChatOwnerContent.ChatContentKind.CustomMatchLobby || this._lobby == GameHubBehaviour.Hub.ClientApi.lobby.GetCurrentLobby());
			}
		}

		public bool IsChatCloseActionLocked
		{
			get
			{
				if (this._group != null)
				{
					return ManagerController.Get<GroupManager>().IsUserInGroupOrPendingInvite && ManagerController.Get<GroupManager>().CurrentGroupID == this._group.Guid;
				}
				return this.ChatKind == ChatOwnerContent.ChatContentKind.CustomMatchLobby && this._lobby == GameHubBehaviour.Hub.ClientApi.lobby.GetCurrentLobby();
			}
		}

		private void ThrowSystemSpamAlert()
		{
			string spamBlockMessage = this._spamFilter.GetSpamBlockMessage(Time.unscaledTime);
			SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(spamBlockMessage, this.UniversalId, true, false, StackableHintKind.SpamAlert, HintColorScheme.System);
		}

		public bool TrySendMessage(string rawMessage, string bag)
		{
			if (this._spamFilter.IsSpam(rawMessage, Time.unscaledTime))
			{
				this.ThrowSystemSpamAlert();
				return false;
			}
			switch (this.ChatKind)
			{
			case ChatOwnerContent.ChatContentKind.Group:
				if (!ManagerController.Get<GroupManager>().IsUserInGroupOrPendingInvite || ManagerController.Get<GroupManager>().CurrentGroupID != this._group.Guid)
				{
					SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(Language.Get("GROUP_CHAT_NOT_IN_GROUP", TranslationContext.Help), this._group.Guid.ToString(), true, false, StackableHintKind.None, HintColorScheme.System);
					return false;
				}
				ManagerController.Get<ChatManager>().SendMessageToGroup(this._group, rawMessage, bag);
				break;
			case ChatOwnerContent.ChatContentKind.User:
				ManagerController.Get<ChatManager>().SendMessageToUser(this._user, rawMessage, bag);
				break;
			case ChatOwnerContent.ChatContentKind.CustomMatchLobby:
				ManagerController.Get<MatchManager>().SendMessageToLobby(this._lobby, rawMessage, bag);
				break;
			case ChatOwnerContent.ChatContentKind.Player:
				ManagerController.Get<ChatManager>().SendMessageToPlayer(this._player, rawMessage, bag);
				break;
			}
			return true;
		}

		private Group _group;

		private IUser _user;

		private Lobby _lobby;

		private IPlayer _player;

		private readonly IGetDisplayableNickName _getDisplayableNickName;

		private SpamFilter _spamFilter;

		public enum ChatContentKind
		{
			Group,
			User,
			CustomMatchLobby,
			Player
		}
	}
}
