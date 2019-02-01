using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using ClientAPI.Objects.Partial;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ChatOwnerContent
	{
		public ChatOwnerContent(IUser user)
		{
			this._user = user;
			this._chatKind = ChatOwnerContent.ChatContentKind.User;
			this._spamFilter = new SpamFilter(SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamMessageCountThreshold, SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamBlockedChatDuration);
		}

		public ChatOwnerContent(Group group)
		{
			this._group = group;
			this._chatKind = ChatOwnerContent.ChatContentKind.Group;
			this._spamFilter = new SpamFilter(SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamMessageCountThreshold, SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamBlockedChatDuration);
		}

		public ChatOwnerContent(Lobby lobby)
		{
			this._lobby = lobby;
			this._chatKind = ChatOwnerContent.ChatContentKind.CustomMatchLobby;
			this._spamFilter = new SpamFilter(SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamMessageCountThreshold, SingletonMonoBehaviour<SocialController>.Instance.SocialConfiguration.SpamBlockedChatDuration);
		}

		public ChatOwnerContent.ChatContentKind ChatKind
		{
			get
			{
				return this._chatKind;
			}
		}

		public string UniversalId
		{
			get
			{
				ChatOwnerContent.ChatContentKind chatKind = this.ChatKind;
				if (chatKind == ChatOwnerContent.ChatContentKind.Group)
				{
					return this._group.Id.ToString();
				}
				if (chatKind == ChatOwnerContent.ChatContentKind.User)
				{
					return this._user.UniversalID;
				}
				if (chatKind != ChatOwnerContent.ChatContentKind.CustomMatchLobby)
				{
					return null;
				}
				return this._lobby.Id.ToString();
			}
		}

		public string OwnerName
		{
			get
			{
				ChatOwnerContent.ChatContentKind chatKind = this.ChatKind;
				if (chatKind != ChatOwnerContent.ChatContentKind.Group)
				{
					if (chatKind != ChatOwnerContent.ChatContentKind.User)
					{
						if (chatKind == ChatOwnerContent.ChatContentKind.CustomMatchLobby)
						{
							return Language.Get("CHAT_LOBBY_TITLE", TranslationSheets.Chat);
						}
					}
					else
					{
						if (this._user is UserFriend)
						{
							return ((UserFriend)this._user).PlayerName;
						}
						if (this._user is GroupMember)
						{
							return ((GroupMember)this._user).PlayerName;
						}
					}
					return null;
				}
				return Language.Get("GROUP_TITLE", TranslationSheets.Help);
			}
		}

		public bool CanReceiveMessages
		{
			get
			{
				return (this._user == null || !(this._user is UserFriend) || ((UserFriend)this._user).State != FriendState.Offline) && (this.ChatKind != ChatOwnerContent.ChatContentKind.CustomMatchLobby || this._lobby == GameHubBehaviour.Hub.ClientApi.lobby.GetCurrentLobby());
			}
		}

		public bool IsChatCloseActionLocked
		{
			get
			{
				if (this._group != null)
				{
					return ManagerController.Get<GroupManager>().IsUserInGroupOrPendingInvite && ManagerController.Get<GroupManager>().CurrentGroupID == this._group.Id;
				}
				return this.ChatKind == ChatOwnerContent.ChatContentKind.CustomMatchLobby && this._lobby == GameHubBehaviour.Hub.ClientApi.lobby.GetCurrentLobby();
			}
		}

		private void ThrowSystemSpamAlert()
		{
			string spamBlockMessage = this._spamFilter.GetSpamBlockMessage(Time.unscaledTime);
			SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(spamBlockMessage, this.UniversalId, true, false, StackableHintKind.SpamAlert, HintColorScheme.System);
		}

		public bool TrySendMessage(string rawMessage)
		{
			if (this._spamFilter.IsSpam(rawMessage, Time.unscaledTime))
			{
				this.ThrowSystemSpamAlert();
				return false;
			}
			ChatOwnerContent.ChatContentKind chatKind = this.ChatKind;
			if (chatKind != ChatOwnerContent.ChatContentKind.Group)
			{
				if (chatKind != ChatOwnerContent.ChatContentKind.User)
				{
					if (chatKind == ChatOwnerContent.ChatContentKind.CustomMatchLobby)
					{
						ManagerController.Get<MatchManager>().SendMessageToLobby(this._lobby, rawMessage);
					}
				}
				else
				{
					ManagerController.Get<ChatManager>().SendMessageToUser(this._user, rawMessage);
				}
			}
			else
			{
				if (!ManagerController.Get<GroupManager>().IsUserInGroupOrPendingInvite || ManagerController.Get<GroupManager>().CurrentGroupID != this._group.Id)
				{
					SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(Language.Get("GROUP_CHAT_NOT_IN_GROUP", TranslationSheets.Help), this._group.Id.ToString(), true, false, StackableHintKind.None, HintColorScheme.System);
					return false;
				}
				ManagerController.Get<ChatManager>().SendMessageToGroup(this._group, rawMessage);
			}
			return true;
		}

		private ChatOwnerContent.ChatContentKind _chatKind;

		private Group _group;

		private IUser _user;

		private Lobby _lobby;

		private SpamFilter _spamFilter;

		public enum ChatContentKind
		{
			Group,
			User,
			CustomMatchLobby
		}
	}
}
