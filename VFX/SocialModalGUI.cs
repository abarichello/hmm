using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Chat;
using ClientAPI.Objects;
using ClientAPI.Objects.Partial;
using FMod;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX.PlotKids;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class SocialModalGUI : ModalGUIController
	{
		public static bool IsWindowOpened
		{
			get
			{
				return SocialModalGUI.Current.Panel.alpha > float.Epsilon;
			}
		}

		public void UpdatePlayerListSorting()
		{
			this._playerListTable.repositionNow = true;
		}

		protected override void InitDialogTasks()
		{
			bool flag = this._openChatTabsDictionary.Count > 0;
			this._chatMessagesGroup.SetActive(flag);
			this.ChatUIInput.enabled = flag;
			this._chatUIInputLabel = this.ChatUIInput.GetComponent<UILabel>();
			this._chatUIInputLabel.enabled = flag;
			if (this._activeTabBaseGUI != null)
			{
				SingletonMonoBehaviour<SocialController>.Instance.ChatUiFeedbackDispatcher.ClearPendingChatMessagesFromUID(this._activeTabBaseGUI.ReferenceObject.UniversalId);
			}
			this.UpdateFriendList(ManagerController.Get<FriendManager>());
			if (this._activeTabsList.Count > 0)
			{
				UICamera.selectedObject = this.ChatUIInput.gameObject;
			}
			this._playerListTable.repositionNow = true;
			this._chatTabListTable.repositionNow = true;
		}

		protected override IEnumerator ResolveModalWindowTasks()
		{
			if (this.IsChatInputFieldSelected())
			{
				UICamera.selectedObject = null;
			}
			yield break;
		}

		protected override void Start()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.OnStateChange;
			MessageHintGuiItem.EvtMessageHintClicked += this.OnMessageHintClicked;
			this._accessCodeRegex = new Regex("\\bC[A-Z0-9]{4,10}\\d\\b");
			this._reversingAccessCodeRegex = new Regex("\\[url=.*?\\](?:.*?\\[u\\](.*?)\\[\\/u\\].*)\\[\\/url\\]\\s\\(.*?\\)");
			base.Start();
		}

		private void OnMessageHintClicked(string ownerId)
		{
			bool flag = false;
			for (int i = 0; i < this._activeTabsList.Count; i++)
			{
				ChatTabGuiItem chatTabGuiItem = this._activeTabsList[i];
				if (string.Equals(chatTabGuiItem.ReferenceObject.UniversalId, ownerId, StringComparison.OrdinalIgnoreCase))
				{
					this.SetActiveTab(chatTabGuiItem);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.OnStateChange;
			MessageHintGuiItem.EvtMessageHintClicked -= this.OnMessageHintClicked;
		}

		private void OnSpectatorMatchSelectionScreenOpen(bool matchSelectionIsOpen)
		{
			UnityEngine.Debug.LogWarning(string.Format("OnSpectatorMatchSelectionScreenOpen - {0}", matchSelectionIsOpen));
			if (!matchSelectionIsOpen)
			{
				return;
			}
			if (!Mathf.Approximately(1f, base.Panel.alpha))
			{
				return;
			}
			SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<SocialModalGUI>();
		}

		public override bool CanOpen()
		{
			if (SingletonMonoBehaviour<PanelController>.Instance.IsModalOfTypeOpened<StoreConfirmationWindow>())
			{
				return false;
			}
			if (GameHubBehaviour.Hub.GuiScripts.Loading.IsLoading)
			{
				return false;
			}
			if (GameHubBehaviour.Hub.GuiScripts.Esc.IsWindowVisible())
			{
				return false;
			}
			GameState gameState = GameHubBehaviour.Hub.State.Current;
			return gameState is MainMenu;
		}

		private void OnStateChange(GameState gameState)
		{
			if (!SocialModalGUI.IsWindowOpened)
			{
				return;
			}
			if (!(gameState is Game) && !(gameState is LoadingState))
			{
				return;
			}
			base.ResolveModalWindow();
		}

		protected void OnEnable()
		{
			SocialModalGUI.Current = this;
			ManagerController.Get<FriendManager>().EvtFriendListUpdated += this.OnFriendListUpdated;
			ManagerController.Get<FriendManager>().EvtFriendRefresh += this.OnFriendRefreshed;
			ManagerController.Get<ChatManager>().EvtMessageReceived += this.OnMessageReceived;
			ManagerController.Get<ChatManager>().EvtGroupMessageReceived += this.onGroupMessageReceived;
			ManagerController.Get<GroupManager>().OnGroupUpdate += this.OnGroupUpdated;
			ManagerController.Get<MatchManager>().EvtLobbyJoined += this.OnLobbyJoined;
			PanelController.EvtSystemMessage += this.EvtSystemMessage;
			ManagerController.Get<GroupManager>().EvtGroupFriendSuggestion += this.onGroupFriendSuggestion;
			ManagerController.Get<GroupManager>().EvtGroupMemberJoin += this.OnGroupMemberJoin;
			ManagerController.Get<GroupManager>().EvtGroupCreated += this.OnGroupCreated;
			ManagerController.Get<GroupManager>().EvtInviteHandled += this.OnInviteHandled;
			SpectatorMatchSelectionGUI.EvtSpectatorMatchSelectionScreenOpen += this.OnSpectatorMatchSelectionScreenOpen;
			SingletonMonoBehaviour<VoiceChatController>.Instance.OnVoiceChatStatusChanged += this.OnVoiceChatStatusChanged;
			this._uiLabelHyperlinkScript.ActionOnHyperlink = new Action<string>(this.onChatHyperlinkClick);
			HudWindowManager.Instance.OnNewWindowAdded += this.OnNewWindowAdded;
			ConfirmWindowReference.EvtConfirmWindowOpened += this.ClearCameraUISelectionIfChatInputSelected;
		}

		private void OnInviteHandled(bool inviteAccepted)
		{
			if (!inviteAccepted)
			{
				return;
			}
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
		}

		private void OnLobbyJoined(Lobby lobby)
		{
			this.CreateLobbyChatTab(lobby);
		}

		protected void OnDisable()
		{
			if (SingletonMonoBehaviour<ManagerController>.DoesInstanceExist())
			{
				ManagerController.Get<FriendManager>().EvtFriendListUpdated -= this.OnFriendListUpdated;
				ManagerController.Get<FriendManager>().EvtFriendRefresh -= this.OnFriendRefreshed;
				ManagerController.Get<ChatManager>().EvtMessageReceived -= this.OnMessageReceived;
				ManagerController.Get<ChatManager>().EvtGroupMessageReceived -= this.onGroupMessageReceived;
				ManagerController.Get<GroupManager>().OnGroupUpdate -= this.OnGroupUpdated;
				ManagerController.Get<GroupManager>().EvtGroupFriendSuggestion -= this.onGroupFriendSuggestion;
				ManagerController.Get<GroupManager>().EvtGroupMemberJoin -= this.OnGroupMemberJoin;
				ManagerController.Get<GroupManager>().EvtGroupCreated -= this.OnGroupCreated;
				ManagerController.Get<GroupManager>().EvtInviteHandled -= this.OnInviteHandled;
				ManagerController.Get<MatchManager>().EvtLobbyJoined -= this.OnLobbyJoined;
			}
			PanelController.EvtSystemMessage -= this.EvtSystemMessage;
			SpectatorMatchSelectionGUI.EvtSpectatorMatchSelectionScreenOpen -= this.OnSpectatorMatchSelectionScreenOpen;
			if (SingletonMonoBehaviour<VoiceChatController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<VoiceChatController>.Instance.OnVoiceChatStatusChanged -= this.OnVoiceChatStatusChanged;
			}
			if (HudWindowManager.DoesInstanceExist())
			{
				HudWindowManager.Instance.OnNewWindowAdded -= this.OnNewWindowAdded;
			}
			ConfirmWindowReference.EvtConfirmWindowOpened -= this.ClearCameraUISelectionIfChatInputSelected;
		}

		private void OnFriendRefreshed(UserFriend userFriend)
		{
			if (userFriend.Status == FriendStatus.Pending)
			{
				this.FriendGuiItemByIDDictionary.Remove(userFriend.UniversalID);
			}
			if (this.FriendGuiItemByIDDictionary.ContainsKey(userFriend.UniversalID))
			{
				return;
			}
			this.UpdateOrInsertFriend(userFriend);
		}

		private void ClearCameraUISelectionIfChatInputSelected()
		{
			if (!this.ChatUIInput.isSelected)
			{
				return;
			}
			UICamera.selectedObject = null;
		}

		private void OnNewWindowAdded(IHudWindow hudWindow)
		{
			if (hudWindow == null)
			{
				return;
			}
			if (hudWindow is PlayerInfoTooltipModalPanel)
			{
				return;
			}
			this.ClearCameraUISelectionIfChatInputSelected();
		}

		private void OnGroupCreated(Guid groupId)
		{
			this.ShowVoiceChatHint(groupId.ToString());
		}

		private void OnGroupMemberJoin(Guid currentGroupID, GroupMember newGroupMember)
		{
			if (!newGroupMember.UniversalID.Equals(GameHubBehaviour.Hub.User.UniversalId))
			{
				return;
			}
			this.ShowVoiceChatHint(currentGroupID.ToString());
		}

		public bool IsChatInputFieldSelected()
		{
			return this.ChatUIInput.isSelected;
		}

		public void UpdateChatInputFeedback()
		{
			bool isSelected = this.ChatUIInput.isSelected;
			if (this.ChatInputEffect.enabled == isSelected)
			{
				return;
			}
			this.ChatInputEffect.enabled = isSelected;
		}

		private void UpdateScreenState()
		{
			if (!this.CanOpen())
			{
				if (Mathf.Approximately(1f, base.Panel.alpha))
				{
					SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<SocialModalGUI>();
				}
				return;
			}
			if (!ControlOptions.GetButtonUp(ControlAction.GUIOpenSocialChat))
			{
				return;
			}
			SingletonMonoBehaviour<PanelController>.Instance.ToggleModalWindow<SocialModalGUI>();
			this.PlaySocialSfx(this.sfx_ui_chat_open);
		}

		protected override void Update()
		{
			this.UpdateScreenState();
			this.UpdateChatInputFeedback();
			if (this.IsWindowVisible() && this._activeTabBaseGUI != null)
			{
				if (UndoTextControl.CheckUndoInput())
				{
					this.ChatUIInput.Set(this._activeTabBaseGUI.Undo(), false);
				}
				if (UndoTextControl.CheckRedoInput())
				{
					this.ChatUIInput.Set(this._activeTabBaseGUI.Redo(), false);
				}
			}
			base.Update();
		}

		public void onButtonClick_CloseChatModalWindow()
		{
			base.ResolveModalWindow();
			this.PlaySocialSfx(this.sfx_ui_chat_close);
		}

		public void onButtonClick_AddNewFriend()
		{
			UnityEngine.Debug.LogWarning("NOT IMPLEMENTED");
		}

		public void OnFriendListUpdated(FriendManager friendManager)
		{
			this.UpdateFriendList(friendManager);
		}

		private string GetShortMsgSenderName(string baseName)
		{
			if (baseName.Length <= 12)
			{
				return baseName;
			}
			return string.Format("{0}...", baseName.Substring(0, 9));
		}

		private void PlaySocialSfx(FMODAsset fmodAsset)
		{
			if (!this.CanOpen())
			{
				return;
			}
			FMODAudioManager.PlayOneShotAt(fmodAsset, base.transform.position, 0);
		}

		public void OnMessageReceived(ChatMessage chatMessage, DateTime receivedTime)
		{
			string message = chatMessage.Message;
			this.ReplaceLocalizedTags(ref message);
			this._uiScrollView.ResetPosition();
			string fromUniversalId = chatMessage.FromUniversalId;
			string text = null;
			IUser user = null;
			this.PlaySocialSfx(this.sfx_ui_chat_message_receive);
			FriendManager friendManager = ManagerController.Get<FriendManager>();
			if (friendManager.FriendsDictionary.ContainsKey(fromUniversalId))
			{
				UserFriend userFriend = friendManager.FriendsDictionary[chatMessage.FromUniversalId].UserFriend;
				user = userFriend;
				text = userFriend.PlayerName;
				if (this._activeTabBaseGUI == null || !string.Equals(fromUniversalId, this._activeTabBaseGUI.ReferenceObject.UniversalId) || !SocialModalGUI.IsWindowOpened)
				{
					string arg;
					if (this._reversingAccessCodeRegex.IsMatch(message))
					{
						arg = this._reversingAccessCodeRegex.Replace(message, new MatchEvaluator(this.ReversingAccessCodeRegexEvaluator));
					}
					else
					{
						arg = message;
					}
					string text2 = string.Format("[{0}]{1}[-]: {2}[-]", HudUtils.RGBToHex(SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.FriendTextColor), text, arg);
					string textContent = text2;
					float timeoutSeconds = 5f;
					string ownerId = fromUniversalId;
					BaseHintContent baseHintContent = new BaseHintContent(textContent, timeoutSeconds, true, null, ownerId);
					SingletonMonoBehaviour<PanelController>.Instance.ShowMessageHint(baseHintContent, StackableHintKind.NewChatMessage, HintColorScheme.Chat);
				}
			}
			if (ManagerController.Get<GroupManager>().GroupMembersByID.ContainsKey(fromUniversalId) && !HudWindowManager.Instance.IsGameTransitioning)
			{
				GroupMember groupMember = ManagerController.Get<GroupManager>().GroupMembersByID[chatMessage.FromUniversalId];
				user = groupMember;
				text = groupMember.PlayerName;
			}
			if (user == null)
			{
				UserFriend userFriend2;
				if (!this._unknownUsers.TryGetValue(chatMessage.FromUniversalId, out userFriend2))
				{
					UnityEngine.Debug.LogWarningFormat("[onMessageReceived] Aborting: Couldn't get sender user from Universal ID: \"{0}\". Message: {1}. Creating mock unknown user!", new object[]
					{
						fromUniversalId,
						chatMessage.Message
					});
					userFriend2 = new UserFriend
					{
						UniversalID = chatMessage.FromUniversalId,
						PlayerName = text
					};
					this._unknownUsers[chatMessage.FromUniversalId] = userFriend2;
				}
				user = userFriend2;
				text = chatMessage.PlayerName;
			}
			if (!this._openChatTabsDictionary.ContainsKey(user.UniversalID))
			{
				this.CreateUserChatTab(user, true, true);
			}
			this.AddFormattedMessageToTab(MessageKind.Friend, message, text, user.UniversalID, chatMessage.FromUniversalId);
			this.RefreshActiveChatTabs();
			this.PlaySocialSfx(this.sfx_ui_chat_invite_receive);
		}

		private string ReversingAccessCodeRegexEvaluator(Match m)
		{
			if (m.Groups.Count > 1)
			{
				return m.Groups[1].Value;
			}
			return m.Value;
		}

		private void onGroupMessageReceived(string chatMessage, string groupID, string playerName, string fromUniversalId, DateTime receivedtime)
		{
			if (!this._openChatTabsDictionary.ContainsKey(groupID))
			{
				UnityEngine.Debug.LogWarning(string.Format("Missing group chat tab for group: {0}, aborting!", groupID));
				return;
			}
			this.ReplaceLocalizedTags(ref chatMessage);
			this.AddFormattedMessageToTab(MessageKind.Group, chatMessage, playerName, fromUniversalId, groupID);
			this.PlaySocialSfx(this.sfx_ui_chat_message_receive);
			this.RefreshActiveChatTabs();
			this.PlaySocialSfx(this.FeedBackAudioFx);
			this._uiScrollView.ResetPosition();
		}

		public void UpdateArrowIconsState()
		{
			bool isEnabled = this._activeTabsList.Count > 3 && this._chatTabNavigationIndex + 3 < this._activeTabsList.Count;
			bool isEnabled2 = this._activeTabsList.Count > 3 && this._chatTabNavigationIndex > 0;
			this._previousChatTabButtonGameobject.isEnabled = isEnabled2;
			this._nextChatTabButtonGameobject.isEnabled = isEnabled;
		}

		public void onButtonPress_PreviousChatTab()
		{
			this._chatTabNavigationIndex--;
			this.RefreshActiveChatTabs();
			this.PlaySocialSfx(this.sfx_ui_chat_click);
		}

		public void onButtonPress_NextChatTab()
		{
			this._chatTabNavigationIndex++;
			this.RefreshActiveChatTabs();
			this.PlaySocialSfx(this.sfx_ui_chat_click);
		}

		public void RefreshActiveChatTabs()
		{
			this.UpdateChatTabNavigation();
			this.UpdateArrowIconsState();
			if (this._activeTabsList.Count == 0)
			{
				return;
			}
			for (int i = 0; i < this._activeTabsList.Count; i++)
			{
				bool active = i >= this._chatTabNavigationIndex && i < this._chatTabNavigationIndex + 3;
				this._activeTabsList[i].UpdateChatTabNameByIndex(i);
				this._activeTabsList[i].gameObject.SetActive(active);
			}
			this._chatTabListTable.repositionNow = true;
			base.StartCoroutine(this.WaitToCheckUnreadMessagesFeedback());
		}

		private IEnumerator WaitToCheckUnreadMessagesFeedback()
		{
			bool hasAnyMessageOnTheLeft = false;
			bool hasAnyMessageOnTheRight = false;
			for (int i = 0; i < this._activeTabsList.Count; i++)
			{
				bool isActive = i >= this._chatTabNavigationIndex && i < this._chatTabNavigationIndex + 3;
				yield return null;
				if (!isActive && this._activeTabsList[i].GotUnreadMessages)
				{
					if (i < this._chatTabNavigationIndex)
					{
						hasAnyMessageOnTheLeft = true;
					}
					if (i > this._chatTabNavigationIndex)
					{
						hasAnyMessageOnTheRight = true;
					}
				}
			}
			this._nextChatTabEffect.SetActive(hasAnyMessageOnTheRight);
			this._previousChatTabEffect.SetActive(hasAnyMessageOnTheLeft);
			yield break;
		}

		public void UpdateChatTabNavigation()
		{
			this._chatTabNavigationIndex = Mathf.Clamp(this._chatTabNavigationIndex, 0, Mathf.Max(0, this._activeTabsList.Count - 3));
		}

		public Dictionary<string, FriendGuiItem> FriendGuiItemByIDDictionary
		{
			get
			{
				return this._friendGuiItemByIDDictionary;
			}
		}

		private void UpdateFriendList(FriendManager friendManager)
		{
			foreach (KeyValuePair<string, FriendHolder> keyValuePair in friendManager.FriendsDictionary)
			{
				this.UpdateOrInsertFriend(keyValuePair.Value.UserFriend);
			}
			this._playerListTable.repositionNow = true;
		}

		private void UpdateOrInsertFriend(UserFriend userFriend)
		{
			FriendFilterGuiItem friendFilterGuiItem;
			switch (userFriend.GetHmmFriendState())
			{
			case HmmFriendState.OnlineHMM:
				friendFilterGuiItem = this._hmmOnlineFilterGuiItem;
				goto IL_82;
			case HmmFriendState.OnlineHMM_Match:
				friendFilterGuiItem = this._playingMatchFilterGuiItem;
				goto IL_82;
			case HmmFriendState.Offline:
				friendFilterGuiItem = this._offlineFilterGuiItem;
				goto IL_82;
			case HmmFriendState.Away:
				friendFilterGuiItem = this._steamAwayFilterGuiItem;
				goto IL_82;
			case HmmFriendState.PlayingOtherGame:
				friendFilterGuiItem = this._steamOtherGameFilterGuiItem;
				goto IL_82;
			case HmmFriendState.OnlineNarrator:
				friendFilterGuiItem = this._hmmOnlineFilterGuiItem;
				goto IL_82;
			}
			friendFilterGuiItem = this._steamOnlineFilterGuiItem;
			IL_82:
			FriendGuiItem friendGuiItem;
			if (this._friendGuiItemByIDDictionary.TryGetValue(userFriend.UniversalID, out friendGuiItem))
			{
				friendGuiItem.SetProperties(userFriend);
			}
			else
			{
				friendGuiItem = this._baseFriendGuiItem.CreateNewGuiItem(userFriend, true, null);
				this._friendGuiItemByIDDictionary.Add(userFriend.UniversalID, friendGuiItem);
			}
			friendGuiItem.AssignParentFilter(friendFilterGuiItem);
			friendGuiItem.ParentPanel = base.Panel;
			friendGuiItem.gameObject.SetActive(friendFilterGuiItem.IsEnabled);
		}

		private void ClearChatInput()
		{
			this.ChatUIInput.Set(string.Empty, false);
			if (this._activeTabBaseGUI != null)
			{
				this._activeTabBaseGUI.ClearMsgText();
			}
		}

		public void OnChange_ChatUiInput()
		{
			if (this._activeTabBaseGUI == null)
			{
				return;
			}
			this._activeTabBaseGUI.MsgTextInput = this.ChatUIInput.value;
		}

		public void EvtSystemMessage(string message, string senderId)
		{
			this.AddSystemMessageToActiveTab(message, senderId);
			this.PlaySocialSfx(this.sfx_ui_chat_member_offline);
		}

		public void onSubmit_SendMessageToFriend()
		{
			string text = NGUIText.StripSymbols(this.ChatUIInput.value);
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			this._uiScrollView.ResetPosition();
			this.PlaySocialSfx(this.sfx_ui_chat_message_sent);
			string rawMessage = text;
			this.TryConvertRawMsgToLobbyCopyUrl(ref rawMessage);
			if (this._activeTabBaseGUI == null)
			{
				return;
			}
			this.ClearChatInput();
			ChatOwnerContent referenceObject = this._activeTabBaseGUI.ReferenceObject;
			if (!referenceObject.CanReceiveMessages)
			{
				string arg = string.Format("[{0}]{1}[-]", HudUtils.RGBToHex(SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OfflinePlayerNameColor), referenceObject.OwnerName);
				this.AddSystemMessageToActiveTab(string.Format(Language.Get("GROUP_OTHERUSER_OFFLINE", TranslationSheets.Help), arg), null);
				return;
			}
			string universalId = referenceObject.UniversalId;
			bool flag = referenceObject.ChatKind == ChatOwnerContent.ChatContentKind.User;
			UserInfo user = GameHubBehaviour.Hub.User;
			if (!referenceObject.TrySendMessage(rawMessage))
			{
				return;
			}
			this.ReplaceLocalizedTags(ref rawMessage);
			this.AddFormattedMessageToTab((!flag) ? MessageKind.SelfOnGroup : MessageKind.SelfPrivate, rawMessage, user.PlayerSF.Name, user.UniversalId, universalId);
		}

		private void AddSystemMessageToActiveTab(string message, string ownerId = null)
		{
			this.AddFormattedMessageToTab(MessageKind.System, message, Language.Get("SYSTEM_TITLE", TranslationSheets.Help), null, ownerId);
		}

		private void TryHighlightChatTabIfNotCurrent(string ownerId)
		{
			ChatTabGuiItem chatTabGuiItem = null;
			if (!this._openChatTabsDictionary.TryGetValue(ownerId, out chatTabGuiItem))
			{
				UnityEngine.Debug.LogErrorFormat("[SocialModalGUI] Trying to highlight tab that doesn't exist! OwnerId: {0}", new object[]
				{
					ownerId
				});
				return;
			}
			if (this._activeTabBaseGUI == chatTabGuiItem)
			{
				return;
			}
			chatTabGuiItem.HighlightChatTab();
		}

		private void AddFormattedMessageToTab(MessageKind msgKind, string rawMessage, string senderName, string senderId, string ownerId)
		{
			if (msgKind != MessageKind.System && string.IsNullOrEmpty(ownerId))
			{
				UnityEngine.Debug.LogWarning(string.Format("Received non-system message without User UID assigned: {0}. Fixing message kind to \"System\"!", ownerId));
				msgKind = MessageKind.System;
			}
			DateTime currentDateTime = DateTime.Now;
			ChatTabGuiItem targetTabItem = null;
			Color targetColor = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.SystemMessageColor;
			switch (msgKind)
			{
			case MessageKind.SelfPrivate:
				targetTabItem = this._activeTabBaseGUI;
				targetColor = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OwnPlayerNameAndTimeColorPrivateChat;
				break;
			case MessageKind.SelfOnGroup:
				targetTabItem = this._activeTabBaseGUI;
				targetColor = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OwnPlayerNameAndTimeColorGroupChat;
				break;
			case MessageKind.Friend:
				targetTabItem = this._openChatTabsDictionary[ownerId];
				targetColor = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OthersPlayersNameAndTimeColorPrivateChat;
				this.TryHighlightChatTabIfNotCurrent(ownerId);
				break;
			case MessageKind.Group:
				targetTabItem = this._openChatTabsDictionary[ownerId];
				targetColor = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OthersPlayersNameAndTimeColorGroupChat;
				this.TryHighlightChatTabIfNotCurrent(ownerId);
				break;
			default:
			{
				if (!string.IsNullOrEmpty(ownerId))
				{
					this._openChatTabsDictionary.TryGetValue(ownerId, out targetTabItem);
				}
				if (targetTabItem == null)
				{
					if (!string.Equals(ownerId, "SystemMessage"))
					{
						return;
					}
					targetTabItem = this.CreateUserChatTab(this._systemMockUser, true, true);
					UnityEngine.Debug.Log(string.Format("Creating systemTAB. [{0}] {1}", msgKind, rawMessage));
				}
				string message = string.Format("[{0}][{1:00}:{2:00}]: {3}", new object[]
				{
					HudUtils.RGBToHex(targetColor),
					currentDateTime.Hour,
					currentDateTime.Minute,
					rawMessage
				});
				targetTabItem.AppendLineToTab(message);
				return;
			}
			}
			string shortSenderName = this.GetShortMsgSenderName(NGUIText.EscapeSymbols(senderName));
			string formattedMessage = string.Format("[{0}][{1:00}:{2:00}] {3}: {4}", new object[]
			{
				HudUtils.RGBToHex(targetColor),
				currentDateTime.Hour,
				currentDateTime.Minute,
				shortSenderName,
				rawMessage
			});
			if (!string.IsNullOrEmpty(senderId))
			{
				TeamUtils.GetUserTagAsync(GameHubBehaviour.Hub, senderId, delegate(string teamTag)
				{
					if (string.IsNullOrEmpty(teamTag))
					{
						targetTabItem.AppendLineToTab(formattedMessage);
						return;
					}
					string message2 = string.Format("[{0}][{1:00}:{2:00}] {3} {4}: {5}", new object[]
					{
						HudUtils.RGBToHex(targetColor),
						currentDateTime.Hour,
						currentDateTime.Minute,
						teamTag,
						shortSenderName,
						rawMessage
					});
					targetTabItem.AppendLineToTab(message2);
				}, delegate(Exception exception)
				{
					SocialModalGUI.Log.WarnFormat("Error on GetUserTagAsync. Exception:{0}", new object[]
					{
						exception
					});
					targetTabItem.AppendLineToTab(formattedMessage);
				});
			}
			else
			{
				targetTabItem.AppendLineToTab(formattedMessage);
			}
		}

		private UserFriend _systemMockUser
		{
			get
			{
				if (this.m_systemMockUser != null)
				{
					return this.m_systemMockUser;
				}
				this.m_systemMockUser = new UserFriend();
				this.m_systemMockUser.PlayerName = Language.Get("SYSTEM_TITLE", TranslationSheets.Help);
				this.m_systemMockUser.UniversalID = "SystemMessage";
				return this.m_systemMockUser;
			}
		}

		public void TryUpdateChatLabel(ChatTabGuiItem chatTabItem, string chatContents)
		{
			if (chatTabItem != this._activeTabBaseGUI)
			{
				return;
			}
			this._chatLabel.text = chatContents;
			this._chatLabel.ProcessText();
		}

		public ChatTabGuiItem CreateLobbyChatTab(Lobby lobby)
		{
			this.PlaySocialSfx(this.sfx_ui_chat_group_create);
			ChatTabGuiItem chatTabGuiItem;
			if (this._openChatTabsDictionary.TryGetValue(lobby.Id.ToString(), out chatTabGuiItem))
			{
				this.SetActiveTab(chatTabGuiItem);
				return chatTabGuiItem;
			}
			chatTabGuiItem = this.CreateChatTab(new ChatOwnerContent(lobby), false);
			string systemMessage = Language.Get("CUSTOM_MATCH_CHAT_INFO", TranslationSheets.Chat);
			SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(systemMessage, lobby.Id.ToString(), true, false, StackableHintKind.None, HintColorScheme.System);
			this.ShowVoiceChatHint(lobby.Id.ToString());
			return chatTabGuiItem;
		}

		public ChatTabGuiItem CreateGroupChatTab(ClientAPI.Objects.Group group)
		{
			UnityEngine.Debug.LogWarningFormat("CREATING GROUP CHAT TAB. Is Group Null? {0}", new object[]
			{
				group == null
			});
			this.PlaySocialSfx(this.sfx_ui_chat_group_create);
			ChatTabGuiItem chatTabGuiItem;
			if (this._openChatTabsDictionary.TryGetValue(ManagerController.Get<GroupManager>().CurrentGroupID.ToString(), out chatTabGuiItem))
			{
				this.SetActiveTab(chatTabGuiItem);
				return chatTabGuiItem;
			}
			if (group == null)
			{
				group = new ClientAPI.Objects.Group(ManagerController.Get<GroupManager>().CurrentGroupID, ManagerController.Get<GroupManager>().GroupMembersSortedList);
			}
			chatTabGuiItem = this.CreateChatTab(new ChatOwnerContent(group), false);
			return chatTabGuiItem;
		}

		public ChatTabGuiItem CreateUserChatTab(IUser targetUser, bool wasMessageReceived = false, bool ignoreSteamOverlay = false)
		{
			UserFriend userFriend = targetUser as UserFriend;
			if (userFriend != null && userFriend.State != FriendState.PlayingGame && !ignoreSteamOverlay)
			{
				SingletonMonoBehaviour<SocialController>.Instance.OpenSteamChatWithFriend(userFriend);
				return null;
			}
			if (!wasMessageReceived)
			{
				SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
			}
			ChatTabGuiItem chatTabGuiItem;
			if (!this._openChatTabsDictionary.TryGetValue(targetUser.UniversalID, out chatTabGuiItem))
			{
				chatTabGuiItem = this.CreateChatTab(new ChatOwnerContent(targetUser), wasMessageReceived);
				return chatTabGuiItem;
			}
			if (wasMessageReceived)
			{
				return chatTabGuiItem;
			}
			this.SetActiveTab(chatTabGuiItem);
			return chatTabGuiItem;
		}

		private ChatTabGuiItem CreateChatTab(ChatOwnerContent chatOwnerContent, bool wasMessageReceived = false)
		{
			ChatTabGuiItem chatTabGuiItem = this._baseChatTabGuiItem.CreateNewGuiItem(chatOwnerContent, true, null);
			this.PlaySocialSfx(this.sfx_ui_chat_tab_new);
			this._openChatTabsDictionary.Add(chatOwnerContent.UniversalId, chatTabGuiItem);
			this._activeTabsList.Insert(this._activeTabsList.Count, chatTabGuiItem);
			this._chatTabListTable.repositionNow = true;
			if (this._activeTabsList.Count <= 1 || !wasMessageReceived)
			{
				this.SetActiveTab(chatTabGuiItem);
			}
			this.RefreshActiveChatTabs();
			return chatTabGuiItem;
		}

		public void SetActiveTab(ChatTabGuiItem tabBaseGUI)
		{
			this.RefreshActiveChatTabs();
			if (tabBaseGUI.Index > this._chatTabNavigationIndex + this.remnantVisibleTabs)
			{
				this._chatTabNavigationIndex = tabBaseGUI.Index - this.remnantVisibleTabs;
			}
			else if (tabBaseGUI.Index < this._chatTabNavigationIndex)
			{
				this._chatTabNavigationIndex = tabBaseGUI.Index;
			}
			if (Mathf.Approximately(1f, base.Panel.alpha))
			{
				this.ChatUIInput.isSelected = true;
			}
			this.EnableChatGroups(true);
			this.PlaySocialSfx(this.sfx_ui_chat_click);
			if (tabBaseGUI == this._activeTabBaseGUI)
			{
				return;
			}
			if (this._activeTabBaseGUI != null)
			{
				this._activeTabBaseGUI.ClearIcons();
				this._activeTabBaseGUI.SetTabState(false, true);
				this._activeTabBaseGUI.MsgTextInput = this.ChatUIInput.value;
				this._previousTabBaseGUIStack.Push(this._activeTabBaseGUI);
				this._activeTabBaseGUI.transform.parent = this._chatTabListTable.transform;
				this._activeTabBaseGUI.gameObject.SetActive(false);
				this._activeTabBaseGUI.gameObject.SetActive(true);
				this._chatTabListTable.repositionNow = true;
			}
			tabBaseGUI.SetTabState(true, true);
			this._activeTabBaseGUI = tabBaseGUI;
			tabBaseGUI.transform.localPosition = Vector3.zero;
			tabBaseGUI.gameObject.SetActive(false);
			tabBaseGUI.gameObject.SetActive(true);
			this._chatLabel.text = tabBaseGUI.GetTabText();
			this._chatLabel.ProcessText();
			this.ChatUIInput.Set(tabBaseGUI.MsgTextInput ?? string.Empty, false);
			this.RefreshActiveChatTabs();
			this._uiScrollView.ResetPosition();
			this._activeTabBaseGUI.UpdateIcons();
		}

		public void TabClosed(ChatTabGuiItem tabBaseGUI)
		{
			tabBaseGUI.ClearIcons();
			this._openChatTabsDictionary.Remove(tabBaseGUI.ReferenceObject.UniversalId);
			this._activeTabsList.Remove(tabBaseGUI);
			this.RefreshActiveChatTabs();
			this.PlaySocialSfx(this.sfx_ui_chat_tab_close);
			if (tabBaseGUI == this._activeTabBaseGUI)
			{
				this._chatLabel.text = null;
				ChatTabGuiItem chatTabGuiItem = null;
				while (this._previousTabBaseGUIStack.Count > 0)
				{
					chatTabGuiItem = this._previousTabBaseGUIStack.Pop();
					if (!(chatTabGuiItem == tabBaseGUI) && !(chatTabGuiItem == null))
					{
						this.SetActiveTab(chatTabGuiItem);
						break;
					}
				}
				if (chatTabGuiItem == null)
				{
					this.EnableChatGroups(false);
					this._activeTabBaseGUI = null;
				}
			}
			this._chatTabListTable.repositionNow = true;
		}

		public void EnableChatGroups(bool haveAnyTabActive)
		{
			this._chatMessagesGroup.SetActive(haveAnyTabActive);
			this.ChatUIInput.enabled = haveAnyTabActive;
			this._chatUIInputLabel.enabled = haveAnyTabActive;
			if (!haveAnyTabActive)
			{
				this.ChatUIInput.value = string.Empty;
			}
		}

		public void OnGroupUpdated()
		{
			if (!SingletonMonoBehaviour<ManagerController>.DoesInstanceExist())
			{
				return;
			}
			GroupStatus selfGroupStatus = ManagerController.Get<GroupManager>().GetSelfGroupStatus();
			bool isUserInGroupOrPendingInvite = ManagerController.Get<GroupManager>().IsUserInGroupOrPendingInvite;
			this.PlaySocialSfx(this.sfx_ui_chat_invite_sent);
			if (isUserInGroupOrPendingInvite && selfGroupStatus != GroupStatus.Invited)
			{
				this.CreateGroupChatTab(ManagerController.Get<GroupManager>().GetCurrentGroupIfExists());
			}
		}

		public void onChatHyperlinkClick(string hyperlinkContent)
		{
			string[] array = hyperlinkContent.Split(new char[]
			{
				'|'
			});
			string a = array[0];
			if (string.Equals(a, SocialModalGUI.HyperlinkCategory.InviteSuggestion.ToString(), StringComparison.InvariantCultureIgnoreCase))
			{
				UserFriend userFriend = new UserFriend();
				userFriend.UniversalID = array[1];
				userFriend.PlayerName = array[2];
				ManagerController.Get<GroupManager>().TryInviteToGroup(userFriend);
				return;
			}
			if (string.Equals(a, SocialModalGUI.HyperlinkCategory.LobbyAccessCode.ToString(), StringComparison.InvariantCultureIgnoreCase))
			{
				ManagerController.Get<MatchManager>().CopyLobbyTokenToClipboard(array[1]);
				return;
			}
		}

		public void onGroupFriendSuggestion(string groupID, string suggestedByPlayerName, string suggestedUniversalID, string suggestedPlayerName, DateTime receivedTime)
		{
			string text = string.Format(Language.Get("SUGGEST_A_PLAYER", TranslationSheets.Help), suggestedByPlayerName, HudUtils.RGBToHex(SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.InviteEventPlayerNameColor), suggestedPlayerName);
			string textContent = text;
			float timeoutSeconds = 5f;
			BaseHintContent baseHintContent = new BaseHintContent(textContent, timeoutSeconds, true, null, groupID);
			SingletonMonoBehaviour<PanelController>.Instance.ShowMessageHint(baseHintContent, StackableHintKind.GroupSuggestion, HintColorScheme.System);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("[{0:00}:{1:00}] ", receivedTime.Hour, receivedTime.Minute);
			stringBuilder.AppendFormat(text, new object[0]);
			stringBuilder.AppendFormat(" [url={0}|{1}|{2}][{3}]>>>>>[u]{4}[/u][-][/url]\n", new object[]
			{
				SocialModalGUI.HyperlinkCategory.InviteSuggestion,
				suggestedUniversalID,
				suggestedPlayerName,
				HudUtils.RGBToHex(SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.InviteEventPlayerNameColor),
				Language.Get("SUGGEST_PLAYER_ACTION", TranslationSheets.Help)
			});
			if (!this._openChatTabsDictionary.ContainsKey(groupID))
			{
				UnityEngine.Debug.LogWarning(string.Format("[onGroupFriendSuggestion] Missing group chat tab for group: {0}, aborting!", groupID));
				return;
			}
			this._openChatTabsDictionary[groupID].AppendLineToTab(stringBuilder.ToString());
		}

		public void TryConvertRawMsgToLobbyCopyUrl(ref string rawMsg)
		{
			if (this._accessCodeRegex.IsMatch(rawMsg))
			{
				string replacement = string.Format("[url={0}|{1}][{2}][u]{1}[/u][-][/url] ({3})", new object[]
				{
					SocialModalGUI.HyperlinkCategory.LobbyAccessCode,
					this._accessCodeRegex.Match(rawMsg),
					HudUtils.RGBToHex(SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.InviteEventPlayerNameColor),
					this.GenerateLocalizedTag("CUSTOM_MATCH_COPY_ACCESS_CODE_CHAT_ACTION", TranslationSheets.MainMenuGui.ToString())
				});
				rawMsg = this._accessCodeRegex.Replace(rawMsg, replacement);
			}
		}

		private string GenerateLocalizedTag(string localizationKey, string localizationTab)
		{
			if (!string.IsNullOrEmpty(localizationKey) && !string.IsNullOrEmpty(localizationTab))
			{
				return string.Format("[localized:{0}|{1}]", localizationKey, localizationTab);
			}
			UnityEngine.Debug.LogError("[SocialModalGUI] - Invalid localization key / tab.");
			return string.Empty;
		}

		private void ReplaceLocalizedTags(ref string rawMsg)
		{
			if (string.IsNullOrEmpty(rawMsg))
			{
				return;
			}
			if (this._localizationTag == null)
			{
				this._localizationTag = new Regex("\\[localized:([^\\|]+)\\|([^\\]]+)\\]");
			}
			rawMsg = this._localizationTag.Replace(rawMsg, new MatchEvaluator(this.ReplaceLocalizedRegexEvaluator));
		}

		private string ReplaceLocalizedRegexEvaluator(Match m)
		{
			if (m.Groups.Count > 2)
			{
				return Language.Get(m.Groups[1].Value, m.Groups[2].Value);
			}
			return m.Value;
		}

		private void OnVoiceChatStatusChanged(VoiceChatTeamStatus obj)
		{
			if (ManagerController.Get<MatchManager>().IsUserInLobby)
			{
				this.ShowVoiceChatHint(ManagerController.Get<MatchManager>().CurrentLobbyToken);
				return;
			}
			this.ShowVoiceChatHint(ManagerController.Get<GroupManager>().CurrentGroupID.ToString());
		}

		public void ShowVoiceChatHint(string ownerId)
		{
			this.PlaySocialSfx(this.sfx_ui_voicechat_notification);
			HMMHub hub = GameHubBehaviour.Hub;
			string textlocalized;
			if (ControlOptions.IsUsingControllerJoystick(hub))
			{
				textlocalized = ControlOptions.GetTextlocalized(ControlAction.PushToTalk, ControlOptions.ControlActionInputType.Secondary);
			}
			else
			{
				textlocalized = ControlOptions.GetTextlocalized(ControlAction.PushToTalk, ControlOptions.ControlActionInputType.Primary);
			}
			string key;
			if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatTeamStatus == VoiceChatTeamStatus.Disable)
			{
				key = "GROUP_VOICE_CHAT_DISABLED_NOTIFICATION";
			}
			else if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType == VoiceChatInputType.AlwaysActive)
			{
				key = "GROUP_VOICE_CHAT_ALWAYS_ON_NOTIFICATION";
			}
			else if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType == VoiceChatInputType.Toggle)
			{
				key = "GROUP_VOICE_CHAT_TOGGLE_NOTIFICATION";
			}
			else
			{
				key = "GROUP_VOICE_CHAT_NOTIFICATION";
			}
			string text = string.Format(Language.Get(key, TranslationSheets.HUDChat), textlocalized);
			string textContent = Language.Get("HINT_VOICE_CHAT_NOTIFICATION_TOP", TranslationSheets.HUDChat);
			string text2 = string.Empty;
			VoiceChatInputType voiceChatInputType = SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatInputType;
			if (voiceChatInputType != VoiceChatInputType.Pressed)
			{
				if (voiceChatInputType == VoiceChatInputType.Toggle)
				{
					text2 = string.Format(Language.Get("VOICE_CHAT_NOTIFICATION_KEY", TranslationSheets.HUDChat), textlocalized);
				}
			}
			else
			{
				text2 = string.Format(Language.Get("VOICE_CHAT_NOTIFICATION_KEY", TranslationSheets.HUDChat), textlocalized);
				if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatTeamStatus == VoiceChatTeamStatus.Enable)
				{
					text = string.Format("{0} {1}", text, text2);
				}
			}
			if (!string.IsNullOrEmpty(ownerId) && !ownerId.Equals(Guid.Empty.ToString()))
			{
				SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(text, ownerId, true, true, StackableHintKind.None, HintColorScheme.Group);
			}
			if (SingletonMonoBehaviour<VoiceChatController>.Instance.VoiceChatTeamStatus == VoiceChatTeamStatus.Disable)
			{
				return;
			}
			BaseHintContent baseHintContent = new BaseHintContent(textContent, text2, 5f, false, this.voicechatSprite, ownerId);
			SingletonMonoBehaviour<PanelController>.Instance.ShowMessageHint(baseHintContent, StackableHintKind.None, HintColorScheme.Group);
		}

		public override bool IsStackableWithType(Type type)
		{
			return type != typeof(DriverHelperController) && type != typeof(StoreConfirmationWindow);
		}

		public override void ChangeWindowVisibility(bool targetVisibleState)
		{
			base.ChangeWindowVisibility(targetVisibleState);
			this._unityUiColliderGameObject.SetActive(targetVisibleState);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SocialModalGUI));

		public static SocialModalGUI Current;

		[Header("Limit chat characters so that NGUI doesn't bug out and screw everything(max vertices per mesh = 65000")]
		public const int MaxChatTabCharacterCount = 6000;

		[Header("Friend List")]
		[SerializeField]
		private UITable _playerListTable;

		[Header("HMM Filters")]
		[SerializeField]
		private FriendFilterGuiItem _hmmMainFilterGuiItem;

		[SerializeField]
		private FriendFilterGuiItem _playingMatchFilterGuiItem;

		[SerializeField]
		private FriendFilterGuiItem _hmmOnlineFilterGuiItem;

		[Header("Steam Filters")]
		[SerializeField]
		private FriendFilterGuiItem _steamMainFilterGuiItem;

		[SerializeField]
		private FriendFilterGuiItem _steamOtherGameFilterGuiItem;

		[SerializeField]
		private FriendFilterGuiItem _steamAwayFilterGuiItem;

		[SerializeField]
		private FriendFilterGuiItem _steamOnlineFilterGuiItem;

		[SerializeField]
		private FriendFilterGuiItem _offlineFilterGuiItem;

		[SerializeField]
		private FriendGuiItem _baseFriendGuiItem;

		[Header("Chat")]
		[SerializeField]
		private UITable _chatTabListTable;

		[SerializeField]
		private ChatTabGuiItem _baseChatTabGuiItem;

		[SerializeField]
		private UIScrollView _chatInboxUIScrollView;

		[SerializeField]
		private UILabelHyperlinkScript _uiLabelHyperlinkScript;

		public ChatFilter ChatFilter;

		[Header("Audio")]
		public FMODAsset FeedBackAudioFx;

		public FMODAsset sfx_ui_chat_open;

		public FMODAsset sfx_ui_chat_close;

		public FMODAsset sfx_ui_chat_tab_close;

		public FMODAsset sfx_ui_chat_tab_new;

		public FMODAsset sfx_ui_chat_click;

		public FMODAsset sfx_ui_chat_filter_minimize;

		public FMODAsset sfx_ui_chat_filter_maximize;

		public FMODAsset sfx_ui_chat_group_create;

		public FMODAsset sfx_ui_chat_message_receive;

		public FMODAsset sfx_ui_chat_message_sent;

		public FMODAsset sfx_ui_chat_invite_receive;

		public FMODAsset sfx_ui_chat_invite_sent;

		public FMODAsset sfx_ui_chat_member_offline;

		[SerializeField]
		private GameObject _chatMessagesGroup;

		[SerializeField]
		public UIInput ChatUIInput;

		[SerializeField]
		private UI2DSprite ChatInputEffect;

		[SerializeField]
		private UILabel _chatUIInputLabel;

		[SerializeField]
		private UILabel _chatLabel;

		[SerializeField]
		private UIScrollView _uiScrollView;

		[SerializeField]
		private UIButton _nextChatTabButtonGameobject;

		[SerializeField]
		private UIButton _previousChatTabButtonGameobject;

		[SerializeField]
		private GameObject _nextChatTabEffect;

		[SerializeField]
		private GameObject _previousChatTabEffect;

		[SerializeField]
		private GameObject _unityUiColliderGameObject;

		private ChatTabGuiItem _activeTabBaseGUI;

		private ChatTabGuiItem _innactiveTabBaseGUI;

		private Stack<ChatTabGuiItem> _previousTabBaseGUIStack = new Stack<ChatTabGuiItem>();

		private List<ChatTabGuiItem> _activeTabsList = new List<ChatTabGuiItem>();

		private int _chatTabNavigationIndex;

		private Regex _accessCodeRegex;

		private Regex _reversingAccessCodeRegex;

		[SerializeField]
		private UIScrollView _chatTabScrollView;

		private const int MaxPlayerShortNameCharCount = 12;

		private Dictionary<string, UserFriend> _unknownUsers = new Dictionary<string, UserFriend>();

		private const int maxActiveChatTabs = 3;

		private readonly Dictionary<string, FriendGuiItem> _friendGuiItemByIDDictionary = new Dictionary<string, FriendGuiItem>();

		private UserFriend m_systemMockUser;

		private Dictionary<string, ChatTabGuiItem> _openChatTabsDictionary = new Dictionary<string, ChatTabGuiItem>();

		private int remnantVisibleTabs = 3;

		private Regex _localizationTag;

		public Sprite voicechatSprite;

		public FMODAsset sfx_ui_voicechat_notification;

		private enum HyperlinkCategory
		{
			InviteSuggestion,
			LobbyAccessCode
		}
	}
}
