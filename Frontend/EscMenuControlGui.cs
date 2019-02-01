using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class EscMenuControlGui : EscMenuScreenGui
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EscMenuControlGui.OnControlModeChangedDelegate OnControlModeChangedCallback;

		private void OnEnable()
		{
			this._inputToBeReplaced = KeyCode.None.ToString();
			GameHubBehaviour.Hub.Options.Controls.OnKeyChangedCallback += this.OnKeyChangedCallback;
			this.SetupMovementModeToggleButtons();
		}

		private void OnDisable()
		{
			this._inputToBeReplaced = KeyCode.None.ToString();
			GameHubBehaviour.Hub.Options.Controls.OnKeyChangedCallback -= this.OnKeyChangedCallback;
			this.InputScreenVisible = EscMenuControlGui.InputScreen.None;
		}

		private void OnKeyChangedCallback(ControlAction pObj)
		{
			this.UpdateControls();
			if (cInput.LastFrameForbiddenKeyCode == KeyCode.None)
			{
				return;
			}
			string text = Language.Get("OPTIONS_INPUT_INVALID", TranslationSheets.Options);
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = string.Format(text, text, this._inputToBeReplaced),
				OkButtonText = Language.Get("OPTIONS_INPUT_INVALID_CONFIRMATION", TranslationSheets.Options),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void UpdateControls()
		{
			this.ReloadCurrent();
			this.ResetControlButtons();
		}

		public override void Show()
		{
			base.Show();
			this.controller_buttons_Panel.SetActive(true);
			this.keyboardMousePanel.SetActive(false);
			this.joystickPanel.SetActive(false);
			this.BackButtonGameObject.SetActive(false);
			this.scrollbarPanel.SetActive(false);
			this.leaveButton.SetActive(true);
			this.InputScreenVisible = EscMenuControlGui.InputScreen.None;
			if (GameHubBehaviour.Hub.Options.Controls.isBusy)
			{
				GameHubBehaviour.Hub.Options.Controls.CancelChangeInput();
				this._inputToBeReplaced = KeyCode.None.ToString();
				this.ReloadCurrent();
			}
			this.ResetControlButtons();
		}

		public override void Hide()
		{
			base.Hide();
			if (GameHubBehaviour.Hub.Options.Controls.isBusy)
			{
				this._inputToBeReplaced = KeyCode.None.ToString();
				GameHubBehaviour.Hub.Options.Controls.CancelChangeInput();
			}
			this.controller_buttons_Panel.SetActive(true);
			this.keyboardMousePanel.SetActive(false);
			this.joystickPanel.SetActive(false);
			this.BackButtonGameObject.SetActive(false);
			this.scrollbarPanel.SetActive(false);
			this.leaveButton.SetActive(true);
			this.InputScreenVisible = EscMenuControlGui.InputScreen.None;
		}

		public void ShowKeyBoardMousePanel()
		{
			this.controller_buttons_Panel.SetActive(false);
			this.keyboardMousePanel.SetActive(true);
			this.joystickPanel.SetActive(false);
			this.BackButtonGameObject.SetActive(true);
			this.scrollbarPanel.SetActive(true);
			this.leaveButton.SetActive(false);
			this.InputScreenVisible = EscMenuControlGui.InputScreen.MouseKeyboard;
		}

		public void ShowJoystickPanel()
		{
			this.controller_buttons_Panel.SetActive(false);
			this.keyboardMousePanel.SetActive(false);
			this.joystickPanel.SetActive(true);
			this.BackButtonGameObject.SetActive(true);
			this.scrollbarPanel.SetActive(true);
			this.InputScreenVisible = EscMenuControlGui.InputScreen.Joystick;
		}

		public override void ReloadCurrent()
		{
			this.GameMovementModePopup.items = new List<string>(GameHubBehaviour.Hub.Options.Game.MovementModeNames);
			this.GameMovementModePopup.value = this.GameMovementModePopup.items[GameHubBehaviour.Hub.Options.Game.MovementModeIndex];
			for (int i = 0; i < this._keyboardGuiKeys.Length; i++)
			{
				this.SetupKeyLabel(this._keyboardGuiKeys[i]);
			}
			this._inputToBeReplaced = KeyCode.None.ToString();
		}

		private void SetupKeyLabel(EscMenuControlGui.ControlGuiKey keyboardGuiKey)
		{
			this.SetupKeyLabel(keyboardGuiKey.TitleLabel, keyboardGuiKey.NameLabel, keyboardGuiKey.KeyControlAction, keyboardGuiKey.KeyConfigButton.enabled, keyboardGuiKey.ActionInputType);
		}

		public override void ResetDefault()
		{
			GameHubBehaviour.Hub.Options.Controls.ResetDefault();
			GameHubBehaviour.Hub.Options.Game.ResetMovementModeDefault();
			GameHubBehaviour.Hub.Options.Game.ResetInverseReverseDefault();
		}

		public void ResetPrimaryDefault()
		{
			GameHubBehaviour.Hub.Options.Controls.ResetPrimaryDefault();
		}

		public void ResetSecundaryDefault()
		{
			GameHubBehaviour.Hub.Options.Controls.ResetSecundaryDefault();
		}

		private void ResetControlButtons()
		{
			this.EnableAllButtons(true);
			this.SetupMovementModeToggleButtons();
		}

		private void EnableControlGuiKeyButton(EscMenuControlGui.ControlGuiKey controlGuiKey, bool isEnabled)
		{
			controlGuiKey.KeyConfigButton.isEnabled = isEnabled;
		}

		private void EnableAllButtons(bool isEnabled)
		{
			for (int i = 0; i < this._keyboardGuiKeys.Length; i++)
			{
				this.EnableControlGuiKeyButton(this._keyboardGuiKeys[i], isEnabled);
			}
		}

		[Obsolete]
		public void OnGameMovementModePopupChanged()
		{
			int num = this.GameMovementModePopup.items.FindIndex((string i) => i == this.GameMovementModePopup.value);
			GameHubBehaviour.Hub.Options.Game.MovementModeIndex = num;
			GameHubBehaviour.Hub.Options.Game.Apply();
			if (this.OnControlModeChangedCallback != null)
			{
				this.OnControlModeChangedCallback((CarInput.DrivingStyleKind)num);
			}
		}

		public void ClickOnKeyboardModeSelected()
		{
			this.MovementModeSelected(CarInput.DrivingStyleKind.FollowMouse);
		}

		public void ClickOnJoystickModeSelected()
		{
			this.MovementModeSelected(CarInput.DrivingStyleKind.Controller);
		}

		private void MovementModeSelected(CarInput.DrivingStyleKind movementMode)
		{
			GameHubBehaviour.Hub.Options.Game.MovementModeIndex = (int)movementMode;
			GameHubBehaviour.Hub.Options.Game.Apply();
			if (this.OnControlModeChangedCallback != null)
			{
				this.OnControlModeChangedCallback(movementMode);
			}
			this.UpdateMovementModeToggleColliders();
			if (movementMode == CarInput.DrivingStyleKind.Controller)
			{
				Guid confirmWindowGuid = Guid.NewGuid();
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = confirmWindowGuid,
					QuestionText = Language.Get("CONTROLLER_SELECTION_WARNING", TranslationSheets.Options).Replace("\\n", "\n"),
					OkButtonText = Language.Get("Ok", TranslationSheets.GUI),
					OnOk = delegate()
					{
						GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					}
				};
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			}
		}

		private void SetupMovementModeToggleButtons()
		{
			CarInput.DrivingStyleKind movementModeIndex = (CarInput.DrivingStyleKind)GameHubBehaviour.Hub.Options.Game.MovementModeIndex;
			if (movementModeIndex == CarInput.DrivingStyleKind.Controller)
			{
				this.ControlToggle.Set(true, true);
			}
			else
			{
				this.KeyboardToggle.Set(true, true);
			}
			this.UpdateMovementModeToggleColliders();
		}

		private void UpdateMovementModeToggleColliders()
		{
			CarInput.DrivingStyleKind movementModeIndex = (CarInput.DrivingStyleKind)GameHubBehaviour.Hub.Options.Game.MovementModeIndex;
			this.KeyboardToggle.GetComponent<BoxCollider>().enabled = (movementModeIndex != CarInput.DrivingStyleKind.FollowMouse);
			this.KeyboardToggle.GetComponent<UIButton>().SetState(UIButtonColor.State.Normal, true);
		}

		public void OnControlsMovementForwardPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.MovementForward);
		}

		public void OnControlsMovementLeftPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.MovementLeft);
		}

		public void OnControlsMovementRightPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.MovementRight);
		}

		public void OnControlsMovementBackwardPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.MovementBackward);
		}

		public void OnControlsGadgetBoostPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.GadgetBoost);
		}

		public void OnControlsGadgetBasicPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.GadgetBasic);
		}

		public void OnControlsGadget0Pressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.Gadget0);
		}

		public void OnControlsGadget1Pressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.Gadget1);
		}

		public void OnControlsGadgetBombPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.GadgetDropBomb);
		}

		public void OnControlsSprayPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.Spray);
		}

		public void OnControlsGadgetQuickChatPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.Ping);
		}

		public void OnControlsChatAllPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.ChatAll);
		}

		public void OnControlsChatTeamPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.ChatTeam);
		}

		public void OnControlsChatSendPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.ChatSend);
		}

		public void OnControlsGUIOpenShopPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.GUIOpenShop);
		}

		public void OnControlsGUIOpenScorePressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.GUIOpenScore);
		}

		public void OnControlsGUIPushToTalkPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.PushToTalk);
		}

		public void OnControlsGUIPausePressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.MouseKeyboard, ControlAction.Pause);
		}

		public void OnJoyMovementBackwardPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.MovementBackward);
		}

		public void OnJoyGadgetBoostPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.GadgetBoost);
		}

		public void OnJoyGadgetBasicPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.GadgetBasic);
		}

		public void OnJoyGadget0Pressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.Gadget0);
		}

		public void OnJoyGadget1Pressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.Gadget1);
		}

		public void OnJoyGadgetBombPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.GadgetDropBomb);
		}

		public void OnJoyGadgetQuickChatPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.Ping);
		}

		public void OnJoyGUIOpenShopPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.GUIOpenShop);
		}

		public void OnJoyGUIOpenScorePressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.GUIOpenScore);
		}

		public void OnJoyGUIPushToTalkPressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.PushToTalk);
		}

		public void OnJoyGUIPausePressed()
		{
			this.PrepareToChangeInput(EscMenuControlGui.InputScreen.Joystick, ControlAction.Pause);
		}

		private void PrepareToChangeInput(EscMenuControlGui.InputScreen inputScreen, ControlAction controlAction)
		{
			if (cInput.scanning)
			{
				return;
			}
			this.EnableAllButtons(false);
			EscMenuControlGui.ControlGuiKey[] array;
			if (inputScreen == EscMenuControlGui.InputScreen.MouseKeyboard)
			{
				this._inputToBeReplaced = ControlOptions.GetText(controlAction, ControlOptions.ControlActionInputType.Primary);
				GameHubBehaviour.Hub.Options.Controls.ChangePrimaryKey(controlAction);
				array = this._keyboardGuiKeys;
			}
			else
			{
				this._inputToBeReplaced = ControlOptions.GetText(controlAction, ControlOptions.ControlActionInputType.Secondary);
				GameHubBehaviour.Hub.Options.Controls.ChangeSecondaryKey(controlAction);
				array = this._joystickGuiKeys;
			}
			this._actionBeingReplaced = controlAction;
			foreach (EscMenuControlGui.ControlGuiKey controlGuiKey in array)
			{
				if (controlGuiKey.KeyControlAction == controlAction)
				{
					this.SetupWaitKeyLabel(controlGuiKey.NameLabel);
					return;
				}
			}
			HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("Control action not found: {0}", controlAction), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			this.EnableAllButtons(true);
		}

		private void SetupKeyLabel(UILabel titleLabel, UILabel keyLabel, ControlAction currentControlAction, bool isButtonEnabled, ControlOptions.ControlActionInputType inputType = ControlOptions.ControlActionInputType.Primary)
		{
			bool flag = this._inputToBeReplaced != KeyCode.None.ToString();
			bool flag2 = ControlOptions.GetText(currentControlAction, inputType) == KeyCode.None.ToString();
			bool flag3 = currentControlAction == this._actionBeingReplaced;
			if (flag && flag2 && !flag3)
			{
				if (inputType == ControlOptions.ControlActionInputType.Primary)
				{
					GameHubBehaviour.Hub.Options.Controls.ChangePrimaryKeyDirectly(currentControlAction, this._inputToBeReplaced);
				}
				else
				{
					GameHubBehaviour.Hub.Options.Controls.ChangeSecondaryKeyDirectly(currentControlAction, this._inputToBeReplaced);
				}
				Guid windowGuid = default(Guid);
				string format = Language.Get("SWAP_COMMANDS_INFO", TranslationSheets.Options).Replace("\\n", "\n");
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = windowGuid,
					QuestionText = string.Format(format, titleLabel.text, this._inputToBeReplaced),
					OkButtonText = Language.Get("SWAP_COMMANDS_CONFIRMATION", TranslationSheets.Options),
					OnOk = delegate()
					{
						GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(windowGuid);
					}
				};
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			}
			else if (flag2 && flag3)
			{
				GameHubBehaviour.Hub.Options.Controls.CancelChangeInput();
			}
			keyLabel.text = ControlOptions.GetTextlocalized(currentControlAction, inputType);
			if (keyLabel.text == Language.Get(KeyCode.None.ToString(), TranslationSheets.Inputs))
			{
				GameHubBehaviour.Hub.Options.Controls.ResetDefault(currentControlAction, inputType);
				keyLabel.text = ControlOptions.GetTextlocalized(currentControlAction, inputType);
				if (keyLabel.text == Language.Get(KeyCode.None.ToString(), TranslationSheets.Inputs))
				{
					HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("GD: Missing key at ControlSettings. Action: {0} InputType: {1}", currentControlAction, inputType), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
					if (isButtonEnabled)
					{
						keyLabel.color = Color.red;
					}
					else
					{
						keyLabel.color = this.disabledColor;
					}
					return;
				}
			}
			if (isButtonEnabled)
			{
				keyLabel.color = Color.white;
			}
			else
			{
				keyLabel.color = this.disabledColor;
			}
		}

		private void SetupWaitKeyLabel(UILabel keyLabel)
		{
			keyLabel.text = "...";
			keyLabel.color = Color.white;
		}

		[Header("GENERAL")]
		public GameObject controller_buttons_Panel;

		public GameObject keyboardMousePanel;

		public GameObject joystickPanel;

		public GameObject scrollbarPanel;

		public GameObject leaveButton;

		public GameObject BackButtonGameObject;

		public EscMenuControlGui.InputScreen InputScreenVisible;

		public UIToggle KeyboardToggle;

		public UIToggle ControlToggle;

		[Header("[COMMANDS - Keyboard]")]
		[SerializeField]
		private EscMenuControlGui.ControlGuiKey[] _keyboardGuiKeys;

		[Header("[COMMANDS - Joystick]")]
		[SerializeField]
		private EscMenuControlGui.ControlGuiKey[] _joystickGuiKeys;

		[SerializeField]
		private Color disabledColor;

		public UIPopupList GameMovementModePopup;

		private string _inputToBeReplaced;

		private ControlAction _actionBeingReplaced;

		public enum InputScreen
		{
			None,
			MouseKeyboard,
			Joystick
		}

		[Serializable]
		private class ControlGuiKey
		{
			public ControlAction KeyControlAction;

			public ControlOptions.ControlActionInputType ActionInputType;

			public UILabel TitleLabel;

			public UILabel NameLabel;

			public UIButton KeyConfigButton;
		}

		public delegate void OnControlModeChangedDelegate(CarInput.DrivingStyleKind drivingStyleKind);
	}
}
