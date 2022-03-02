using System;
using System.Collections.Generic;
using System.Text;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using Assets.Standard_Assets.Scripts.Infra.GUI.Hints;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.Objects;
using ClientAPI.Objects.Partial;
using HeavyMetalMachines.Chat.Filters;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Crossplay.Rules;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social.Friends.Business;
using HeavyMetalMachines.Social.Friends.Models;
using HeavyMetalMachines.VFX.PlotKids;
using Hoplon.Input;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.VFX
{
	public class ChatTabGuiItem : BaseGUIItem<ChatTabGuiItem, ChatOwnerContent>
	{
		public bool GotUnreadMessages
		{
			get
			{
				return SingletonMonoBehaviour<SocialController>.Instance.ChatUiFeedbackDispatcher.GetPendingMessagesCount(base.ReferenceObject.UniversalId) > 0;
			}
		}

		public string MsgTextInput
		{
			get
			{
				if (this._undoTextControl == null)
				{
					return null;
				}
				return this._undoTextControl.CurrentText;
			}
			set
			{
				if (this._undoTextControl == null)
				{
					this._undoTextControl = new UndoTextControl();
				}
				this._undoTextControl.PushNewText(value);
			}
		}

		public string GetTabText()
		{
			return this._stringBuilder.ToString();
		}

		public void ClearMsgText()
		{
			if (this._undoTextControl == null)
			{
				return;
			}
			this._undoTextControl.Clear();
		}

		public string Redo()
		{
			return this._undoTextControl.Redo();
		}

		public string Undo()
		{
			return this._undoTextControl.Undo();
		}

		private void OnEnable()
		{
			this._disposables = new CompositeDisposable();
			ManagerController.Get<GroupManager>().EvtGroupQuit += this.OnGroupQuit;
			ManagerController.Get<MatchManager>().EvtLobbyFinished += this.OnLobbyFinished;
			this._activeSprite.color = this.NormalColor;
			this._inactiveSprite.color = this.NormalColor;
			this._hub = GameHubBehaviour.Hub;
			this._disposables.Add(ObservableExtensions.Subscribe<Friend>(Observable.Do<Friend>(this._observeFriendChanges.Observe(), new Action<Friend>(this.HandleFriendChanges))));
			IDisposable disposable = ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(this._getAndObserveCrossplayChange.GetAndObserve(), new Action<bool>(this.HandleCrossplayChange)));
			this._disposables.Add(disposable);
		}

		private void OnLobbyFinished(Lobby lobby, LobbyMatchmakingMessage.LobbyMessageErrorType lobbyerrortype)
		{
			if (base.ReferenceObject.ChatKind != ChatOwnerContent.ChatContentKind.CustomMatchLobby)
			{
				return;
			}
			if (!string.Equals(lobby.Id.ToString(), base.ReferenceObject.UniversalId))
			{
				return;
			}
			this.CloseTab();
		}

		private void OnDisable()
		{
			if (!SingletonMonoBehaviour<SocialController>.DoesInstanceExist())
			{
				return;
			}
			this._disposables.Dispose();
			ManagerController.Get<GroupManager>().EvtGroupQuit -= this.OnGroupQuit;
			ManagerController.Get<MatchManager>().EvtLobbyFinished -= this.OnLobbyFinished;
			this._activeSprite.color = this.Offlinecolor;
			this._inactiveSprite.color = this.Offlinecolor;
		}

		private void HandleFriendChanges(Friend friend)
		{
			if (!friend.UniversalId.Equals(base.ReferenceObject.UniversalId))
			{
				return;
			}
			this.TitleLabel.text = friend.Nickname;
		}

		private void OnFriendRefreshed(UserFriend friend)
		{
			if (!friend.UniversalID.Equals(base.ReferenceObject.UniversalId))
			{
				return;
			}
			this.TitleLabel.text = friend.PlayerName;
		}

		public void onButtonClick_CloseTab()
		{
			this.CloseTab();
		}

		public void OnClick()
		{
			int currentTouchID = UICamera.currentTouchID;
			if (currentTouchID != -3)
			{
				if (currentTouchID == -1)
				{
					this._parentUI.SetActiveTab(this);
					this._parentUI.RefreshActiveChatTabs();
					this._hasNewMessageFx.enabled = false;
				}
			}
			else
			{
				this.CloseTab();
			}
			SingletonMonoBehaviour<SocialController>.Instance.ChatUiFeedbackDispatcher.ClearPendingChatMessagesFromUID(base.ReferenceObject.UniversalId);
		}

		private void OnGroupQuit(string groupid)
		{
			if (base.ReferenceObject.ChatKind != ChatOwnerContent.ChatContentKind.Group)
			{
				return;
			}
			if (!string.Equals(groupid, base.ReferenceObject.UniversalId))
			{
				return;
			}
			this.CloseTab();
		}

		public void CloseTab()
		{
			if (!this.CanCloseChatTab())
			{
				this.SendSystemMsg_UnableToCloseChatTab();
				return;
			}
			SingletonMonoBehaviour<SocialController>.Instance.ChatUiFeedbackDispatcher.ClearPendingChatMessagesFromUID(base.ReferenceObject.UniversalId);
			if (base.ReferenceObject.ChatKind != ChatOwnerContent.ChatContentKind.CustomMatchLobby)
			{
				ChatTabGuiItem.ChatTabLogsFromPlaySessionDictionary[base.ReferenceObject.UniversalId] = this._stringBuilder.ToString();
				ChatTabGuiItem.ChatTabIconsFromPlaySessionDictionary[base.ReferenceObject.UniversalId] = this._activeIcons;
			}
			this._parentUI.TabClosed(this);
			this._parentUI.ChatUIInput.value = null;
			this.SetActiceGamepadShortcut(false);
			if (this._undoTextControl != null)
			{
				this._undoTextControl.Clear();
			}
			Object.Destroy(base.gameObject);
			ChatUiFeedbackDispatcher.EvtPendingMsgCountUpdated -= this.onPendingMsgCountUpdated;
		}

		private void HandleCrossplayChange(bool enabledPSNCrossplay)
		{
			Friend friendByUniversalId = this._getFriends.GetFriendByUniversalId(base.ReferenceObject.UniversalId);
			if (friendByUniversalId == null)
			{
				return;
			}
			if (this._crossplaySocialRuleFriendShouldNotBeSeenInFriendList.Get(friendByUniversalId.PlayerId, friendByUniversalId.Publisher, friendByUniversalId.CrossPlayEnable))
			{
				this.CloseTab();
			}
		}

		private bool CanCloseChatTab()
		{
			return !base.ReferenceObject.IsChatCloseActionLocked;
		}

		private void SendSystemMsg_UnableToCloseChatTab()
		{
			if (this._nextAllowedCloseGroupChatMessage > Time.time)
			{
				return;
			}
			this._nextAllowedCloseGroupChatMessage = Time.time + 5f;
			SingletonMonoBehaviour<PanelController>.Instance.SendSystemMessage(Language.Get("CANT_CLOSE_ACTIVE_GROUP_CHAT_WINDOW", TranslationContext.Help), base.ReferenceObject.UniversalId, true, false, StackableHintKind.None, HintColorScheme.System);
		}

		protected override void SetPropertiesTasks(ChatOwnerContent chatOwner)
		{
			this.TitleLabel.text = chatOwner.OwnerName;
			this._baseChatTabName = string.Format("{0}_{1}_{2}", base.gameObject.name, chatOwner.OwnerName, chatOwner.UniversalId);
			base.gameObject.name = this._baseChatTabName;
			if (ChatTabGuiItem.ChatTabLogsFromPlaySessionDictionary.ContainsKey(base.ReferenceObject.UniversalId))
			{
				this._stringBuilder.Append(ChatTabGuiItem.ChatTabLogsFromPlaySessionDictionary[base.ReferenceObject.UniversalId]);
				this.UpdateChatLabel();
				this._activeIcons = ChatTabGuiItem.ChatTabIconsFromPlaySessionDictionary[base.ReferenceObject.UniversalId];
				this._parentUI.ChatFilter.UpdateIconsPosition(0, ref this._activeIcons, this.CurrentChatTabState);
			}
			this.RefreshCloseButtonState();
			ChatUiFeedbackDispatcher.EvtPendingMsgCountUpdated += this.onPendingMsgCountUpdated;
		}

		private void RefreshCloseButtonState()
		{
			bool active = this.CanCloseChatTab();
			for (int i = 0; i < this._closeChatTabButtons.Length; i++)
			{
				this._closeChatTabButtons[i].gameObject.SetActive(active);
			}
		}

		private void onPendingMsgCountUpdated()
		{
			if (SingletonMonoBehaviour<SocialController>.Instance.ChatUiFeedbackDispatcher.GetPendingMessagesCount(base.ReferenceObject.UniversalId) == 0)
			{
				return;
			}
			if (this._currentChatTabState && this._parentUI.IsWindowVisible())
			{
				SingletonMonoBehaviour<SocialController>.Instance.ChatUiFeedbackDispatcher.ClearPendingChatMessagesFromUID(base.ReferenceObject.UniversalId);
				return;
			}
		}

		public void UpdateChatTabNameByIndex(int targetIndex)
		{
			base.gameObject.name = string.Format("{0:0000}_{1}", targetIndex, this._baseChatTabName);
			this.Index = targetIndex;
		}

		public void AppendLineToTab(string message)
		{
			this._chatMessageBadWordFilter.Filter(message, ref message);
			message = this._parentUI.ChatFilter.OnMessageReceived(message, ref this._activeIcons);
			int num = 0;
			while (this._stringBuilder.Length + message.Length > 6000)
			{
				string text = this._stringBuilder.ToString();
				int num2 = text.IndexOf("\n", StringComparison.Ordinal);
				this._stringBuilder.Remove(0, num2 + 1);
				this._parentUI.ChatFilter.OnLineRemoved(num, ref this._activeIcons);
				num++;
			}
			this._parentUI.ChatFilter.UpdateIconsPosition(-num, ref this._activeIcons, this.CurrentChatTabState);
			this._stringBuilder.AppendLine(message);
			this.UpdateChatLabel();
		}

		public void ClearIcons()
		{
			this._parentUI.ChatFilter.Clear(ref this._activeIcons);
		}

		public void UpdateIcons()
		{
			this._parentUI.ChatFilter.UpdateIconsPosition(0, ref this._activeIcons, this.CurrentChatTabState);
		}

		public bool CurrentChatTabState
		{
			get
			{
				return this._currentChatTabState;
			}
		}

		public void SetTabState(bool targetState, bool centerTabOnScrollView = true)
		{
			this._currentChatTabState = targetState;
			bool flag = base.ReferenceObject.ChatKind == ChatOwnerContent.ChatContentKind.User;
			this._activeSprite.gameObject.SetActive(targetState && flag);
			this._inactiveSprite.gameObject.SetActive(!targetState && flag);
			this._groupMaximizedSprite.gameObject.SetActive(targetState && !flag);
			this._groupMinimizedSprite.gameObject.SetActive(!targetState && !flag);
			if (this._activeSprite.isActiveAndEnabled || this._inactiveSprite.isActiveAndEnabled)
			{
				this._hasNewMessageFx.color = this.NormalColor;
			}
			else
			{
				this._hasNewMessageFx.color = this.GroupColor;
			}
			if (targetState)
			{
				this._hasNewMessageFx.enabled = false;
				this.UpdateChatLabel();
				SingletonMonoBehaviour<SocialController>.Instance.ChatUiFeedbackDispatcher.ClearPendingChatMessagesFromUID(base.ReferenceObject.UniversalId);
			}
			base.gameObject.SetActive(targetState);
			this.SetActiceGamepadShortcut(this.ShouldShowGamepadShortcuts());
		}

		private void SetActiceGamepadShortcut(bool active)
		{
			if (active)
			{
				this.TryToSetupUiNavigationShortcutIconToCloseTab();
			}
			this._gamepadCloseGroupTabShortcutImage.gameObject.SetActive(active);
			this._gamepadCloseBaseTabShortcutImage.gameObject.SetActive(active);
		}

		private void TryToSetupUiNavigationShortcutIconToCloseTab()
		{
			int num = 44;
			ISprite sprite;
			string text;
			this._inputTranslation.TryToGetInputActionJoystickAssetOrFallbackToTranslation(num, ref sprite, ref text);
			Sprite sprite2 = (sprite as UnitySprite).GetSprite();
			if (this._gamepadCloseGroupTabShortcutImage != null)
			{
				this._gamepadCloseGroupTabShortcutImage.sprite2D = sprite2;
			}
			if (this._gamepadCloseBaseTabShortcutImage != null)
			{
				this._gamepadCloseBaseTabShortcutImage.sprite2D = sprite2;
			}
		}

		private bool ShouldShowGamepadShortcuts()
		{
			bool flag = this._activeDevice.GetActiveDevice() == 3;
			return flag && this._currentChatTabState && this._parentUI.IsOnUiNavigationFocus && this._isOnUiNavigationFocus;
		}

		private void UpdateChatLabel()
		{
			this._parentUI.TryUpdateChatLabel(this, this._stringBuilder.ToString());
		}

		public void HighlightChatTab()
		{
			if (!this._activeSprite.isActiveAndEnabled)
			{
				this._hasNewMessageFx.enabled = true;
			}
		}

		public void SetUiNavigationFocus(bool isOnFocus)
		{
			this._isOnUiNavigationFocus = isOnFocus;
			this.SetActiceGamepadShortcut(this.ShouldShowGamepadShortcuts());
		}

		public Color NormalColor;

		public Color GroupColor;

		public Color Offlinecolor;

		public int Index;

		[SerializeField]
		private UI2DSprite _gamepadCloseGroupTabShortcutImage;

		[SerializeField]
		private UI2DSprite _gamepadCloseBaseTabShortcutImage;

		[Inject]
		private IInputGetActiveDevicePoller _activeDevice;

		[Inject]
		private IInputTranslation _inputTranslation;

		[Inject]
		private IObserveCrossplayChange _getAndObserveCrossplayChange;

		[Inject]
		private IGetFriends _getFriends;

		[Inject]
		private ICrossplaySocialRuleFriendShouldNotBeSeenInFriendList _crossplaySocialRuleFriendShouldNotBeSeenInFriendList;

		[Inject]
		private IChatMessageBadWordFilter _chatMessageBadWordFilter;

		private bool _isOnUiNavigationFocus;

		private static readonly Dictionary<string, string> ChatTabLogsFromPlaySessionDictionary = new Dictionary<string, string>();

		private static readonly Dictionary<string, List<ChatFilter.ActiveIcons>> ChatTabIconsFromPlaySessionDictionary = new Dictionary<string, List<ChatFilter.ActiveIcons>>();

		private UndoTextControl _undoTextControl;

		[SerializeField]
		private SocialModalGUI _parentUI;

		[Header("Chat_Box_Texts")]
		public UILabel TitleLabel;

		public UILabel InboxUILabel;

		private StringBuilder _stringBuilder = new StringBuilder(7000);

		[SerializeField]
		private UI2DSprite _activeSprite;

		[SerializeField]
		private UI2DSprite _inactiveSprite;

		[SerializeField]
		private UI2DSprite _hasNewMessageFx;

		[SerializeField]
		private UI2DSprite _groupMaximizedSprite;

		[SerializeField]
		private UI2DSprite _groupMinimizedSprite;

		[SerializeField]
		private UIButton[] _closeChatTabButtons;

		private HMMHub _hub;

		private CompositeDisposable _disposables;

		[InjectOnClient]
		private IObservePlayerChanges<Friend> _observeFriendChanges;

		[InjectOnClient]
		private IConfigLoader _configLoader;

		private const float MinimumIntervalCloseGroupChatTab = 5f;

		private float _nextAllowedCloseGroupChatMessage;

		private string _baseChatTabName;

		private List<ChatFilter.ActiveIcons> _activeIcons = new List<ChatFilter.ActiveIcons>();

		private bool _currentChatTabState;
	}
}
