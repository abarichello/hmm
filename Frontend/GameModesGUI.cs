using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.Matchmaking;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.Objects;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids;
using Hoplon.Input;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using JetBrains.Annotations;
using Pocketverse;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class GameModesGUI : GameHubBehaviour
	{
		private IUiNavigationGroupHolder CustomMatchUiNavigationGroupHolder
		{
			get
			{
				return this._customMatchUiNavigationGroupHolder;
			}
		}

		private IUiNavigationSubGroupHolder CustomMatchLobbyUiNavigationSubGroupHolder
		{
			get
			{
				return this._customMatchLobbyUiNavigationSubGroupHolder;
			}
		}

		private IUiNavigationSubGroupHolder CustomMatchEnterUiNavigationSubGroupHolder
		{
			get
			{
				return this._customMatchEnterUiNavigationSubGroupHolder;
			}
		}

		private IUiNavigationEdgeNotifier CustomMatchLobbyUiNavigationEdgeNotifier
		{
			get
			{
				return this._customMatchLobbyUiNavigationAxisSelector;
			}
		}

		private IUiNavigationEdgeNotifier CustomMatchEnterUiNavigationEdgeNotifier
		{
			get
			{
				return this._customMatchEnterUiNavigationAxisSelector;
			}
		}

		private static MainMenuGui MainMenuGui
		{
			get
			{
				return GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			}
		}

		[UsedImplicitly]
		private void Awake()
		{
			this._rootPanel = this.RootGameObject.GetComponent<NGUIPanelAlpha>();
			this.RootGameObject.SetActive(false);
		}

		private void ObservePreCustomNotifications()
		{
			this._lobbyUiNavigationEdgeNotifierDisposable = ObservableExtensions.Subscribe<AxisSelectorEdge>(this.CustomMatchLobbyUiNavigationEdgeNotifier.ObserveOnEdgeReached(), delegate(AxisSelectorEdge edge)
			{
				if (edge == 3)
				{
					this.CustomMatchEnterFocus();
				}
			});
			this._enterUiNavigationEdgeNotifierDisposable = ObservableExtensions.Subscribe<AxisSelectorEdge>(this.CustomMatchEnterUiNavigationEdgeNotifier.ObserveOnEdgeReached(), delegate(AxisSelectorEdge edge)
			{
				if (edge == 2)
				{
					this.CustomMatchLobbyFocus();
				}
			});
			this._activeDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), delegate(InputDevice _)
			{
				this.UpdateCustomMatchHighlight();
			});
		}

		private void CustomMatchEnterFocus()
		{
			this._isCustomMatchEnterFocused = true;
			this.CustomMatchLobbyUiNavigationSubGroupHolder.SubGroupFocusRelease();
			this.CustomMatchEnterUiNavigationSubGroupHolder.SubGroupFocusGet();
			this.UpdateCustomMatchHighlight();
		}

		private void CustomMatchLobbyFocus()
		{
			this._isCustomMatchEnterFocused = false;
			this.CustomMatchEnterUiNavigationSubGroupHolder.SubGroupFocusRelease();
			this.CustomMatchLobbyUiNavigationSubGroupHolder.SubGroupFocusGet();
			this.UpdateCustomMatchHighlight();
		}

		private void UpdateCustomMatchHighlight()
		{
			bool flag = this.IsJoystickActive();
			this._customMatchEnterGamepadHighlightGameObject.SetActive(this._isCustomMatchEnterFocused && flag);
			this._customMatchLobbyGamepadHighlightGameObject.SetActive(!this._isCustomMatchEnterFocused && flag);
		}

		[UsedImplicitly]
		private void Start()
		{
			this._acessCodeUiInput.defaultText = Language.Get("GAMEMODE_ACCESS_CODE_TITLE", TranslationContext.CustomMatch);
			GameHubBehaviour.Hub.ClientApi.matchmakingClient.Connection += this.OnMatchmakingClientConnection;
			GameHubBehaviour.Hub.ClientApi.lobby.LobbyReady += new EventHandlerEx<MatchmakingLobbyCreatedEventArgs>(this.OnCustomMatchLobbyReady);
			GameHubBehaviour.Hub.ClientApi.lobby.JoinedLobby += new EventHandlerEx<MatchmakingUpdateLobbyMembersEventArgs>(this.OnCustomMatchLobbyJoined);
			CustomMatchController.EvtCreateOrJoinLobbyError += this.OnCustomMatchLobbyCreateOrJoinError;
			EventDelegate item = new EventDelegate(new EventDelegate.Callback(this.UpdateAccessCodeInputState));
			this._acessCodeUiInput.onChange.Add(item);
		}

		private bool IsJoystickActive()
		{
			return this._inputGetActiveDevicePoller.GetActiveDevice() == 3;
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			GameHubBehaviour.Hub.ClientApi.matchmakingClient.Connection -= this.OnMatchmakingClientConnection;
			GameHubBehaviour.Hub.ClientApi.lobby.LobbyReady -= new EventHandlerEx<MatchmakingLobbyCreatedEventArgs>(this.OnCustomMatchLobbyReady);
			GameHubBehaviour.Hub.ClientApi.lobby.JoinedLobby -= new EventHandlerEx<MatchmakingUpdateLobbyMembersEventArgs>(this.OnCustomMatchLobbyJoined);
			CustomMatchController.EvtCreateOrJoinLobbyError -= this.OnCustomMatchLobbyCreateOrJoinError;
			this._acessCodeUiInput.onChange.Clear();
			this.TryToDisposePreCustomNotifications();
		}

		private void TryToDisposePreCustomNotifications()
		{
			if (this._lobbyUiNavigationEdgeNotifierDisposable != null)
			{
				this._lobbyUiNavigationEdgeNotifierDisposable.Dispose();
			}
			if (this._enterUiNavigationEdgeNotifierDisposable != null)
			{
				this._enterUiNavigationEdgeNotifierDisposable.Dispose();
			}
			if (this._activeDeviceChangeNotifierDisposable != null)
			{
				this._activeDeviceChangeNotifierDisposable.Dispose();
			}
		}

		private void SetGameModesButtonsColliders(bool isEnabled, bool customHoverOut = false)
		{
			this._normalButton.GetComponent<Collider>().enabled = isEnabled;
			this._customMatchButton.GetComponent<Collider>().enabled = isEnabled;
			this._tutorialButton.GetComponent<Collider>().enabled = isEnabled;
			if (customHoverOut)
			{
				this._gameModesHoverView.OnCustomMatchButtonHoverOut();
			}
			this._normalButton.SetState(UIButtonColor.State.Normal, true);
			this._customMatchButton.SetState(UIButtonColor.State.Normal, true);
			this._rankedInfoButton.SetState(UIButtonColor.State.Normal, true);
			this._tutorialButton.SetState(UIButtonColor.State.Normal, true);
			if (isEnabled)
			{
				this._regionGameModeButtonGui.EnableRegionButton();
			}
			else
			{
				this._regionGameModeButtonGui.DisableRegionButton();
			}
		}

		private void SetCustomMatchButtonsColliders(bool isEnabled)
		{
			this._customMatchCreateButton.GetComponent<Collider>().enabled = isEnabled;
			this._customMatchEnterButton.GetComponent<Collider>().enabled = isEnabled;
			this._customMatchCreateButton.SetState(UIButtonColor.State.Normal, true);
			this._customMatchEnterButton.SetState(UIButtonColor.State.Normal, true);
		}

		public void ReturnToMainMenu()
		{
			this.AnimateHideWindow();
			if (this.IsCustomMatch())
			{
				return;
			}
			GameModesGUI.MainMenuGui.AnimateReturnToLobby(false, false);
		}

		private bool IsCustomMatch()
		{
			return GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.MatchMadeQueue == "Custom";
		}

		public void AnimateShowWindow(bool returningFromCustomMatch)
		{
			this._rootPanel.alpha = 1f;
			this._modesAnimation.Play("gameModeIn");
			this.SetGameModesButtonsColliders(true, false);
		}

		public void AnimateHideWindow()
		{
			this.SetGameModesButtonsColliders(false, false);
			this.SetCustomMatchButtonsColliders(false);
			this.TryToDisposePreCustomNotifications();
		}

		private IEnumerator AnimateHideWindowCoroutine()
		{
			this._mainAnimation.Play("backgroundGameModesOut");
			this._modesAnimation.Play("gameModeOut");
			while (this._mainAnimation.isPlaying || this._modesAnimation.isPlaying)
			{
				yield return null;
			}
			this.RootGameObject.SetActive(false);
			this._rootPanel.alpha = 0f;
			yield break;
		}

		public void OnClickedNormalGame(GameModeTabs queue)
		{
			if (!this.CheckCanJoinPvpOrCompetitiveQueue())
			{
				return;
			}
			this.SearchForAMatchAndReturnToMainMenu(queue);
		}

		public void OnClickedCustomMatch()
		{
			this.OpenPreCustomMatch();
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.PreCustomMatchNode));
		}

		public void OnClickedCustomMatchFromMainMenu()
		{
			this.RootGameObject.SetActive(true);
			this._rootPanel.alpha = 1f;
			this._shouldReturnToMainMenuAfterLeavePreCustom = true;
			this.OpenPreCustomMatch();
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.MainMenuPreCustomMatchNode));
		}

		private void OpenPreCustomMatch()
		{
			GameHubBehaviour.Hub.Match.Kind = 4;
			this.CustomMatchGroupGameObject.SetActive(true);
			this._customMatchAnimation.Play("customGroupIn");
			this._modesAnimation.Play("gameModeOut");
			this._customMatchGroup.SetActive(true);
			this.SetGameModesButtonsColliders(false, true);
			this.SetCustomMatchButtonsColliders(true);
			this.CustomMatchUiNavigationGroupHolder.AddGroup();
			this.TryToDisposePreCustomNotifications();
			this.ObservePreCustomNotifications();
			this.CustomMatchLobbyFocus();
			this._buttonBILogger.LogButtonClick(ButtonName.GameModeCustom);
			this.UpdateAccessCodeInputState();
		}

		public void OnClickedCustomMatchBackButton()
		{
			this._acessCodeUiInput.Set(null, false);
			this._customMatchAnimation.Play("customGroupOut");
			this.TryToDisposePreCustomNotifications();
			this.ToggleCustomMatchLobbyErrorFeedback(false, null);
			this.SetCustomMatchButtonsColliders(false);
			this._buttonBILogger.LogButtonClick(ButtonName.GameModeCustomBack);
			this.CustomMatchUiNavigationGroupHolder.RemoveGroup();
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateBackwards());
			if (this._shouldReturnToMainMenuAfterLeavePreCustom)
			{
				this.ForceHideGameModesGuiBackground();
				return;
			}
			this.AnimateShowWindow(true);
		}

		public void ForceHideGameModesGuiBackground()
		{
			this.RootGameObject.SetActive(false);
			this._rootPanel.alpha = 0f;
		}

		public void OnClickedCompetitiveMatchButton(GameModeTabs queueName)
		{
			this._buttonBILogger.LogButtonClick(ButtonName.GameModeRanked);
			if (!this.CheckCanJoinPvpOrCompetitiveQueue())
			{
				return;
			}
			GameHubBehaviour.Hub.Match.Kind = 3;
			this.SearchForAMatchAndReturnToMainMenu(queueName);
		}

		private bool CheckCanJoinPvpOrCompetitiveQueue()
		{
			if (GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Visible)
			{
				return false;
			}
			if (ManagerController.Get<GroupManager>().CurrentGroupID != Guid.Empty && ManagerController.Get<GroupManager>().GetSelfGroupStatus() != GroupStatus.Owner)
			{
				CustomMatchController.ShowCustomMatchDialog(Language.Get("YouAreNotPartyOwner", TranslationContext.MainMenuGui));
				return false;
			}
			for (int i = 0; i < ManagerController.Get<GroupManager>().GroupMembersSortedList.Count; i++)
			{
				GroupMember groupMember = ManagerController.Get<GroupManager>().GroupMembersSortedList[i];
				if (groupMember.GroupId.Equals(Guid.Empty))
				{
					string formatted = Language.GetFormatted("CANT_QUEUE_PENDING_INVITE", TranslationContext.MainMenuGui, new object[]
					{
						groupMember.PlayerName
					});
					BaseHintContent baseHintContent = new BaseHintContent(formatted, 5f, true, null, "SystemMessage");
					SingletonMonoBehaviour<PanelController>.Instance.ShowMessageHint(baseHintContent, StackableHintKind.None, HintColorScheme.System);
					return false;
				}
			}
			if (this._matchBlocker.IsBlocked())
			{
				CustomMatchController.ShowCustomMatchDialog(Language.Get("MatchBlockerWarning", TranslationContext.MainMenuGui));
				return false;
			}
			return true;
		}

		private void SearchForAMatchAndReturnToMainMenu(GameModeTabs currentTab)
		{
			this.GameMode = currentTab;
			GameModesGUI.MainMenuGui.SearchForAMatch();
			GameModesGUI.MainMenuGui.AnimateReturnToLobbySearchingMatch();
			this.AnimateHideWindow();
		}

		private void OnCustomMatchLobbyReady(object sender, MatchmakingLobbyCreatedEventArgs matchmakingLobbyCreatedEventArgs)
		{
			GameModesGUI.Log.Debug("[GameModesGUI] OnLobbyReady");
			this.ShowCustomMatchLobbyWindow();
		}

		private void OnCustomMatchLobbyJoined(object sender, MatchmakingUpdateLobbyMembersEventArgs matchmakingUpdateLobbyMembersEventArgs)
		{
			GameModesGUI.Log.Debug("[GameModesGUI] OnLobbyJoined");
			this.ShowCustomMatchLobbyWindow();
		}

		private void ShowCustomMatchLobbyWindow()
		{
			this._acessCodeUiInput.Set(null, false);
			this._acessCodeUiInput.isSelected = false;
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.HideWaitingWindow(typeof(CustomMatchController));
			this.CustomMatchGroupGameObject.SetActive(false);
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryCloseAll();
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<CustomMatchLobby_Modal_UI>();
			this.CustomMatchUiNavigationGroupHolder.RemoveGroup();
		}

		public void OnClickedCustomMatchJoinLobby()
		{
			this._buttonBILogger.LogButtonClick(ButtonName.GameModeCustomJoinLobby);
			string value = this._acessCodeUiInput.value;
			GameModesGUI.Log.Debug(string.Format("Checking Custom Match Acess Code \"{0}\"", value));
			this._acessCodeUiInput.isSelected = false;
			if (string.IsNullOrEmpty(this._acessCodeUiInput.value))
			{
				this.ToggleCustomMatchLobbyErrorFeedback(true, Language.Get("GAMEMODE_EMPTYCODE", TranslationContext.MainMenuGui));
				return;
			}
			SingletonMonoBehaviour<CustomMatchController>.Instance.JoinLobby(value, 2);
		}

		public void OnClickedCustomMatchCreateLobby()
		{
			SingletonMonoBehaviour<CustomMatchController>.Instance.CreateLobby((!this._allowSpectatorsToggle.value) ? 0 : 2);
			this._buttonBILogger.LogButtonClick(ButtonName.GameModeCustomCreateLobby);
		}

		private void OnCustomMatchLobbyCreateOrJoinError(bool showErrorText, string errorMsg = null)
		{
			this.ToggleCustomMatchLobbyErrorFeedback(showErrorText, errorMsg);
		}

		private void ToggleCustomMatchLobbyErrorFeedback(bool showFeedback, string errorMsg = null)
		{
			this._customMatchFeedbackBorderInfocodeAcessSprite.gameObject.SetActive(showFeedback);
			this._customMatchFeedbackLabel.gameObject.SetActive(showFeedback);
			if (!showFeedback)
			{
				return;
			}
			if (string.IsNullOrEmpty(errorMsg))
			{
				return;
			}
			this._customMatchFeedbackLabel.text = errorMsg;
		}

		public void OnClickedCustomMatchHideAccessCodeToggle()
		{
			this._acessCodeUiInput.value = this._acessCodeUiInput.value.Trim();
			this._acessCodeUiInput.inputType = ((!this._hideCustomMatchCodeInputToggle.value) ? UIInput.InputType.Standard : UIInput.InputType.Password);
			this._acessCodeUiInput.UpdateLabel();
		}

		private void UpdateAccessCodeInputState()
		{
			bool flag = !string.IsNullOrEmpty(this._acessCodeUiInput.value);
			this._customMatchEnterButton.GetComponent<Collider>().enabled = flag;
			UIButton[] components = this._customMatchEnterButton.GetComponents<UIButton>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].SetState((!flag) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, true);
			}
		}

		public void StartLoadingAndGoToTutorial()
		{
			this._buttonBILogger.LogButtonClick(ButtonName.Tutorial);
			Guid confirmWindowGuid = Guid.NewGuid();
			string key = GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.HasPlayerDoneTutorial() ? "AskPlayAgainTuto" : "AskFirstPlayOnStartGame";
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get(key, TranslationContext.Tutorial),
				ConfirmButtonText = Language.Get("Yes", TranslationContext.GUI),
				OnConfirm = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.StartLoadingAndGoToTutorial();
				},
				RefuseButtonText = Language.Get("No", TranslationContext.GUI),
				OnRefuse = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void BlockPlayer()
		{
			this._matchBlocker.BlockPlayer();
		}

		public bool IsPlayerBlocked()
		{
			return this._matchBlocker.IsBlocked();
		}

		private void OnMatchmakingClientConnection(object sender, MatchConnectionArgs e)
		{
			this.ReturnToMainMenu();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GameModesGUI));

		private readonly MatchBlocker _matchBlocker = new MatchBlocker();

		public GameModeTabs GameMode;

		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("MatchStats")]
		private MatchStatsGui _matchStats;

		[FormerlySerializedAs("rootGameObject")]
		public GameObject RootGameObject;

		[FormerlySerializedAs("gameModesGameObject")]
		public GameObject GameModesGameObject;

		[FormerlySerializedAs("gameSelectedGameObject")]
		public GameObject CustomMatchGroupGameObject;

		[SerializeField]
		[UsedImplicitly]
		private GameModesHoverView _gameModesHoverView;

		[Header("[Animation]")]
		[SerializeField]
		[UsedImplicitly]
		private Animation _mainAnimation;

		[SerializeField]
		[UsedImplicitly]
		private Animation _modesAnimation;

		[SerializeField]
		[UsedImplicitly]
		private Animation _customMatchAnimation;

		[Header("[Game modes buttons]")]
		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("NormalButton")]
		private UIButton _normalButton;

		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("CustomMatchButton")]
		private UIButton _customMatchButton;

		[SerializeField]
		[UsedImplicitly]
		private UIButton _rankedButton;

		[SerializeField]
		[UsedImplicitly]
		private UIButton _rankedInfoButton;

		[SerializeField]
		[UsedImplicitly]
		private UIButton _tutorialButton;

		[Header("[Pre Custom match]")]
		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("CustomMatchCreateButton")]
		private UIButton _customMatchCreateButton;

		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("CustomMatchEnterButton")]
		private UIButton _customMatchEnterButton;

		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("gameDescriptionLabel")]
		private UILabel _gameDescriptionLabel;

		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("playersCountLabel")]
		private UILabel _playersCountLabel;

		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("gameAvgTimeLabel")]
		private UILabel _gameAvgTimeLabel;

		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("CustomMatchGroup")]
		private GameObject _customMatchGroup;

		[SerializeField]
		[UsedImplicitly]
		[FormerlySerializedAs("_acessCodeUIInput")]
		private UIInput _acessCodeUiInput;

		[SerializeField]
		[UsedImplicitly]
		private UILabel _customMatchFeedbackLabel;

		[SerializeField]
		[UsedImplicitly]
		private UI2DSprite _customMatchFeedbackBorderInfocodeAcessSprite;

		[SerializeField]
		[UsedImplicitly]
		private UIToggle _allowSpectatorsToggle;

		[SerializeField]
		[UsedImplicitly]
		private UIToggle _hideCustomMatchCodeInputToggle;

		[SerializeField]
		private UiNavigationGroupHolder _customMatchUiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationSubGroupHolder _customMatchLobbyUiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationSubGroupHolder _customMatchEnterUiNavigationSubGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _customMatchLobbyUiNavigationAxisSelector;

		[SerializeField]
		private UiNavigationAxisSelector _customMatchEnterUiNavigationAxisSelector;

		[SerializeField]
		private GameObject _customMatchEnterGamepadHighlightGameObject;

		[SerializeField]
		private GameObject _customMatchLobbyGamepadHighlightGameObject;

		[Header("[Region GUI]")]
		[SerializeField]
		private RegionGameModeButtonGUI _regionGameModeButtonGui;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		[Inject]
		private IMainMenuPresenterTree _mainMenuPresenterTree;

		[Inject]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[Inject]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		private bool _shouldReturnToMainMenuAfterLeavePreCustom;

		private NGUIPanelAlpha _rootPanel;

		private IDisposable _lobbyUiNavigationEdgeNotifierDisposable;

		private IDisposable _enterUiNavigationEdgeNotifierDisposable;

		private IDisposable _activeDeviceChangeNotifierDisposable;

		private bool _isCustomMatchEnterFocused;
	}
}
