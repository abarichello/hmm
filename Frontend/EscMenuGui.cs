using System;
using System.Collections.Generic;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.CharacterHelp.Presenting;
using HeavyMetalMachines.HostingPlatforms;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Login;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.MuteSystem;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.GenericConfirmWindow;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.ShopPopup.Presenting.Business;
using HeavyMetalMachines.Social.Buttons;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tutorial;
using HeavyMetalMachines.VFX;
using Hoplon.Input.UiNavigation;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class EscMenuGui : HudWindow, IOptionsPresenter
	{
		private IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public bool Visible
		{
			get
			{
				return this.IsVisible || this.IsOptionsWindowVisible();
			}
		}

		public IObservable<bool> VisibilityChanged()
		{
			return this._visibilityObservation;
		}

		public void Show()
		{
			base.SetWindowVisibility(true);
		}

		public void SetCharacterSelectionActive()
		{
			this._isCharacterSelectionActive = true;
		}

		public void SetCharacterSelectionInactive()
		{
			this._isCharacterSelectionActive = false;
		}

		public int CurrentWindowHash
		{
			get
			{
				if (this._optionsWindow != null)
				{
					return this._optionsWindow.GetHashCode();
				}
				return this.GetHashCode();
			}
		}

		protected void Awake()
		{
			this.WindowGameObject.SetActive(false);
			this._changeUserButton.UIButton = this._changeUserGameButton;
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<bool>(Observable.Repeat<bool>(Observable.Do<bool>(Observable.ContinueWith<Unit, bool>(Observable.Do<Unit>(Observable.First<Unit>(this._changeUserButton.OnClick()), delegate(Unit _)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.EscMenuChangeUser);
			}), (Unit _) => this.ShowChangeUserConfirmation()), new Action<bool>(this.EndSession)))), this);
			this.SetupSocialButton(this._instagramButton, delegate
			{
				this._diContainer.Resolve<ISocialButtonsOpenUrl>().OpenInstagram();
			});
			this.SetupSocialButton(this._facebookButton, delegate
			{
				this._diContainer.Resolve<ISocialButtonsOpenUrl>().OpenFacebook();
			});
			this.SetupSocialButton(this._vkButton, delegate
			{
				this._diContainer.Resolve<ISocialButtonsOpenUrl>().OpenVk();
			});
			this.SetupSocialButton(this._twitterButton, delegate
			{
				this._diContainer.Resolve<ISocialButtonsOpenUrl>().OpenTwitter();
			});
		}

		private void SetupSocialButton(IButton button, Action onClickAction)
		{
			ObservableExtensions.Subscribe<bool>(Observable.Repeat<bool>(Observable.ContinueWith<Unit, bool>(Observable.First<Unit>(button.OnClick()), (Unit _) => this.ExecutePossiblyUgcRestrictedAction(onClickAction))));
		}

		private IObservable<bool> ExecutePossiblyUgcRestrictedAction(Action action)
		{
			return Observable.Do<bool>(Observable.First<bool>(this._diContainer.Resolve<IGetUGCRestrictionIsEnabled>().OfferToChangeGlobalRestriction(), (bool isRestricted) => !isRestricted), delegate(bool _)
			{
				action();
			});
		}

		private IObservable<bool> ShowChangeUserConfirmation()
		{
			return this._diContainer.Resolve<IGenericConfirmWindowPresenter>().ShowQuestionWindow(this.GetShowChangeUserConfirmationConfiguration());
		}

		private QuestionConfiguration GetShowChangeUserConfirmationConfiguration()
		{
			return new QuestionConfiguration
			{
				Message = this._translation.Get("FEEDBACK_CHANGE_ACCOUNT_FEEDBACK", TranslationContext.MainMenuGui),
				Title = this._translation.Get("FEEDBACK_TITLE_CHANGE_ACCOUNT_FEEDBACK", TranslationContext.MainMenuGui),
				AcceptMessage = this._translation.Get("Ok", TranslationContext.GUI),
				DeclineMessage = this._translation.Get("Cancel", TranslationContext.GUI)
			};
		}

		private void EndSession(bool hasConfirmed)
		{
			if (!hasConfirmed)
			{
				return;
			}
			this._diContainer.Resolve<IEndSession>().End("User requested chaning user");
		}

		public bool IsOptionsWindowVisible()
		{
			return this._optionsWindow != null && this._optionsWindow.IsWindowVisible();
		}

		public override bool CanBeHiddenByEscKey()
		{
			return !this._optionsControllerTabPresenter.IsBinding() && base.CanBeHiddenByEscKey();
		}

		public void OpenOnControlsScreen()
		{
			this.OpenOptionsWindow(OptionsWindow.OptionScreenKind.Control);
		}

		public override bool CanOpen()
		{
			bool flag = GameHubBehaviour.Hub.State.Current is MainMenu;
			if (flag)
			{
				MainMenuGui stateGuiController = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
				if (stateGuiController.MatchAccept.Visible)
				{
					return false;
				}
				if (this._getShopPopupVisibility.IsVisible())
				{
					return false;
				}
			}
			return !GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Visible && !this._optionsControllerTabPresenter.IsBinding() && HudWindowManager.Instance.State != GuiGameState.Transitioning;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (visible)
			{
				this.ShowEnteringMenu();
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
				if (this._visibilityObservation != null)
				{
					this._visibilityObservation.OnNext(true);
				}
				this.UiNavigationGroupHolder.AddHighPriorityGroup();
			}
			else
			{
				GameHubBehaviour.Hub.CursorManager.Pop();
				if (this._visibilityObservation != null)
				{
					this._visibilityObservation.OnNext(false);
				}
				this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
			}
		}

		public void ShowOptionsWindow()
		{
			if (this.IsVisible)
			{
				this._buttonBILogger.LogButtonClick(ButtonName.Options);
				this.OpenOptionsWindow(OptionsWindow.OptionScreenKind.Game);
				base.SetWindowVisibility(false);
			}
		}

		private void ShowEnteringMenu()
		{
			this.SetupMenuLayout(this.GetMenuLayout());
			this._grid.Reposition();
			if (SocialModalGUI.Current.IsWindowVisible())
			{
				SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<SocialModalGUI>();
			}
		}

		private EscMenuGui.MenuLayout GetMenuLayout()
		{
			EscMenuGui.MenuLayout result = EscMenuGui.MenuLayout.QuitGame;
			GuiGameState guiGameState = HudWindowManager.Instance.State;
			if (this._isCharacterSelectionActive)
			{
				guiGameState = GuiGameState.PickScreen;
			}
			if (guiGameState == GuiGameState.PickScreen || guiGameState == GuiGameState.Game)
			{
				if (GameHubBehaviour.Hub.User.IsNarrator || GameHubBehaviour.Hub.Match.Kind == 6)
				{
					result = EscMenuGui.MenuLayout.LeaveMatch;
				}
				else if (GameHubBehaviour.Hub.Match.LevelIsTutorial() && GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.HasPlayerDoneTutorial())
				{
					result = EscMenuGui.MenuLayout.LeaveTutorial;
				}
			}
			return result;
		}

		private void SetupMenuLayout(EscMenuGui.MenuLayout menuLayout)
		{
			this.QuitButton.SetActive(false);
			this.LeaveMatchButton.SetActive(false);
			this.LeaveTutorialButton.SetActive(false);
			this._muteGameButton.SetActive(false);
			this._pauseGameButton.SetActive(false);
			this._helpButton.SetActive(false);
			this._changeUserGameButton.gameObject.SetActive(false);
			this._socialButtonsGameObject.SetActive(HudWindowManager.Instance.State == GuiGameState.MainMenu);
			if (menuLayout != EscMenuGui.MenuLayout.QuitGame)
			{
				if (menuLayout != EscMenuGui.MenuLayout.LeaveMatch)
				{
					if (menuLayout == EscMenuGui.MenuLayout.LeaveTutorial)
					{
						this.LeaveTutorialButton.SetActive(true);
					}
				}
				else
				{
					this._helpButton.SetActive(this.CanShowHelpButton());
					this._muteGameButton.SetActive(this.CanShowMuteButton());
					this.LeaveMatchButton.SetActive(true);
				}
			}
			else
			{
				if (!Platform.Current.IsConsole())
				{
					this.QuitButton.SetActive(true);
				}
				if (this.CanShowPauseButton())
				{
					this._pauseGameButton.SetActive(true);
					this.ConfigurePauseButton();
				}
				this._muteGameButton.SetActive(this.CanShowMuteButton());
				this._helpButton.SetActive(this.CanShowHelpButton());
				this._changeUserGameButton.gameObject.SetActive(this.CanShowChangeUserButton());
			}
		}

		private bool CanShowChangeUserButton()
		{
			IGetHostPlatform getHostPlatform = this._diContainer.Resolve<IGetHostPlatform>();
			return getHostPlatform.GetCurrent() == 2 && HudWindowManager.Instance.State == GuiGameState.MainMenu;
		}

		private bool CanShowHelpButton()
		{
			return !GameHubBehaviour.Hub.Match.LevelIsTutorial() && HudWindowManager.Instance.State == GuiGameState.Game && !GameHubBehaviour.Hub.Match.MatchOver;
		}

		private bool CanShowMuteButton()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial() || HudWindowManager.Instance.State != GuiGameState.Game || GameHubBehaviour.Hub.Match.MatchOver)
			{
				return false;
			}
			int num = (GameHubBehaviour.Hub.Match.Kind != 4) ? 0 : EscMenuGui.CountOtherPlayers(GameHubBehaviour.Hub.Players.Narrators);
			int num2 = EscMenuGui.CountOtherPlayers(GameHubBehaviour.Hub.Players.Players) + num;
			return num2 > 0;
		}

		private static int CountOtherPlayers(List<PlayerData> players)
		{
			int num = 0;
			PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
			foreach (PlayerData playerData in players)
			{
				if (currentPlayerData == null || playerData.PlayerId != currentPlayerData.PlayerId)
				{
					num++;
				}
			}
			return num;
		}

		private bool CanShowPauseButton()
		{
			return !GameHubBehaviour.Hub.Match.LevelIsTutorial() && HudWindowManager.Instance.State == GuiGameState.Game && !GameHubBehaviour.Hub.Match.MatchOver;
		}

		public void QuitApplication()
		{
			this._buttonBILogger.LogButtonClick(ButtonName.EscMenuQuitAppPreConfirmation);
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenCloseGameConfirmWindow(delegate
			{
				this._buttonBILogger.LogButtonClick(ButtonName.EscMenuQuitAppConfirm);
				base.SetWindowVisibility(false);
				try
				{
					if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
					{
						GameHubBehaviour.Hub.Server.ClientSendPlayerDisconnectInfo();
					}
				}
				catch (Exception ex)
				{
					EscMenuGui.Log.DebugFormat("Exception while cleaning swordfish. Msg: {0}", new object[]
					{
						ex.Message
					});
				}
			});
		}

		private void OpenOptionsWindow(OptionsWindow.OptionScreenKind optionScreenKind)
		{
			this._optionsWindow.Show(optionScreenKind, null);
			if (this._visibilityObservation != null)
			{
				this._visibilityObservation.OnNext(false);
			}
		}

		public void HideOptionsWindow()
		{
			this._optionsWindow.HideOptionsWindow();
		}

		private void ConfigurePauseButton()
		{
			bool isGamePaused = PauseController.Instance.IsGamePaused;
			if (isGamePaused)
			{
				this._pauseLabel.text = this._translation.Get("GUI_RETURN_TITLE_BUTTON", TranslationContext.GUI);
			}
			else
			{
				this._pauseLabel.text = this._translation.Get("GUI_PAUSE_TITLE_BUTTON", TranslationContext.GUI);
			}
			this._pauseIconGameObject.SetActive(!isGamePaused);
			this._unpauseIconGameObject.SetActive(isGamePaused);
		}

		public void ShowMuteSystemWindow()
		{
			this._buttonBILogger.LogButtonClick(ButtonName.EscMenuMuteSystem);
			this._diContainer.Resolve<IMuteSystemPresenter>().Show();
			base.SetWindowVisibility(false);
		}

		public void ShowHelpWindow()
		{
			ICharacterHelpPresenter characterHelpPresenter = this._diContainer.Resolve<ICharacterHelpPresenter>();
			ObservableExtensions.Subscribe<Unit>(characterHelpPresenter.Show());
			base.SetWindowVisibility(false);
		}

		public void PauseGame()
		{
			this._buttonBILogger.LogButtonClick(ButtonName.EscMenuPause);
			PauseController.Instance.DispatchReliable(new byte[0]).TogglePauseServer();
			base.SetWindowVisibility(false);
		}

		public void TryExit()
		{
			this.Exit();
		}

		private void Exit()
		{
		}

		public void TryDisconnectFromGame()
		{
			this._buttonBILogger.LogButtonClick(ButtonName.EscMenuDisconnectPreConfirmation);
			this.TryDisconnect(this._translation.Get("DISCONNECT_HINT", TranslationContext.MainMenuGui), ButtonName.EscMenuDisconnectConfirm);
		}

		public void TryDisconnectFromTutorial()
		{
			this._buttonBILogger.LogButtonClick(ButtonName.EscMenuDisconnectTutorialPreConfirmation);
			this.TryDisconnect(this._translation.Get("HINT_EXIT_TUTORIAL", TranslationContext.Tutorial), ButtonName.EscMenuDisconnectTutorialConfirm);
		}

		private void TryDisconnect(string questionText, ButtonNameInstance confirmationBiButtonName)
		{
			this.SetLeaveButtonColliders(false);
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = questionText,
				ConfirmButtonText = this._translation.Get("YES", TranslationContext.GUI),
				OnConfirm = delegate()
				{
					this.DisconnectConfirm(confirmWindowGuid, confirmationBiButtonName);
				},
				RefuseButtonText = this._translation.Get("NO", TranslationContext.GUI),
				OnRefuse = delegate()
				{
					this.DisconnectRefuse(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void DisconnectRefuse(Guid confirmWindowGuid)
		{
			this.SetLeaveButtonColliders(true);
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
		}

		private void DisconnectConfirm(Guid confirmWindowGuid, ButtonNameInstance confirmationBiButtonName)
		{
			this._buttonBILogger.LogButtonClick(confirmationBiButtonName);
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				TutorialUIController.Instance.DisposeTutorialDialog();
			}
			base.SetWindowVisibility(false);
			GameHubBehaviour.Hub.Server.ClientSendPlayerDisconnectInfo();
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				this.Disconnect();
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
			}
			else
			{
				PlayerCustomWS.ClearCurrentServer(delegate(object x, string res)
				{
					EscMenuGui.Log.DebugFormat("Server cleared, result={0}", new object[]
					{
						res
					});
					GameHubBehaviour.Hub.User.Bag.CurrentServerIp = null;
					GameHubBehaviour.Hub.User.Bag.CurrentMatchId = null;
					GameHubBehaviour.Hub.User.Bag.CurrentGroupId = null;
					GameHubBehaviour.Hub.User.Bag.CurrentPort = 0;
					this.Disconnect();
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}, delegate(object x, Exception ex)
				{
					EscMenuGui.Log.Fatal("Error clearing server from bag.", ex);
					this.Disconnect();
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				});
			}
		}

		public void Disconnect()
		{
			this.SetLeaveButtonColliders(true);
			if (this._horta.Enabled)
			{
				this._horta.Behaviour.QuitPlayback();
				return;
			}
			Game game = GameHubBehaviour.Hub.State.Current as Game;
			if (game != null)
			{
				game.EndMatch();
				game.ClearBackToMain();
				return;
			}
			GameHubBehaviour.Hub.User.Bag.CurrentServerIp = null;
			GameHubBehaviour.Hub.User.Bag.CurrentMatchId = null;
			GameHubBehaviour.Hub.User.Bag.CurrentGroupId = null;
			GameHubBehaviour.Hub.User.Bag.CurrentPort = 0;
			EscMenuGui.Log.DebugFormat("Client Cleared Current server from PlayerBag by disconnect", new object[0]);
			Application.LoadLevel("Void");
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
			EscMenuGui.Log.Debug("client disconnected outside Game State");
			GameHubBehaviour.Hub.State.GotoState(GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.MainMenu), false);
		}

		private void SetLeaveButtonColliders(bool isEnabled)
		{
			this.LeaveMatchButton.GetComponent<Collider>().enabled = isEnabled;
			this.LeaveTutorialButton.GetComponent<Collider>().enabled = isEnabled;
			UIButtonColor.State state = (!isEnabled) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal;
			this.LeaveMatchButton.GetComponent<UIButton>().SetState(state, true);
			this.LeaveTutorialButton.GetComponent<UIButton>().SetState(state, true);
		}

		public override bool IsStackableWithType(Type type)
		{
			return true;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (this._visibilityObservation != null)
			{
				this._visibilityObservation.Dispose();
				this._visibilityObservation = null;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(EscMenuGui));

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IClientButtonBILogger _buttonBILogger;

		[Inject]
		private HORTAComponent _horta;

		[Inject]
		private ILocalizeKey _translation;

		[Inject]
		private IGetShopPopupVisibility _getShopPopupVisibility;

		public GameObject QuitButton;

		public GameObject LeaveMatchButton;

		public GameObject LeaveTutorialButton;

		[SerializeField]
		private GameObject _helpButton;

		[SerializeField]
		private GameObject _muteGameButton;

		[SerializeField]
		private GameObject _pauseGameButton;

		[SerializeField]
		private UIButton _changeUserGameButton;

		[SerializeField]
		private UILabel _pauseLabel;

		[SerializeField]
		private GameObject _pauseIconGameObject;

		[SerializeField]
		private GameObject _unpauseIconGameObject;

		[SerializeField]
		private GameObject _socialButtonsGameObject;

		[SerializeField]
		private NGuiButton _instagramButton;

		[SerializeField]
		private NGuiButton _facebookButton;

		[SerializeField]
		private NGuiButton _vkButton;

		[SerializeField]
		private NGuiButton _twitterButton;

		[SerializeField]
		private UIGrid _grid;

		[SerializeField]
		private OptionsWindow _optionsWindow;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[InjectOnClient]
		private IOptionsControllerTabPresenter _optionsControllerTabPresenter;

		private Subject<bool> _visibilityObservation = new Subject<bool>();

		private bool _isCharacterSelectionActive;

		private readonly NGuiButton _changeUserButton = new NGuiButton();

		private enum MenuLayout
		{
			QuitGame,
			LeaveMatch,
			LeaveTutorial
		}
	}
}
