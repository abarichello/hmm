using System;
using System.Diagnostics;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Tutorial;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class EscMenuGui : HudWindow
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EscMenuControlGui.OnControlModeChangedDelegate OnControlModeChangedCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ListenToOptionsCloseCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> OnEscMenuVisibleChanged;

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
		}

		public bool IsOptionsWindowVisible()
		{
			return this._optionsWindow != null && this._optionsWindow.IsWindowVisible();
		}

		public override bool CanBeHiddenByEscKey()
		{
			return !cInput.scanning && base.CanBeHiddenByEscKey();
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
			}
			return !GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Visible && HudWindowManager.Instance.State != HudWindowManager.GuiGameState.Transitioning && !cInput.scanning;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (visible)
			{
				this.ShowEnteringMenu();
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
				if (this.OnEscMenuVisibleChanged != null)
				{
					this.OnEscMenuVisibleChanged(true);
				}
			}
			else
			{
				GameHubBehaviour.Hub.CursorManager.Pop();
				if (this.OnEscMenuVisibleChanged != null && !this._optionsWindow.IsWindowVisible())
				{
					this.OnEscMenuVisibleChanged(false);
				}
			}
		}

		public void ShowOptionsWindow()
		{
			if (this.IsVisible)
			{
				this.OpenOptionsWindow(OptionsWindow.OptionScreenKind.Graphic);
				base.SetWindowVisibility(false);
			}
		}

		private void ShowEnteringMenu()
		{
			this.SetupMenuLayout(this.GetMenuLayout());
			if (SocialModalGUI.Current.IsWindowVisible())
			{
				SingletonMonoBehaviour<PanelController>.Instance.TryCloseModalWindow<SocialModalGUI>();
			}
		}

		private EscMenuGui.MenuLayout GetMenuLayout()
		{
			EscMenuGui.MenuLayout result = EscMenuGui.MenuLayout.QuitGame;
			HudWindowManager.GuiGameState state = HudWindowManager.Instance.State;
			if (state == HudWindowManager.GuiGameState.PickScreen || state == HudWindowManager.GuiGameState.Game)
			{
				if (GameHubBehaviour.Hub.User.IsNarrator)
				{
					result = EscMenuGui.MenuLayout.LeaveMatch;
				}
				else if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
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
					this.LeaveMatchButton.SetActive(true);
				}
			}
			else
			{
				this.QuitButton.SetActive(true);
			}
		}

		public void QuitApplication()
		{
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenCloseGameConfirmWindow(delegate
			{
				try
				{
					if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
					{
						GameHubBehaviour.Hub.Server.ClientSendPlayerDisconnectInfo();
						GameHubBehaviour.Hub.Swordfish.Msg.Cleanup();
					}
				}
				catch (Exception ex)
				{
				}
			});
		}

		private void OpenOptionsWindow(OptionsWindow.OptionScreenKind optionScreenKind)
		{
			this._optionsWindow.Show(optionScreenKind, new OptionsWindow.OnCloseDelegate(this.OnOptionsWindowClose));
			this._optionsWindow.ControlGui.OnControlModeChangedCallback += this.OnControlModeChangedCallback;
		}

		private void OnOptionsWindowClose(OptionsWindow optionsWindow)
		{
			this._optionsWindow.ControlGui.OnControlModeChangedCallback -= this.OnControlModeChangedCallback;
			if (this.ListenToOptionsCloseCallback != null)
			{
				this.ListenToOptionsCloseCallback();
			}
			if (this.OnEscMenuVisibleChanged != null)
			{
				this.OnEscMenuVisibleChanged(false);
			}
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
			this.TryDisconnect(Language.Get("DISCONNECT_HINT", TranslationSheets.MainMenuGui));
		}

		public void TryDisconnectFromTutorial()
		{
			this.TryDisconnect(Language.Get("HINT_EXIT_TUTORIAL", TranslationSheets.Tutorial));
		}

		private void TryDisconnect(string questionText)
		{
			this.SetLeaveButtonColliders(false);
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = questionText,
				ConfirmButtonText = Language.Get("YES", "GUI"),
				OnConfirm = delegate()
				{
					this.DisconnectConfirm(confirmWindowGuid);
				},
				RefuseButtonText = Language.Get("NO", "GUI"),
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

		private void DisconnectConfirm(Guid confirmWindowGuid)
		{
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
			Game game = GameHubBehaviour.Hub.State.Current as Game;
			if (game != null)
			{
				game.EndMatch();
				game.ClearBackToMain();
				return;
			}
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

		private static readonly BitLogger Log = new BitLogger(typeof(EscMenuGui));

		public GameObject QuitButton;

		public GameObject LeaveMatchButton;

		public GameObject LeaveTutorialButton;

		[SerializeField]
		protected string WindowOptionsBundleName = "gui_window_options";

		[SerializeField]
		private OptionsWindow _optionsWindow;

		private enum MenuLayout
		{
			QuitGame,
			LeaveMatch,
			LeaveTutorial
		}
	}
}
