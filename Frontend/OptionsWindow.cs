using System;
using System.Diagnostics;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class OptionsWindow : HudWindow
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event OptionsWindow.OnCloseDelegate OnCloseCallback;

		protected void Awake()
		{
			this.WindowGameObject.SetActive(false);
		}

		public void Show(OptionsWindow.OptionScreenKind screen, OptionsWindow.OnCloseDelegate onCloseCallback)
		{
			base.SetWindowVisibility(true);
			this._confirmWindowGuid = Guid.Empty;
			this._currentScreen = screen;
			this.OnCloseCallback = onCloseCallback;
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.ListenToResolutionChange += this.ScreenResolutionOnListenToResolutionChange;
		}

		private void ScreenResolutionOnListenToResolutionChange()
		{
			this.Panel.Invalidate(true);
		}

		public override bool CanBeHiddenByEscKey()
		{
			return !cInput.scanning && base.CanBeHiddenByEscKey() && !GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Visible;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (visible)
			{
				this.ChangeTab(OptionsWindow.OptionScreenKind.Graphic);
				this.GraphicsToggle.value = true;
				this.CommandsToggle.value = false;
				this.InterfaceToggle.value = false;
				this.AudioToggle.value = false;
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
				GameHubBehaviour.Hub.Options.Game.GetterInfoForBILog();
			}
			else
			{
				GameHubBehaviour.Hub.Options.Game.WriteBILogs();
				GameHubBehaviour.Hub.PlayerPrefs.SaveNow();
				GameHubBehaviour.Hub.CursorManager.Pop();
				if (GameHubBehaviour.Hub.State.Loading)
				{
					this.CloseWindow();
				}
			}
		}

		public override void AnimationOnWindowExit()
		{
			base.AnimationOnWindowExit();
			this.CloseWindow();
		}

		private void CloseWindow()
		{
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.ListenToResolutionChange -= this.ScreenResolutionOnListenToResolutionChange;
			if (this.OnCloseCallback != null)
			{
				this.OnCloseCallback(this);
			}
			this.OnCloseCallback = null;
		}

		public void HideOptionsWindow()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.None);
			base.SetWindowVisibility(false);
		}

		public void OnGraphicsTabButtonPressed()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.Graphic);
		}

		public void OnGameTabButtonPressed()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.Game);
		}

		public void OnControlTabButtonPressed()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.Control);
		}

		public void OnAudioTabButtonPressed()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.Audio);
		}

		public void OnResetDefaultButtonPressed()
		{
			this.TryResetDefault();
		}

		private void TryResetDefault()
		{
			string key = string.Empty;
			switch (this._currentScreen)
			{
			case OptionsWindow.OptionScreenKind.Audio:
				key = "ResetAudioDefaultValues";
				break;
			case OptionsWindow.OptionScreenKind.Control:
			{
				EscMenuControlGui.InputScreen inputScreenVisible = this.ControlGui.InputScreenVisible;
				if (inputScreenVisible != EscMenuControlGui.InputScreen.None)
				{
					if (inputScreenVisible != EscMenuControlGui.InputScreen.MouseKeyboard)
					{
						if (inputScreenVisible == EscMenuControlGui.InputScreen.Joystick)
						{
							key = "ResetJoystickControlDefaultValues";
						}
					}
					else
					{
						key = "ResetMouseControlDefaultValues";
					}
				}
				else
				{
					key = "ResetAllControlDefaultValues";
				}
				break;
			}
			case OptionsWindow.OptionScreenKind.Game:
				key = "ResetGameDefaultValues";
				break;
			case OptionsWindow.OptionScreenKind.Graphic:
				key = "ResetGraphicDefaultValues";
				break;
			default:
				OptionsWindow.Log.WarnFormat("TryResetDefault in an invalid screen: [{0}]", new object[]
				{
					this._currentScreen
				});
				return;
			}
			this._confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = this._confirmWindowGuid,
				QuestionText = Language.Get(key, "GUI"),
				ConfirmButtonText = Language.Get("YES", "GUI"),
				OnConfirm = delegate()
				{
					this.ResetDefaultConfirm();
				},
				RefuseButtonText = Language.Get("NO", "GUI"),
				OnRefuse = delegate()
				{
					this.ResetDefaultRefuse();
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void ResetDefaultRefuse()
		{
			this.HideConfirmWindow();
			this.ChangeTab(this._currentScreen);
		}

		private void ResetDefaultConfirm()
		{
			this.HideConfirmWindow();
			this.CurrentResetDefault();
			this.ChangeTab(this._currentScreen);
		}

		private void CurrentResetDefault()
		{
			switch (this._currentScreen)
			{
			case OptionsWindow.OptionScreenKind.Audio:
				this.AudioGui.ResetDefault();
				break;
			case OptionsWindow.OptionScreenKind.Control:
			{
				EscMenuControlGui.InputScreen inputScreenVisible = this.ControlGui.InputScreenVisible;
				if (inputScreenVisible != EscMenuControlGui.InputScreen.None)
				{
					if (inputScreenVisible != EscMenuControlGui.InputScreen.MouseKeyboard)
					{
						if (inputScreenVisible == EscMenuControlGui.InputScreen.Joystick)
						{
							this.ControlGui.ResetSecundaryDefault();
						}
					}
					else
					{
						this.ControlGui.ResetPrimaryDefault();
					}
				}
				else
				{
					this.ControlGui.ResetDefault();
				}
				break;
			}
			case OptionsWindow.OptionScreenKind.Game:
				this.GameGui.ResetDefault();
				break;
			case OptionsWindow.OptionScreenKind.Graphic:
				this.GraphicGui.ResetDefault();
				break;
			}
		}

		private void ChangeTab(OptionsWindow.OptionScreenKind newScreen)
		{
			this._currentScreen = newScreen;
			EscMenuControlGui.InputScreen inputScreenVisible = this.ControlGui.InputScreenVisible;
			this.GraphicsToggle.value = false;
			this.InterfaceToggle.value = false;
			this.AudioToggle.value = false;
			this.CommandsToggle.value = false;
			this.GraphicGui.Hide();
			this.ControlGui.Hide();
			this.GameGui.Hide();
			this.AudioGui.Hide();
			switch (this._currentScreen)
			{
			case OptionsWindow.OptionScreenKind.Audio:
				this._currentToggle = this.AudioToggle;
				this.AudioGui.Show();
				break;
			case OptionsWindow.OptionScreenKind.Control:
				this._currentToggle = this.CommandsToggle;
				this.ControlGui.Show();
				if (inputScreenVisible == EscMenuControlGui.InputScreen.MouseKeyboard)
				{
					this.ControlGui.ShowKeyBoardMousePanel();
				}
				else if (inputScreenVisible == EscMenuControlGui.InputScreen.Joystick)
				{
					this.ControlGui.ShowJoystickPanel();
				}
				break;
			case OptionsWindow.OptionScreenKind.Game:
				this._currentToggle = this.InterfaceToggle;
				this.GameGui.Show();
				break;
			case OptionsWindow.OptionScreenKind.Graphic:
				this._currentToggle = this.GraphicsToggle;
				this.GraphicGui.Show();
				break;
			}
			this._currentToggle.value = true;
			this.ReloadCurrent();
		}

		private void ReloadCurrent()
		{
			switch (this._currentScreen)
			{
			case OptionsWindow.OptionScreenKind.Audio:
				this.AudioGui.ReloadCurrent();
				break;
			case OptionsWindow.OptionScreenKind.Control:
				this.ControlGui.ReloadCurrent();
				break;
			case OptionsWindow.OptionScreenKind.Game:
				this.GameGui.ReloadCurrent();
				break;
			case OptionsWindow.OptionScreenKind.Graphic:
				this.GraphicGui.ReloadCurrent();
				break;
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (this._confirmWindowGuid != Guid.Empty)
			{
				this.HideConfirmWindow();
			}
		}

		private void HideConfirmWindow()
		{
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(this._confirmWindowGuid);
			this._confirmWindowGuid = Guid.Empty;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(OptionsWindow));

		public EscMenuAudioGui AudioGui;

		public EscMenuControlGui ControlGui;

		public EscMenuGameGui GameGui;

		public EscMenuGraphicGui GraphicGui;

		public UIToggle GraphicsToggle;

		public UIToggle InterfaceToggle;

		public UIToggle CommandsToggle;

		public UIToggle AudioToggle;

		public new UIPanel Panel;

		private OptionsWindow.OptionScreenKind _currentScreen;

		private UIToggle _currentToggle;

		private Guid _confirmWindowGuid;

		public delegate void OnCloseDelegate(OptionsWindow optionsWindow);

		public enum OptionScreenKind
		{
			Starting,
			Audio,
			Control,
			Game,
			Graphic,
			None
		}
	}
}
