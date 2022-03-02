using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Chat.Business;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Social.Friends.Business;
using HeavyMetalMachines.Social.Friends.Business.BlockedPlayers;
using HeavyMetalMachines.Social.Friends.Business.Invites;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Social.Groups.Models;
using HeavyMetalMachines.VFX.PlotKids;
using HeavyMetalMachines.VoiceChat.Business;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	[Obsolete]
	public class GroupMemberContextMenuModalGUI : ModalGUIController
	{
		public GroupMember GroupMember
		{
			get
			{
				return this._groupMember;
			}
			set
			{
				this._groupMember = value;
				this._groupMemberPlayer = this._groupMember.ConvertToPlayer();
				this._nameLabel.text = this._groupMember.PlayerName;
				HMMHub hub = GameHubBehaviour.Hub;
				bool flag = string.Equals(this._groupMember.UniversalID, hub.User.UniversalId, StringComparison.InvariantCultureIgnoreCase);
				bool flag2 = ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner;
				bool flag3 = this._groupMember.GroupId.Equals(Guid.Empty);
				this._promoteToOwnerButton.gameObject.SetActive(!flag && flag2 && !flag3 && !hub.IsWaitingInQueue());
				this._removeFromGroupButton.gameObject.SetActive(!flag && flag2);
				IIsPlayerLocalPlayerFriend isPlayerLocalPlayerFriend = this._diContainer.Resolve<IIsPlayerLocalPlayerFriend>();
				bool flag4 = isPlayerLocalPlayerFriend.IsFriend(this._groupMember.PlayerId);
				IIsPlayerBlocked isPlayerBlocked = this._diContainer.Resolve<IIsPlayerBlocked>();
				if (isPlayerBlocked.IsBlocked(this._groupMember.PlayerId))
				{
					this._sendMessageButton.gameObject.SetActive(false);
				}
				this._addFriendButton.gameObject.SetActive(!flag && !flag4);
				this._actionsTable.repositionNow = true;
				this.UpdateFriendVoiceMutedStatus();
				this.RefreshGroupChatAllowedState();
			}
		}

		protected override void InitDialogTasks()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.onStateChange;
			ManagerController.Get<GroupManager>().OnGroupUpdate += this.OnGroupUpdate;
			base.SetPositionToCurrentContext(this._contextMenuTransform);
		}

		private void OnGroupUpdate()
		{
			Group currentGroup = GameHubBehaviour.Hub.ClientApi.group.GetCurrentGroup();
			if (currentGroup != null)
			{
				for (int i = 0; i < currentGroup.Members.Count; i++)
				{
					if (string.Equals(currentGroup.Members[i].UniversalID, this._groupMember.UniversalID, StringComparison.Ordinal))
					{
						return;
					}
				}
			}
			base.ResolveModalWindow();
		}

		protected void SetPropertiesTasks(UserFriend userFriend)
		{
			this._nameLabel.text = userFriend.PlayerName;
		}

		public void onButtonClick_OpenGroupChat()
		{
			Debug.Log(string.Format("onButtonClick_OpenGroupChat: {0} ({1})", this.GroupMember.PlayerName, this.GroupMember.UniversalID));
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
			IGroupStorage groupStorage = this._diContainer.Resolve<IGroupStorage>();
			SocialModalGUI.Current.CreateGroupChatTab(groupStorage.Group);
			base.ResolveModalWindow();
		}

		public void onButtonClick_SendMessage()
		{
			Debug.Log(string.Format("onButtonClick_SendMessageToUser: {0} ({1})", this.GroupMember.PlayerName, this.GroupMember.UniversalID));
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
			SocialModalGUI.Current.CreateUserChatTab(this.GroupMember, false, false);
			base.ResolveModalWindow();
		}

		public void onButtonClick_TransferGroupOwnership()
		{
			ManagerController.Get<GroupManager>().PromoteToGroupOwner(this.ConvertedGroupMember());
			base.ResolveModalWindow();
		}

		private GroupMember ConvertedGroupMember()
		{
			GroupMember groupMember = this._groupMember;
			return new GroupMember
			{
				PlayerId = groupMember.PlayerId,
				UniversalId = groupMember.UniversalID,
				IsPendingInvite = groupMember.IsPendingInviteToGroup(),
				Nickname = groupMember.PlayerName,
				IsGroupLeader = groupMember.IsUserGroupLeader()
			};
		}

		public void onButtonClick_KickMemberFromGroup()
		{
			ManagerController.Get<GroupManager>().TryKickMemberOrCancelInvite(this._groupMember);
			base.ResolveModalWindow();
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(76, true);
		}

		public void onButtonClick_AddFriend()
		{
			ISendInvite sendInvite = this._diContainer.Resolve<ISendInvite>();
			sendInvite.Send(this._groupMember.PlayerId);
			IClientButtonBILogger clientButtonBILogger = this._diContainer.Resolve<IClientButtonBILogger>();
			clientButtonBILogger.LogButtonClick(ButtonName.SocialContextMenuFriendsListAddFriend);
			base.ResolveModalWindow();
		}

		public void onButtonClick_LeaveGroup()
		{
			ManagerController.Get<GroupManager>().LeaveGroup(false);
			base.ResolveModalWindow();
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(77, true);
		}

		public void onButtonClick_FriendVoiceHandle()
		{
			ObservableExtensions.Subscribe<Unit>(this._muteVoiceChatPlayer.ToggleMute(this._groupMemberPlayer), delegate(Unit _)
			{
				this.UpdateFriendVoiceMutedStatus();
			});
		}

		private void UpdateFriendVoiceMutedStatus()
		{
			bool flag = this._isVoiceChatPlayerMuted.IsMuted(this._groupMemberPlayer);
			this._headSetIcon_ActivatedSprite.enabled = !flag;
			this._headSetIcon_DesactivatedSprite.enabled = flag;
			this._activatedVoice_Label.enabled = flag;
			this._desactivatedVoice_Label.enabled = !flag;
			Debug.Log(string.Format("Player {0} is mute = {1}", this._groupMember.PlayerName, this._isPlayerSpeakingOnVoiceChat.IsSpeaking(this._groupMemberPlayer)));
		}

		public void onButtonClick_EnableDesableVoiceChat()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._blockPlayerInGroupChat.ToggleBlock(this._groupMemberPlayer), delegate(Unit _)
			{
				this.RefreshGroupChatAllowedState();
			}));
		}

		private void RefreshGroupChatAllowedState()
		{
			bool flag = this._isPlayerBlockedInGroupChat.IsBlocked(this._groupMemberPlayer);
			this._dialogIcon_ActivatedSprite.enabled = flag;
			this._dialogIcon_DesactivatedSprite.enabled = !flag;
			this._activatedDialog_Label.enabled = !flag;
			this._desactivatedDialog_Label.enabled = flag;
		}

		public void onButtonClick_Close()
		{
			base.ResolveModalWindow();
		}

		private void onStateChange(GameState gameState)
		{
			if (gameState is Game)
			{
				base.ResolveModalWindow();
			}
		}

		protected override IEnumerator ResolveModalWindowTasks()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.onStateChange;
			ManagerController.Get<GroupManager>().OnGroupUpdate -= this.OnGroupUpdate;
			yield break;
		}

		[SerializeField]
		private UITable _actionsTable;

		[SerializeField]
		private Transform _contextMenuTransform;

		[SerializeField]
		private UILabel _nameLabel;

		private GroupMember _groupMember;

		private IPlayer _groupMemberPlayer;

		[Header("Buttons References")]
		[SerializeField]
		private UIButton _sendMessageButton;

		[SerializeField]
		private UIButton _promoteToOwnerButton;

		[SerializeField]
		private UIButton _removeFromGroupButton;

		[SerializeField]
		private UIButton _addFriendButton;

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IIsPlayerSpeakingOnVoiceChat _isPlayerSpeakingOnVoiceChat;

		[Inject]
		private IMuteVoiceChatPlayer _muteVoiceChatPlayer;

		[Inject]
		private IIsVoiceChatPlayerMuted _isVoiceChatPlayerMuted;

		[Inject]
		private IIsPlayerBlockedInGroupChat _isPlayerBlockedInGroupChat;

		[Inject]
		private IBlockPlayerInGroupChat _blockPlayerInGroupChat;

		[NonSerialized]
		public SocialModalGUI ParentGUI;

		[SerializeField]
		private UI2DSprite _headSetIcon_ActivatedSprite;

		[SerializeField]
		private UI2DSprite _headSetIcon_DesactivatedSprite;

		[SerializeField]
		private UILabel _activatedVoice_Label;

		[SerializeField]
		private UILabel _desactivatedVoice_Label;

		[SerializeField]
		private UI2DSprite _dialogIcon_ActivatedSprite;

		[SerializeField]
		private UI2DSprite _dialogIcon_DesactivatedSprite;

		[SerializeField]
		private UILabel _activatedDialog_Label;

		[SerializeField]
		private UILabel _desactivatedDialog_Label;
	}
}
