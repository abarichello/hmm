using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.VFX.PlotKids;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
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
				this._nameLabel.text = this._groupMember.PlayerName;
				bool flag = string.Equals(this._groupMember.UniversalID, GameHubBehaviour.Hub.User.UniversalId, StringComparison.InvariantCultureIgnoreCase);
				bool flag2 = ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner;
				bool flag3 = this._groupMember.GroupId.Equals(Guid.Empty);
				this._promoteToOwnerButton.gameObject.SetActive(!flag && flag2 && !flag3);
				this._removeFromGroupButton.gameObject.SetActive(!flag && flag2);
				bool flag4 = ManagerController.Get<FriendManager>().FriendsDictionary.ContainsKey(this._groupMember.UniversalID);
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
			SocialModalGUI.Current.CreateGroupChatTab(ManagerController.Get<GroupManager>().GetCurrentGroupIfExists());
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
			ManagerController.Get<GroupManager>().PromoteToGroupOwner(this._groupMember);
			base.ResolveModalWindow();
		}

		public void onButtonClick_KickMemberFromGroup()
		{
			ManagerController.Get<GroupManager>().TryKickMemberOrCancelInvite(this._groupMember);
			base.ResolveModalWindow();
		}

		public void onButtonClick_AddFriend()
		{
			SingletonMonoBehaviour<SocialController>.Instance.OpenSteamFriendInvite(this._groupMember.UniversalID);
			base.ResolveModalWindow();
		}

		public void onButtonClick_LeaveGroup()
		{
			ManagerController.Get<GroupManager>().LeaveGroup(false);
			base.ResolveModalWindow();
		}

		public void onButtonClick_FriendVoiceHandle()
		{
			SingletonMonoBehaviour<VoiceChatController>.Instance.ToggleMuteUser(this._groupMember.UniversalID);
			this.UpdateFriendVoiceMutedStatus();
		}

		private void UpdateFriendVoiceMutedStatus()
		{
			bool flag = SingletonMonoBehaviour<VoiceChatController>.Instance.IsUserMuted(this._groupMember.UniversalID);
			this._headSetIcon_ActivatedSprite.enabled = !flag;
			this._headSetIcon_DesactivatedSprite.enabled = flag;
			this._activatedVoice_Label.enabled = flag;
			this._desactivatedVoice_Label.enabled = !flag;
			Debug.Log(string.Format("Player {0} is mute = {1}", this._groupMember.PlayerName, SingletonMonoBehaviour<VoiceChatController>.Instance.IsUserSpeaking(this._groupMember.UniversalID)));
		}

		public void onButtonClick_EnableDesableVoiceChat()
		{
			ManagerController.Get<ChatManager>().ToggleIgnoreUserGroupChat(this._groupMember.UniversalID);
			this.RefreshGroupChatAllowedState();
		}

		private void RefreshGroupChatAllowedState()
		{
			bool flag = !ManagerController.Get<ChatManager>().IsUserIgnored(this._groupMember.UniversalID);
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

		[Header("Buttons References")]
		[SerializeField]
		private UIButton _sendMessageButton;

		[SerializeField]
		private UIButton _promoteToOwnerButton;

		[SerializeField]
		private UIButton _removeFromGroupButton;

		[SerializeField]
		private UIButton _addFriendButton;

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
