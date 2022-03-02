using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Chat;
using ClientAPI.Objects;
using ClientAPI.Objects.Partial;
using FMod;
using HeavyMetalMachines.Chat;
using HeavyMetalMachines.Chat.Business;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Crossplay.DataTransferObjects;
using HeavyMetalMachines.DriverHelper;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.Social.Friends.Business;
using HeavyMetalMachines.Social.Friends.Models;
using HeavyMetalMachines.Social.Friends.Presenting.FriendsList;
using HeavyMetalMachines.Social.Groups.Business;
using HeavyMetalMachines.Social.Groups.Models;
using HeavyMetalMachines.VFX.PlotKids;
using HeavyMetalMachines.VoiceChat.Business;
using Hoplon.Assertions;
using Hoplon.Input;
using Hoplon.Input.Infra;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using Hoplon.Input.UiNavigation.ContextInputNotifier;
using Hoplon.Input.UiNavigation.InputDeliverer;
using Hoplon.Input.UiNavigation.ToggleSelector;
using Hoplon.Localization.TranslationTable;
using Hoplon.Serialization;
using Pocketverse;
using Standard_Assets.Scripts.HMM.Util;
using UniRx;
using UnityEngine;
using Zenject;

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
			if (this._playerListTable != null)
			{
				this._playerListTable.repositionNow = true;
			}
		}

		public bool IsOnUiNavigationFocus { get; private set; }

		public bool IsChatTabOpen
		{
			get
			{
				return this._activeTabsList.Count > 0;
			}
		}

		protected override void InitDialogTasks()
		{
			ITextChatRestriction textChatRestriction = this._diContainer.Resolve<ITextChatRestriction>();
			if (textChatRestriction.IsGloballyEnabled())
			{
				this.ChatUIInput.gameObject.SetActive(false);
			}
			bool flag = this._openChatTabsDictionary.Count > 0;
			this._chatMessagesGroup.SetActive(flag);
			this.ChatUIInput.enabled = flag;
			this._chatUIInputLabel = this.ChatUIInput.GetComponent<UILabel>();
			this._chatUIInputLabel.enabled = flag;
			if (this._activeTabBaseGUI != null)
			{
				SingletonMonoBehaviour<SocialController>.Instance.ChatUiFeedbackDispatcher.ClearPendingChatMessagesFromUID(this._activeTabBaseGUI.ReferenceObject.UniversalId);
			}
			this.TryToSelectChatInput();
			if (this._playerListTable != null)
			{
				this._playerListTable.repositionNow = true;
			}
			this._chatTabListTable.repositionNow = true;
		}

		private void TryToSelectChatInput()
		{
			if (this._activeTabsList.Count > 0 && this._activeDevice.GetActiveDevice() != 3)
			{
				UICamera.selectedObject = this.ChatUIInput.gameObject;
			}
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
			this._getPublisherUserName = this._diContainer.Resolve<IGetPublisherUserName>();
			this._getDisplayableNickName = this._diContainer.Resolve<IGetDisplayableNickName>();
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
			if (!this._diContainer.Resolve<IFriendsListPresenter>().IsInitialized)
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
			CompositeDisposable compositeDisposable = new CompositeDisposable();
			compositeDisposable.Add(ObservableExtensions.Subscribe<IChatMessageRequest>(Observable.Do<IChatMessageRequest>(this._observeChatMessageRequests.Observe(), new Action<IChatMessageRequest>(this.HandleChatMessageRequest))));
			compositeDisposable.Add(ObservableExtensions.Subscribe<Group>(Observable.Do<Group>(this._observeChatGroupMessageRequest.Observe(), new Action<Group>(this.HandleGroupMessageRequest))));
			compositeDisposable.Add(ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(this._chatGroupUiNavigationFocusRequest.Observe(), new Action<bool>(this.HandleGetUiNavigationFocus))));
			compositeDisposable.Add(ObservableExtensions.Subscribe<UiNavigationInputCode>(Observable.Do<UiNavigationInputCode>(Observable.Do<UiNavigationInputCode>(Observable.Do<UiNavigationInputCode>(this._uiNavigationContextInputNotifier.ObserveInputDown(), new Action<UiNavigationInputCode>(this.OnTryNavigateToRight)), new Action<UiNavigationInputCode>(this.TryRemoveNavigationFocus)), new Action<UiNavigationInputCode>(this.TryCloseCurrentTab))));
			compositeDisposable.Add(ObservableExtensions.Subscribe<AxisSelectorEdge>(Observable.Do<AxisSelectorEdge>(this._uiNavigationToggleSelector.ObserveOnEdgeReached(), new Action<AxisSelectorEdge>(this.OnEdgeReach))));
			compositeDisposable.Add(ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(Observable.Where<bool>(this.ChatUIInput.ObserveChatGetSelectOrUnselected(), (bool active) => !active), delegate(bool active)
			{
				this.SetShortcutActive(!active);
			})));
			compositeDisposable.Add(ObservableExtensions.Subscribe<string>(Observable.Do<string>(this.ChatUIInput.ObserveVirtualKeyboardClose(), delegate(string _)
			{
				this.onSubmit_SendMessageToFriend();
			})));
			compositeDisposable.Add(ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), new Action<InputDevice>(this.OnActiveDeviceChange))));
			compositeDisposable.Add(ObservableExtensions.Subscribe<Unit>(this.ReceiveFriendSuggestion()));
			this._disposables = compositeDisposable;
			ManagerController.Get<ChatManager>().EvtMessageReceived += this.OnMessageReceived;
			ManagerController.Get<ChatManager>().EvtGroupMessageReceived += this.onGroupMessageReceived;
			ManagerController.Get<GroupManager>().OnGroupUpdate += this.OnGroupUpdated;
			ManagerController.Get<MatchManager>().EvtLobbyJoined += this.OnLobbyJoined;
			PanelController.EvtSystemMessage += this.EvtSystemMessage;
			ManagerController.Get<GroupManager>().EvtGroupMemberJoin += this.OnGroupMemberJoin;
			ManagerController.Get<GroupManager>().EvtGroupCreated += this.OnGroupCreated;
			ManagerController.Get<GroupManager>().EvtInviteHandled += this.OnInviteHandled;
			this._voiceChatPreferences.OnTeamStatusChanged += this.OnVoiceChatStatusChanged;
			this._uiLabelHyperlinkScript.ActionOnHyperlink = new Action<string>(this.onChatHyperlinkClick);
			HudWindowManager.Instance.OnNewWindowAdded += this.OnNewWindowAdded;
			ConfirmWindowReference.EvtConfirmWindowOpened += this.ClearCameraUISelectionIfChatInputSelected;
		}

		private IObservable<Unit> ReceiveFriendSuggestion()
		{
			GroupManager groupManager = ManagerController.Get<GroupManager>();
			return Observable.AsUnitObservable<SocialModalGUI.GroupFriendSuggestionDataTransferObject>(Observable.Do<SocialModalGUI.GroupFriendSuggestionDataTransferObject>(Observable.SelectMany<SocialModalGUI.GroupFriendSuggestionDataTransferObject, SocialModalGUI.GroupFriendSuggestionDataTransferObject>(Observable.Select<GroupFriendSuggestion, SocialModalGUI.GroupFriendSuggestionDataTransferObject>(Observable.FromEvent<GroupFriendSuggestion>(delegate(Action<GroupFriendSuggestion> handler)
			{
				groupManager.EvtGroupFriendSuggestion += handler;
			}, delegate(Action<GroupFriendSuggestion> handler)
			{
				groupManager.EvtGroupFriendSuggestion -= handler;
			}), (GroupFriendSuggestion suggestion) => new SocialModalGUI.GroupFriendSuggestionDataTransferObject
			{
				GroupId = suggestion.GroupId,
				SuggestedByPlayerName = suggestion.SuggestedByPlayerName,
				SuggestedUniversalId = suggestion.SuggestedUniversalId,
				SuggestedPlayerNameUnformatted = suggestion.SuggestedPlayerNameUnformatted,
				SuggestedPlayerId = suggestion.SuggestedPlayerId,
				ReceivedTime = suggestion.ReceivedTime,
				DisplayableNicknameParameters = new DisplayableNicknameParameters
				{
					PlayerId = suggestion.SuggestedPlayerId,
					PlayerName = suggestion.SuggestedPlayerNameUnformatted,
					UniversalId = suggestion.SuggestedUniversalId
				}
			}), (SocialModalGUI.GroupFriendSuggestionDataTransferObject request) => Observable.Select<string, SocialModalGUI.GroupFriendSuggestionDataTransferObject>(this._diContainer.Resolve<IGetDisplayableNickName>().GetLatestFormattedNickName(request.DisplayableNicknameParameters), delegate(string latestNickname)
			{
				request.LatestSuggestedPlayerName = latestNickname;
				return request;
			})), new Action<SocialModalGUI.GroupFriendSuggestionDataTransferObject>(this.OnGroupFriendSuggestion)));
		}

		private void OnActiveDeviceChange(InputDevice inputDevice)
		{
			if (inputDevice == 3)
			{
				this.ChangeCodeText("CUSTOM_MATCH_COPY_ACCESS_CODE_CHAT_ACTION", this._customMatchCopyAccessCodeChatActionOnGamepad.CurrentPlatformDraft, TranslationContext.CustomMatch);
				this.ChangeCodeText("SUGGEST_PLAYER_ACTION", "SUGGEST_PLAYER_ACTION_GAMEPAD", TranslationContext.Help);
				return;
			}
			this.ChangeCodeText(this._customMatchCopyAccessCodeChatActionOnGamepad.CurrentPlatformDraft, "CUSTOM_MATCH_COPY_ACCESS_CODE_CHAT_ACTION", TranslationContext.CustomMatch);
			this.ChangeCodeText("SUGGEST_PLAYER_ACTION_GAMEPAD", "SUGGEST_PLAYER_ACTION", TranslationContext.Help);
		}

		private void HandleGetUiNavigationFocus(bool focus)
		{
			if (focus)
			{
				this.AddUiNavigationFocus();
				return;
			}
			this.RemoveUiNavigationFocus();
		}

		private void AddUiNavigationFocus()
		{
			this._uiNavigationGroupHolder.AddHighPriorityGroup();
			this.SetActiveUiNavigationHighlightGroup(true);
			this.IsOnUiNavigationFocus = true;
			this.SetFocusOnActiveChatTab(true);
		}

		private void SetFocusOnActiveChatTab(bool focus)
		{
			if (this._activeTabBaseGUI != null)
			{
				this._activeTabBaseGUI.SetUiNavigationFocus(focus);
			}
		}

		private void RemoveUiNavigationFocus()
		{
			this.SetActiveUiNavigationHighlightGroup(false);
			this._uiNavigationGroupHolder.RemoveHighPriorityGroup();
			this.IsOnUiNavigationFocus = false;
			this.SetFocusOnActiveChatTab(false);
		}

		private void SetActiveUiNavigationHighlightGroup(bool active)
		{
			this._uiNavigationHighlightBorder.SetActive(active);
			this.SetShortcutActive(active);
		}

		private void OnTryNavigateToRight(UiNavigationInputCode edge)
		{
			if (edge == 15 || edge == 21)
			{
				this.SetFocusBackToFriendList();
			}
		}

		private void TryRemoveNavigationFocus(UiNavigationInputCode inputCode)
		{
			if (!this.IsOnUiNavigationFocus)
			{
				return;
			}
			if (inputCode == 1)
			{
				this.SetFocusBackToFriendList();
			}
		}

		private void SetFocusBackToFriendList()
		{
			this._diContainer.Resolve<IFriendsListPresenter>().SetUiNavigationFocus();
			this.RemoveUiNavigationFocus();
		}

		private void TryCloseCurrentTab(UiNavigationInputCode input)
		{
			if (input == 2)
			{
				if (this._openChatTabsDictionary.Count > 0)
				{
					this._activeTabBaseGUI.CloseTab();
				}
				else
				{
					this.SetFocusBackToFriendList();
				}
			}
		}

		private void SetShortcutActive(bool active)
		{
			if (this._gamepadCancelFocusShortcutImage == null)
			{
				return;
			}
			bool flag = this._activeDevice.GetActiveDevice() == 3;
			if (flag)
			{
				this.TryToSetupShortcutIcon();
			}
			this._gamepadCancelFocusShortcutImage.gameObject.SetActive(active && flag);
		}

		private void ChangeCodeText(string from, string to, ContextTag translationContext)
		{
			string text = this._chatLabel.text;
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			string formatted = this._localizeKey.GetFormatted(from, translationContext, new object[0]);
			string joystickCodeNameTranslation = this._inputTranslationProvider.GetJoystickCodeNameTranslation(10);
			string formatted2 = this._localizeKey.GetFormatted(to, translationContext, new object[]
			{
				joystickCodeNameTranslation
			});
			Regex regex = new Regex(formatted);
			this._chatLabel.text = regex.Replace(text, formatted2);
			this._chatLabel.ProcessText();
		}

		private void TryToSetupShortcutIcon()
		{
			if (this._gamepadCancelFocusShortcutImage == null)
			{
				return;
			}
			int num = 33;
			ISprite sprite;
			string text;
			this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(num, ref sprite, ref text);
			this._gamepadCancelFocusShortcutImage.sprite2D = (sprite as UnitySprite).GetSprite();
		}

		private void OnEdgeReach(AxisSelectorEdge edge)
		{
			if (edge == 3)
			{
				this.onButtonPress_NextChatTab();
				return;
			}
			if (edge == 2)
			{
				this.onButtonPress_PreviousChatTab();
			}
		}

		private void HandleChatMessageRequest(IChatMessageRequest chatMessageRequest)
		{
			FriendsChatMessageRequest friendsChatMessageRequest = chatMessageRequest as FriendsChatMessageRequest;
			this.CreatePlayerChatTab(friendsChatMessageRequest.Player, false, false);
		}

		private void HandleGroupMessageRequest(Group group)
		{
			this.CreateGroupChatTab(group);
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
				ManagerController.Get<ChatManager>().EvtMessageReceived -= this.OnMessageReceived;
				ManagerController.Get<ChatManager>().EvtGroupMessageReceived -= this.onGroupMessageReceived;
				ManagerController.Get<GroupManager>().OnGroupUpdate -= this.OnGroupUpdated;
				ManagerController.Get<GroupManager>().EvtGroupMemberJoin -= this.OnGroupMemberJoin;
				ManagerController.Get<GroupManager>().EvtGroupCreated -= this.OnGroupCreated;
				ManagerController.Get<GroupManager>().EvtInviteHandled -= this.OnInviteHandled;
				ManagerController.Get<MatchManager>().EvtLobbyJoined -= this.OnLobbyJoined;
			}
			this._disposables.Dispose();
			PanelController.EvtSystemMessage -= this.EvtSystemMessage;
			this._voiceChatPreferences.OnTeamStatusChanged -= this.OnVoiceChatStatusChanged;
			if (HudWindowManager.DoesInstanceExist())
			{
				HudWindowManager.Instance.OnNewWindowAdded -= this.OnNewWindowAdded;
			}
			ConfirmWindowReference.EvtConfirmWindowOpened -= this.ClearCameraUISelectionIfChatInputSelected;
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
			this.FetchGroupMemberParentalControlInfo(newGroupMember);
			this.ShowVoiceChatHint(currentGroupID.ToString());
		}

		private void FetchGroupMemberParentalControlInfo(GroupMember newGroupMember)
		{
			PlayerIdentification playerIdentification = new PlayerIdentification
			{
				PlayerId = newGroupMember.PlayerId,
				UniversalId = newGroupMember.UniversalID
			};
			IFetchAndStorePlayersParentalControlInfo fetchAndStorePlayersParentalControlInfo = this._diContainer.Resolve<IFetchAndStorePlayersParentalControlInfo>();
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(fetchAndStorePlayersParentalControlInfo.FetchAndStore(new PlayerIdentification[]
			{
				playerIdentification
			})), this);
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

		private void UpdateGamepadCopyCode()
		{
			if (this._inputActionPoller.GetButtonDown(41))
			{
				if (!this.IsWindowVisible())
				{
					return;
				}
				this.CopyCurrentTabLastLobbyTokenToClipboard();
			}
		}

		private void UpdateInviteGroupSuggestionOnGamepad()
		{
			if (this._inputActionPoller.GetButtonDown(52))
			{
				if (!this.IsWindowVisible())
				{
					return;
				}
				this.TryInviteSuggestedFriendWithGamepad();
			}
		}

		private void TryInviteSuggestedFriendWithGamepad()
		{
			if (string.IsNullOrEmpty(this._groupSuggestionUrlString))
			{
				return;
			}
			Guid currentGroupID = ManagerController.Get<GroupManager>().CurrentGroupID;
			if (currentGroupID == Guid.Empty)
			{
				return;
			}
			ChatTabGuiItem chatTabGuiItem;
			if (!this._openChatTabsDictionary.TryGetValue(currentGroupID.ToString(), out chatTabGuiItem))
			{
				return;
			}
			if (this._activeTabBaseGUI == chatTabGuiItem)
			{
				this.TryInviteFriendSuggestionToGroup(SocialModalGUI.GetHyperlinkSplited(this._groupSuggestionUrlString));
			}
		}

		private static string[] GetHyperlinkSplited(string hyperlink)
		{
			return hyperlink.Split(new char[]
			{
				'|'
			});
		}

		private void CopyCurrentTabLastLobbyTokenToClipboard()
		{
			string text = this._chatLabel.text;
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			MatchCollection matchCollection = this._accessCodeRegex.Matches(text);
			if (matchCollection.Count > 0)
			{
				Match match = matchCollection[matchCollection.Count - 1];
				ManagerController.Get<MatchManager>().CopyLobbyTokenToClipboard(match.Value);
			}
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
			if (!this._inputActionPoller.GetButtonUp(19))
			{
				return;
			}
			if (this._hasHighPriorityGroupFocused.HasHighPriorityGroupFocused() && !this.IsWindowVisible())
			{
				return;
			}
			HoplonUiUtils.EventSystemCancelSelection();
			SingletonMonoBehaviour<PanelController>.Instance.ToggleModalWindow<SocialModalGUI>();
			this.PlaySocialSfx(this.sfx_ui_chat_open);
		}

		protected override void Update()
		{
			this.UpdateScreenState();
			this.UpdateChatInputFeedback();
			this.UpdateGamepadCopyCode();
			this.UpdateInviteGroupSuggestionOnGamepad();
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
			Debug.LogWarning("NOT IMPLEMENTED");
		}

		private string GetShortMsgSenderName(string baseName)
		{
			if (baseName.Length <= 12)
			{
				return baseName;
			}
			return string.Format("{0}...", baseName.Substring(0, 9));
		}

		private void PlaySocialSfx(AudioEventAsset fmodAsset)
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
			ChatMessageBag bag = JsonSerializeable<ChatMessageBag>.Deserialize(chatMessage.Bag);
			this.ReplaceLocalizedTags(ref message);
			this._uiScrollView.ResetPosition();
			string fromUniversalId = chatMessage.FromUniversalId;
			string text = chatMessage.PlayerName;
			this.PlaySocialSfx(this.sfx_ui_chat_message_receive);
			bool flag = this._isPlayerLocalPlayerFriend.IsFriend(fromUniversalId);
			IGetFriends getFriends = this._diContainer.Resolve<IGetFriends>();
			Friend friendByUniversalId = getFriends.GetFriendByUniversalId(fromUniversalId);
			UserFriend userFriend = SocialModalGUI.CreateUserFriendFromPlayer(friendByUniversalId);
			long num = -1L;
			IUser user = userFriend;
			if (userFriend != null)
			{
				text = userFriend.PlayerName;
				num = userFriend.PlayerId;
			}
			if (flag && (this._activeTabBaseGUI == null || !string.Equals(fromUniversalId, this._activeTabBaseGUI.ReferenceObject.UniversalId) || !SocialModalGUI.IsWindowOpened))
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
				text = this._diContainer.Resolve<IGetDisplayableNickName>().GetFormattedNickName(num, text);
				string text2 = string.Format("[{0}]{1}[-]: {2}[-]", HudUtils.RGBToHex(SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.FriendTextColor), text, arg);
				string textContent = text2;
				float timeoutSeconds = 5f;
				string ownerId = fromUniversalId;
				BaseHintContent baseHintContent = new BaseHintContent(textContent, timeoutSeconds, true, null, ownerId);
				SingletonMonoBehaviour<PanelController>.Instance.ShowMessageHint(baseHintContent, StackableHintKind.NewChatMessage, HintColorScheme.Chat);
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
					Debug.LogWarningFormat("[onMessageReceived] Aborting: Couldn't get sender user from Universal ID: \"{0}\". Message: {1}. Creating mock unknown user!", new object[]
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
			this.AddFormattedMessageToTab(MessageKind.Friend, message, text, user.UniversalID, chatMessage.FromUniversalId, chatMessage.SenderPlayerId, bag);
			this.RefreshActiveChatTabs();
			this.PlaySocialSfx(this.sfx_ui_chat_invite_receive);
		}

		private static UserFriend CreateUserFriendFromPlayer(IPlayer player)
		{
			if (player == null)
			{
				return null;
			}
			return new UserFriend
			{
				PlayerId = player.PlayerId,
				PlayerName = player.Nickname,
				UniversalId = player.UniversalId,
				UniversalID = player.UniversalId
			};
		}

		private string ReversingAccessCodeRegexEvaluator(Match m)
		{
			if (m.Groups.Count > 1)
			{
				return m.Groups[1].Value;
			}
			return m.Value;
		}

		private void onGroupMessageReceived(string chatMessage, string bag, string groupID, string playerName, string fromUniversalId, long playerId, DateTime receivedtime)
		{
			if (!this._openChatTabsDictionary.ContainsKey(groupID))
			{
				Debug.LogWarning(string.Format("Missing group chat tab for group: {0}, aborting!", groupID));
				return;
			}
			this.ReplaceLocalizedTags(ref chatMessage);
			ChatMessageBag bag2 = JsonSerializeable<ChatMessageBag>.Deserialize(bag);
			this.AddFormattedMessageToTab(MessageKind.Group, chatMessage, playerName, fromUniversalId, groupID, playerId, bag2);
			this.PlaySocialSfx(this.sfx_ui_chat_message_receive);
			this.RefreshActiveChatTabs();
			this.PlaySocialSfx(this.FeedBackAudioFx);
			this._uiScrollView.ResetPosition();
		}

		private string GetPublisherUserName(string name, string onlineId)
		{
			return string.Format("{0} ({1})", name, onlineId);
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
			this.RebuildNavigation();
		}

		public void onButtonPress_NextChatTab()
		{
			this._chatTabNavigationIndex++;
			this.RefreshActiveChatTabs();
			this.PlaySocialSfx(this.sfx_ui_chat_click);
			this.RebuildNavigation();
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
			ChatOwnerContent chatTabOwner = this._activeTabBaseGUI.ReferenceObject;
			if (!chatTabOwner.CanReceiveMessages)
			{
				string text2 = string.Format("[{0}]{1}[-]", HudUtils.RGBToHex(SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OfflinePlayerNameColor), chatTabOwner.OwnerName);
				this.AddSystemMessageToActiveTab(Language.GetFormatted("GROUP_OTHERUSER_OFFLINE", TranslationContext.Help, new object[]
				{
					text2
				}), null);
				return;
			}
			if (this.ShouldShowPublisherUserName())
			{
				ObservableExtensions.Subscribe<ChatMessageBag>(Observable.Do<ChatMessageBag>(Observable.Select<string, ChatMessageBag>(this._getPublisherUserName.Get(this._localPlayer.Player.UniversalId), (string onlineId) => new ChatMessageBag
				{
					PublisherId = this._getCurrentPublisher.Get().Id,
					PublisherUserName = onlineId
				}), delegate(ChatMessageBag msgBag)
				{
					this.TabSendMessage(chatTabOwner, rawMessage, msgBag);
				}));
				return;
			}
			ChatMessageBag bag = new ChatMessageBag
			{
				PublisherId = this._getCurrentPublisher.Get().Id
			};
			this.TabSendMessage(chatTabOwner, rawMessage, bag);
		}

		private void TabSendMessage(ChatOwnerContent owner, string message, ChatMessageBag bag)
		{
			bool flag = owner.TrySendMessage(message, bag.Serialize());
			SocialModalGUI.Log.DebugFormat("Sent message={0} bag={1}", new object[]
			{
				flag,
				bag.Serialize()
			});
			if (!flag)
			{
				return;
			}
			bool flag2 = owner.ChatKind == ChatOwnerContent.ChatContentKind.User;
			UserInfo user = GameHubBehaviour.Hub.User;
			this.ReplaceLocalizedTags(ref message);
			this.AddFormattedMessageToTab((!flag2) ? MessageKind.SelfOnGroup : MessageKind.SelfPrivate, message, user.PlayerSF.Name, user.UniversalId, owner.UniversalId, user.PlayerSF.Id, bag);
		}

		private bool ShouldShowPublisherUserName()
		{
			return this._getPublisherPresentingData.Get(this._getCurrentPublisher.Get()).ShouldShowPublisherUserName;
		}

		private void AddSystemMessageToActiveTab(string message, string ownerId = null)
		{
			this.AddFormattedMessageToTab(MessageKind.System, message, Language.Get("SYSTEM_TITLE", TranslationContext.Help), null, ownerId, -1L, null);
			this.RefreshActiveChatTabs();
		}

		private void TryHighlightChatTabIfNotCurrent(string ownerId)
		{
			ChatTabGuiItem chatTabGuiItem = null;
			if (!this._openChatTabsDictionary.TryGetValue(ownerId, out chatTabGuiItem))
			{
				Debug.LogErrorFormat("[SocialModalGUI] Trying to highlight tab that doesn't exist! OwnerId: {0}", new object[]
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

		private void AddFormattedMessageToTab(MessageKind msgKind, string rawMessage, string senderName, string senderId, string ownerId, long senderPlayerId, ChatMessageBag bag)
		{
			if (msgKind != MessageKind.System && string.IsNullOrEmpty(ownerId))
			{
				Debug.LogWarning(string.Format("Received non-system message without User UID assigned: {0}. Fixing message kind to \"System\"!", ownerId));
				msgKind = MessageKind.System;
			}
			DateTime now = DateTime.Now;
			ChatTabGuiItem chatTabGuiItem = null;
			Color color = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.SystemMessageColor;
			switch (msgKind)
			{
			case MessageKind.SelfPrivate:
				chatTabGuiItem = this._activeTabBaseGUI;
				color = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OwnPlayerNameAndTimeColorPrivateChat;
				break;
			case MessageKind.SelfOnGroup:
				chatTabGuiItem = this._activeTabBaseGUI;
				color = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OwnPlayerNameAndTimeColorGroupChat;
				break;
			case MessageKind.Friend:
				chatTabGuiItem = this._openChatTabsDictionary[ownerId];
				color = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OthersPlayersNameAndTimeColorPrivateChat;
				this.TryHighlightChatTabIfNotCurrent(ownerId);
				break;
			case MessageKind.Group:
				chatTabGuiItem = this._openChatTabsDictionary[ownerId];
				color = SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.OthersPlayersNameAndTimeColorGroupChat;
				this.TryHighlightChatTabIfNotCurrent(ownerId);
				break;
			default:
			{
				if (!string.IsNullOrEmpty(ownerId))
				{
					this._openChatTabsDictionary.TryGetValue(ownerId, out chatTabGuiItem);
				}
				if (chatTabGuiItem == null)
				{
					if (!string.Equals(ownerId, "SystemMessage"))
					{
						return;
					}
					chatTabGuiItem = this.CreateUserChatTab(this._systemMockUser, false, true);
					Debug.Log(string.Format("Creating systemTAB. [{0}] {1}", msgKind, rawMessage));
				}
				string message = string.Format("[{0}][{1:00}:{2:00}]: {3}", new object[]
				{
					HudUtils.RGBToHex(color),
					now.Hour,
					now.Minute,
					rawMessage
				});
				chatTabGuiItem.AppendLineToTab(message);
				return;
			}
			}
			string text = this.GetShortMsgSenderName(NGUIText.EscapeSymbols(senderName));
			text = this.GetDisplayableShortSenderName(senderPlayerId, bag, text);
			string message2 = string.Format("[{0}][{1:00}:{2:00}] {3}: {4}", new object[]
			{
				HudUtils.RGBToHex(color),
				now.Hour,
				now.Minute,
				text,
				rawMessage
			});
			chatTabGuiItem.AppendLineToTab(message2);
		}

		private string GetDisplayableShortSenderName(long senderPlayerId, ChatMessageBag bag, string shortSenderName)
		{
			shortSenderName = this._diContainer.Resolve<IGetDisplayableNickName>().GetFormattedNickName(senderPlayerId, shortSenderName);
			if (bag != null)
			{
				Publisher publisherById = Publishers.GetPublisherById(bag.PublisherId);
				PublisherPresentingData publisherPresentingData = this._diContainer.Resolve<IGetPublisherPresentingData>().Get(publisherById);
				if (publisherPresentingData.ShouldShowPublisherUserName)
				{
					shortSenderName = this.GetPublisherUserName(shortSenderName, bag.PublisherUserName);
				}
			}
			return shortSenderName;
		}

		[Conditional("AllowHacks")]
		private void LogFormattedMessageToTab(MessageKind msgKind, string rawMessage, string senderName, string senderId, string ownerId, long senderPlayerId, ChatMessageBag bag)
		{
			SocialModalGUI.Log.DebugFormat("LogFormattedMessageToTab. msgKind={0}, rawMessage={1}, senderName={2}, senderId={3}, ownerId={4}, senderPlayerId={5}, bag={6}", new object[]
			{
				msgKind.ToString(),
				rawMessage,
				senderName,
				senderId,
				ownerId,
				senderPlayerId,
				(bag == null) ? "null" : bag.Serialize()
			});
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
				this.m_systemMockUser.PlayerName = Language.Get("SYSTEM_TITLE", TranslationContext.Help);
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
			chatTabGuiItem = this.CreateChatTab(new ChatOwnerContent(lobby, this._getDisplayableNickName), false);
			string systemMessage = Language.Get("CUSTOM_MATCH_CHAT_INFO", TranslationContext.Chat);
			SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(systemMessage, lobby.Id.ToString(), true, false, StackableHintKind.None, HintColorScheme.System);
			this.ShowVoiceChatHint(lobby.Id.ToString());
			return chatTabGuiItem;
		}

		public ChatTabGuiItem CreateGroupChatTab(Group group)
		{
			Debug.LogWarningFormat("CREATING GROUP CHAT TAB. Is Group Null? {0}", new object[]
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
				group = new Group
				{
					Guid = ManagerController.Get<GroupManager>().CurrentGroupID,
					Members = this.ConvertGroupMember(ManagerController.Get<GroupManager>().GroupMembersSortedList)
				};
			}
			chatTabGuiItem = this.CreateChatTab(new ChatOwnerContent(group, this._getDisplayableNickName), false);
			return chatTabGuiItem;
		}

		private List<GroupMember> ConvertGroupMember(List<GroupMember> groupMembers)
		{
			List<GroupMember> list = new List<GroupMember>(groupMembers.Count);
			for (int i = 0; i < groupMembers.Count; i++)
			{
				GroupMember groupMember = groupMembers[i];
				GroupMember item = new GroupMember
				{
					PlayerId = groupMember.PlayerId,
					UniversalId = groupMember.UniversalID,
					IsPendingInvite = groupMember.IsPendingInviteToGroup(),
					Nickname = groupMember.PlayerName,
					IsGroupLeader = groupMember.IsUserGroupLeader()
				};
				list.Add(item);
			}
			return list;
		}

		public ChatTabGuiItem CreateUserChatTab(IUser targetUser, bool wasMessageReceived = false, bool ignoreSteamOverlay = false)
		{
			UserFriend userFriend = targetUser as UserFriend;
			if (userFriend != null && userFriend.State != 4 && !ignoreSteamOverlay)
			{
				SingletonMonoBehaviour<SocialController>.Instance.OpenSteamChatWithFriend(userFriend);
				return null;
			}
			if (!wasMessageReceived && targetUser.UniversalID != this._systemMockUser.UniversalID)
			{
				SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
			}
			ChatTabGuiItem chatTabGuiItem;
			if (!this._openChatTabsDictionary.TryGetValue(targetUser.UniversalID, out chatTabGuiItem))
			{
				if (this.IsWindowVisible())
				{
					this.SetSelectionInChatInput();
				}
				chatTabGuiItem = this.CreateChatTab(new ChatOwnerContent(targetUser, this._getDisplayableNickName), wasMessageReceived);
				return chatTabGuiItem;
			}
			if (wasMessageReceived)
			{
				return chatTabGuiItem;
			}
			this.SetActiveTab(chatTabGuiItem);
			return chatTabGuiItem;
		}

		public ChatTabGuiItem CreatePlayerChatTab(IPlayer player, bool wasMessageReceived = false, bool ignoreSteamOverlay = false)
		{
			Assert.IsNotNull<IPlayer>(player, "Player should not be null");
			ChatTabGuiItem chatTabGuiItem;
			if (!this._openChatTabsDictionary.TryGetValue(player.UniversalId, out chatTabGuiItem))
			{
				this.AddUiNavigationFocus();
				chatTabGuiItem = this.CreateChatTab(new ChatOwnerContent(player, this._getDisplayableNickName), wasMessageReceived);
				return chatTabGuiItem;
			}
			if (wasMessageReceived)
			{
				return chatTabGuiItem;
			}
			this.SetActiveTab(chatTabGuiItem);
			this.AddUiNavigationFocus();
			return chatTabGuiItem;
		}

		private static void EnforceTabToggleSelection(GameObject tab)
		{
			UIToggle[] componentsInChildren = tab.transform.parent.GetComponentsInChildren<UIToggle>();
			if (componentsInChildren.Length == 1)
			{
				tab.GetComponent<UIToggle>().value = true;
			}
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
			this.RebuildNavigation();
			SocialModalGUI.EnforceTabToggleSelection(chatTabGuiItem.gameObject);
			return chatTabGuiItem;
		}

		private void RebuildNavigation()
		{
			this._uiNavigationToggleSelector.Rebuild();
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
			InputDevice activeDevice = this._activeDevice.GetActiveDevice();
			this.OnActiveDeviceChange(activeDevice);
			if (Mathf.Approximately(1f, base.Panel.alpha))
			{
				this.SetSelectionInChatInput();
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
				this._activeTabBaseGUI.SetUiNavigationFocus(false);
				this._activeTabBaseGUI.SetTabState(false, true);
				this._activeTabBaseGUI.MsgTextInput = this.ChatUIInput.value;
				this._previousTabBaseGUIStack.Push(this._activeTabBaseGUI);
				this._activeTabBaseGUI.transform.parent = this._chatTabListTable.transform;
				this._activeTabBaseGUI.gameObject.SetActive(false);
				this._activeTabBaseGUI.gameObject.SetActive(true);
				this._chatTabListTable.repositionNow = true;
			}
			tabBaseGUI.SetUiNavigationFocus(true);
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

		private void SetSelectionInChatInput()
		{
			InputDevice activeDevice = this._activeDevice.GetActiveDevice();
			if (activeDevice == 3)
			{
				return;
			}
			this.ChatUIInput.isSelected = true;
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
			this.RebuildNavigation();
			if (this.ShouldSetFocusBackToFriendList())
			{
				this.SetFocusBackToFriendList();
			}
		}

		private bool ShouldSetFocusBackToFriendList()
		{
			return this.IsWindowVisible() && this._activeTabsList.Count == 0;
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
				IGroupStorage groupStorage = this._diContainer.Resolve<IGroupStorage>();
				this.CreateGroupChatTab(groupStorage.Group);
			}
		}

		public void onChatHyperlinkClick(string hyperlinkContent)
		{
			string[] hyperlinkSplited = SocialModalGUI.GetHyperlinkSplited(hyperlinkContent);
			string hyperlink = hyperlinkSplited[0];
			if (SocialModalGUI.CheckHyperlinkCategory(hyperlink, SocialModalGUI.HyperlinkCategory.InviteSuggestion.ToString()))
			{
				this.TryInviteFriendSuggestionToGroup(hyperlinkSplited);
				return;
			}
			if (SocialModalGUI.CheckHyperlinkCategory(hyperlink, SocialModalGUI.HyperlinkCategory.LobbyAccessCode.ToString()))
			{
				ManagerController.Get<MatchManager>().CopyLobbyTokenToClipboard(hyperlinkSplited[1]);
				return;
			}
		}

		private static bool CheckHyperlinkCategory(string hyperlink, string hyperlinkCategory)
		{
			return string.Equals(hyperlink, hyperlinkCategory, StringComparison.InvariantCultureIgnoreCase);
		}

		private void TryInviteFriendSuggestionToGroup(string[] userData)
		{
			HMMHub hub = GameHubBehaviour.Hub;
			Assert.IsTrue(userData.Length == 4, "InviteSuggestion hyperlinkContent with wrong number of parameters");
			UserFriend userFriend = new UserFriend();
			userFriend.UniversalID = userData[1];
			userFriend.PlayerName = userData[2];
			userFriend.PlayerId = long.Parse(userData[3]);
			SerializableCrossplay serializableCrossplay = CrossplayDataSerializableConvertions.ToSerializable(this._getLocalPlayerCrossplayData.Get());
			ManagerController.Get<GroupManager>().TryInviteToGroup(userFriend, serializableCrossplay.Serialize());
			hub.Swordfish.Log.BILogClient(78, true);
		}

		private void OnGroupFriendSuggestion(SocialModalGUI.GroupFriendSuggestionDataTransferObject groupFriendSuggestion)
		{
			if (!this._openChatTabsDictionary.ContainsKey(groupFriendSuggestion.GroupId))
			{
				Debug.LogWarning(string.Format("[onGroupFriendSuggestion] Missing group chat tab for group: {0}, aborting!", groupFriendSuggestion.GroupId));
				return;
			}
			string formatted = Language.GetFormatted("SUGGEST_A_PLAYER", TranslationContext.Help, new object[]
			{
				groupFriendSuggestion.SuggestedByPlayerName,
				HudUtils.RGBToHex(SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.InviteEventPlayerNameColor),
				groupFriendSuggestion.LatestSuggestedPlayerName
			});
			string textContent = formatted;
			float timeoutSeconds = 5f;
			string groupId = groupFriendSuggestion.GroupId;
			BaseHintContent baseHintContent = new BaseHintContent(textContent, timeoutSeconds, true, null, groupId);
			SingletonMonoBehaviour<PanelController>.Instance.ShowMessageHint(baseHintContent, StackableHintKind.GroupSuggestion, HintColorScheme.System);
			DateTime receivedTime = groupFriendSuggestion.ReceivedTime;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("[{0:00}:{1:00}] ", receivedTime.Hour, receivedTime.Minute);
			stringBuilder.AppendFormat(formatted, new object[0]);
			InputDevice activeDevice = this._activeDevice.GetActiveDevice();
			string key = (activeDevice != 3) ? "SUGGEST_PLAYER_ACTION" : "SUGGEST_PLAYER_ACTION_GAMEPAD";
			this._groupSuggestionUrlString = string.Format("{0}|{1}|{2}|{3}", new object[]
			{
				SocialModalGUI.HyperlinkCategory.InviteSuggestion,
				groupFriendSuggestion.SuggestedUniversalId,
				groupFriendSuggestion.LatestSuggestedPlayerName,
				groupFriendSuggestion.SuggestedPlayerId
			});
			stringBuilder.AppendFormat(" [url={0}|{1}|{2}|{5}][{3}]>>>>>[u]{4}[/u][-][/url]", new object[]
			{
				SocialModalGUI.HyperlinkCategory.InviteSuggestion,
				groupFriendSuggestion.SuggestedUniversalId,
				groupFriendSuggestion.LatestSuggestedPlayerName,
				HudUtils.RGBToHex(SingletonMonoBehaviour<PanelController>.Instance.ColorConfiguration.InviteEventPlayerNameColor),
				Language.Get(key, TranslationContext.Help),
				groupFriendSuggestion.SuggestedPlayerId
			});
			this._openChatTabsDictionary[groupFriendSuggestion.GroupId].AppendLineToTab(stringBuilder.ToString());
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
					this.GenerateLocalizedTag("CUSTOM_MATCH_COPY_ACCESS_CODE_CHAT_ACTION", TranslationContext.CustomMatch.ContextName)
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
			Debug.LogError("[SocialModalGUI] - Invalid localization key / tab.");
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
				string key = this.ChangeDraftIfUsingGamepad(m.Groups[1].Value);
				return Language.Get(key, (ContextTag)m.Groups[2].Value);
			}
			return m.Value;
		}

		private string ChangeDraftIfUsingGamepad(string value)
		{
			if (string.CompareOrdinal(value, "CUSTOM_MATCH_COPY_ACCESS_CODE_CHAT_ACTION") != 0)
			{
				return value;
			}
			if (Platform.Current.IsConsole() || this._activeDevice.GetActiveDevice() == 3)
			{
				return this._customMatchCopyAccessCodeChatActionOnGamepad.CurrentPlatformDraft;
			}
			return "CUSTOM_MATCH_COPY_ACCESS_CODE_CHAT_ACTION";
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
			ITextChatRestriction textChatRestriction = this._diContainer.Resolve<ITextChatRestriction>();
			if (textChatRestriction.IsGloballyEnabled())
			{
				if (this._getCurrentPublisher.Get() == Publishers.XboxLive)
				{
					return;
				}
				string translatedMessageRestrictionInGroup = textChatRestriction.GetTranslatedMessageRestrictionInGroup();
				SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(translatedMessageRestrictionInGroup, "SystemMessage", true, true, StackableHintKind.None, HintColorScheme.Group);
				return;
			}
			else
			{
				this.PlaySocialSfx(this.sfx_ui_voicechat_notification);
				if (this._voiceRestrictions.IsVoiceDisabled())
				{
					this.SendMyGamesVoiceDisabledSystemMessage();
					return;
				}
				string inputActionActiveDeviceTranslation = this._inputTranslation.GetInputActionActiveDeviceTranslation(18);
				string key;
				if (this._voiceChatPreferences.TeamStatus == null)
				{
					key = "GROUP_VOICE_CHAT_DISABLED_NOTIFICATION";
				}
				else if (this._voiceChatPreferences.InputType == 2)
				{
					key = "GROUP_VOICE_CHAT_ALWAYS_ON_NOTIFICATION";
				}
				else if (this._voiceChatPreferences.InputType == 1)
				{
					key = "GROUP_VOICE_CHAT_TOGGLE_NOTIFICATION";
				}
				else
				{
					key = "GROUP_VOICE_CHAT_NOTIFICATION";
				}
				string text = Language.GetFormatted(key, TranslationContext.HUDChat, new object[]
				{
					inputActionActiveDeviceTranslation
				});
				string textContent = Language.Get("HINT_VOICE_CHAT_NOTIFICATION_TOP", TranslationContext.HUDChat);
				string text2 = string.Empty;
				VoiceChatInputType inputType = this._voiceChatPreferences.InputType;
				if (inputType != null)
				{
					if (inputType == 1)
					{
						text2 = Language.GetFormatted("VOICE_NOTIFICATION_KEY", TranslationContext.HUDChat, new object[]
						{
							inputActionActiveDeviceTranslation
						});
					}
				}
				else
				{
					text2 = Language.GetFormatted("VOICE_NOTIFICATION_KEY", TranslationContext.HUDChat, new object[]
					{
						inputActionActiveDeviceTranslation
					});
					if (this._voiceChatPreferences.TeamStatus == 1)
					{
						text = string.Format("{0} {1}", text, text2);
					}
				}
				if (this._getCurrentPublisher.Get() == Publishers.Psn)
				{
					text2 = Language.Get("CHAT_VOICE_CHAT_HINT_GROUP_OPEN_MIC", TranslationContext.HUDChat);
				}
				if (!string.IsNullOrEmpty(ownerId) && !ownerId.Equals(Guid.Empty.ToString()))
				{
					SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(text, ownerId, true, true, StackableHintKind.None, HintColorScheme.Group);
				}
				if (this._voiceChatPreferences.TeamStatus == null)
				{
					return;
				}
				BaseHintContent baseHintContent = new BaseHintContent(textContent, text2, 5f, false, this.voicechatSprite, ownerId);
				SingletonMonoBehaviour<PanelController>.Instance.ShowMessageHint(baseHintContent, StackableHintKind.None, HintColorScheme.Group);
				return;
			}
		}

		private void SendMyGamesVoiceDisabledSystemMessage()
		{
			string formatted = this._localizeKey.GetFormatted("VOICE_CHAT_NOTIFICATION_DISABLED", TranslationContext.HUDChat, new object[0]);
			SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(formatted, "SystemMessage", true, true, StackableHintKind.None, HintColorScheme.Group);
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
		public AudioEventAsset FeedBackAudioFx;

		public AudioEventAsset sfx_ui_chat_open;

		public AudioEventAsset sfx_ui_chat_close;

		public AudioEventAsset sfx_ui_chat_tab_close;

		public AudioEventAsset sfx_ui_chat_tab_new;

		public AudioEventAsset sfx_ui_chat_click;

		public AudioEventAsset sfx_ui_chat_filter_minimize;

		public AudioEventAsset sfx_ui_chat_filter_maximize;

		public AudioEventAsset sfx_ui_chat_group_create;

		public AudioEventAsset sfx_ui_chat_message_receive;

		public AudioEventAsset sfx_ui_chat_message_sent;

		public AudioEventAsset sfx_ui_chat_invite_receive;

		public AudioEventAsset sfx_ui_chat_invite_sent;

		public AudioEventAsset sfx_ui_chat_member_offline;

		[SerializeField]
		private GameObject _chatMessagesGroup;

		[SerializeField]
		public HMMUIInput ChatUIInput;

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

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationToggleSelector _uiNavigationToggleSelector;

		[SerializeField]
		private UiNavigationContextInputNotifier _uiNavigationContextInputNotifier;

		[SerializeField]
		private GameObject _uiNavigationHighlightBorder;

		[SerializeField]
		private UI2DSprite _gamepadCancelFocusShortcutImage;

		[SerializeField]
		private MultiPlatformLocalizationDraft _customMatchCopyAccessCodeChatActionOnGamepad;

		private ChatTabGuiItem _activeTabBaseGUI;

		private ChatTabGuiItem _innactiveTabBaseGUI;

		private Stack<ChatTabGuiItem> _previousTabBaseGUIStack = new Stack<ChatTabGuiItem>();

		private List<ChatTabGuiItem> _activeTabsList = new List<ChatTabGuiItem>();

		private int _chatTabNavigationIndex;

		private Regex _accessCodeRegex;

		private Regex _reversingAccessCodeRegex;

		private string _groupSuggestionUrlString;

		[InjectOnClient]
		private IControllerInputActionPoller _inputActionPoller;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private ILocalPlayerStorage _localPlayer;

		[InjectOnClient]
		private IConfigLoader _configLoader;

		[InjectOnClient]
		private IGetPublisherPresentingData _getPublisherPresentingData;

		[InjectOnClient]
		private IGetCurrentPublisher _getCurrentPublisher;

		private IGetPublisherUserName _getPublisherUserName;

		private IGetDisplayableNickName _getDisplayableNickName;

		[Inject]
		private IIsPlayerLocalPlayerFriend _isPlayerLocalPlayerFriend;

		[Inject]
		private IObserveChatMessageRequests _observeChatMessageRequests;

		[Inject]
		private IObserveChatGroupMessageRequest _observeChatGroupMessageRequest;

		[Inject]
		private IVoiceRestrictions _voiceRestrictions;

		[Inject]
		private ILocalizeKey _localizeKey;

		[Inject]
		private IObserveChatGroupUiNavigationFocusRequest _chatGroupUiNavigationFocusRequest;

		[Inject]
		private IInputGetActiveDevicePoller _activeDevice;

		[Inject]
		private IUiNavigationHasHighPriorityGroupFocused _hasHighPriorityGroupFocused;

		[Inject]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[Inject]
		private IVoiceChatPreferences _voiceChatPreferences;

		[Inject]
		private IGetLocalPlayerCrossplayData _getLocalPlayerCrossplayData;

		[Inject]
		private IInputTranslationProvider _inputTranslationProvider;

		[Inject]
		private DiContainer _diContainer;

		private CompositeDisposable _disposables;

		[SerializeField]
		private UIScrollView _chatTabScrollView;

		private const int MaxPlayerShortNameCharCount = 12;

		private Dictionary<string, UserFriend> _unknownUsers = new Dictionary<string, UserFriend>();

		private const int maxActiveChatTabs = 3;

		private const string CustomMatchCopyAccessCodeChatAction = "CUSTOM_MATCH_COPY_ACCESS_CODE_CHAT_ACTION";

		private const string GroupSuggestionCodeAction = "SUGGEST_PLAYER_ACTION";

		private const string GroupSuggestionCodeChatActionOnGamepad = "SUGGEST_PLAYER_ACTION_GAMEPAD";

		private const ControllerInputActions CopyTokenWithGamepad = 41;

		private const ControllerInputActions InviteGroupSuggestionControllerInputAction = 52;

		private UserFriend m_systemMockUser;

		private Dictionary<string, ChatTabGuiItem> _openChatTabsDictionary = new Dictionary<string, ChatTabGuiItem>();

		private int remnantVisibleTabs = 3;

		private Regex _localizationTag;

		public Sprite voicechatSprite;

		public AudioEventAsset sfx_ui_voicechat_notification;

		private class GroupFriendSuggestionDataTransferObject : GroupFriendSuggestion
		{
			public DisplayableNicknameParameters DisplayableNicknameParameters;

			public string LatestSuggestedPlayerName;
		}

		private enum HyperlinkCategory
		{
			InviteSuggestion,
			LobbyAccessCode
		}
	}
}
