using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Assets.Standard_Assets.Scripts.Infra.KeyBoardLayout;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.VFX.PlotKids.VoiceChat;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	public class ControlOptions : GameHubBehaviour, ICleanupListener
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ControlOptions.OnKeyChangedDelegate OnKeyChangedCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnResetDefaultCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnResetPrimaryDefaultCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnResetSecondaryDefaultCallback;

		public static int lockedControlActions { get; private set; }

		public void Init()
		{
			ControlOptions.instance = this;
			ControlOptions.UnlockAllControlActions();
			HMMPlayerPrefs playerPrefs = GameHubBehaviour.Hub.PlayerPrefs;
			if (ControlOptions.<>f__mg$cache0 == null)
			{
				ControlOptions.<>f__mg$cache0 = new Action(cInput.Init);
			}
			playerPrefs.ExecOnPrefsLoaded(ControlOptions.<>f__mg$cache0);
			GameHubBehaviour.Hub.PlayerPrefs.ExecOnPrefsLoaded(new Action(this.LoadDefaults));
			HMMPlayerPrefs playerPrefs2 = GameHubBehaviour.Hub.PlayerPrefs;
			if (ControlOptions.<>f__mg$cache1 == null)
			{
				ControlOptions.<>f__mg$cache1 = new Action(cInput.OnPrefsLoaded);
			}
			playerPrefs2.ExecOnPrefsLoaded(ControlOptions.<>f__mg$cache1);
			HMMPlayerPrefs playerPrefs3 = GameHubBehaviour.Hub.PlayerPrefs;
			if (ControlOptions.<>f__mg$cache2 == null)
			{
				ControlOptions.<>f__mg$cache2 = new Action(cInput.CleanAxis3);
			}
			playerPrefs3.ExecOnPrefsLoaded(ControlOptions.<>f__mg$cache2);
			if (!GameHubBehaviour.Hub.Net.isTest)
			{
				GameHubBehaviour.Hub.PlayerPrefs.ExecOnPrefsLoaded(new Action(SingletonMonoBehaviour<VoiceChatController>.Instance.LoadPrefs));
			}
			GameHubBehaviour.Hub.PlayerPrefs.ExecOnPrefsWrongVersion(new Action(this.OnPlayerPrefsWrongVersion));
			ControlOptions.inputNGUIIconDictionary.Add("Mouse0", "[ML]");
			ControlOptions.inputNGUIIconDictionary.Add("Mouse1", "[MR]");
			ControlOptions.inputNGUIIconDictionary.Add("Mouse2", "[MC]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button0", "[A]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button1", "[B]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button2", "[X]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button3", "[Y]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button4", "[LB]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button5", "[RB]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button6", "[Back]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button7", "[Start]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button8", "[L3]");
			ControlOptions.inputNGUIIconDictionary.Add("Joystick1Button9", "[R3]");
			ControlOptions.inputNGUIIconDictionary.Add("Joy1 Axis 9+", "[LT]");
			ControlOptions.inputNGUIIconDictionary.Add("Joy1 Axis 10+", "[RT]");
		}

		private void OnPlayerPrefsWrongVersion()
		{
			this.ResetDefault();
			GameHubBehaviour.Hub.PlayerPrefs.SaveNow();
			HMMHub hub = GameHubBehaviour.Hub;
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("PLAYERPREFS_WRONG_VERSION", TranslationSheets.MainMenuGui),
				OkButtonText = Language.Get("PLAYERPREFS_CONFIRM", TranslationSheets.MainMenuGui),
				OnOk = delegate()
				{
					hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void OnDestroy()
		{
			ControlOptions.instance = null;
		}

		private void LoadDefaults()
		{
			cInput.allowDuplicates = this.DefaultSetting.AllowDuplicates;
			if (this.DefaultSetting.Modifiers != null)
			{
				for (int i = 0; i < this.DefaultSetting.Modifiers.Length; i++)
				{
					cInput.AddModifier(this.DefaultSetting.Modifiers[i]);
				}
			}
			if (this.DefaultSetting.ForbiddenKeys != null)
			{
				for (int j = 0; j < this.DefaultSetting.ForbiddenKeys.Length; j++)
				{
					cInput.ForbidKey(this.DefaultSetting.ForbiddenKeys[j]);
				}
			}
			this.LoadFromJson();
			ControlOptions._currentLayout = this.GetCurrentKeyboardLayout();
			ControlOptions._defaultLayout = this.GetDefaultKeyboardLayout();
			for (int k = 0; k < ControlOptions._defaultLayout.Controls.Length; k++)
			{
				Control controlInputFromLayout = this.GetControlInputFromLayout(ControlOptions._defaultLayout.Controls[k], ControlOptions._currentLayout);
				ControlOptions.ControlInputs controlInputs = this.ControlMap(controlInputFromLayout);
				cInput.SetKey(controlInputs.Action, controlInputs.PrimaryKey, controlInputs.SecondaryKey, controlInputs.PrimaryModifier, controlInputs.SecondaryModifier);
			}
			this.ChangeInputLanguage();
		}

		private Control GetControlInputFromLayout(Control defaultControl, KeyboarLayout layout)
		{
			for (int i = 0; i < layout.Controls.Length; i++)
			{
				Control control = layout.Controls[i];
				if (control.Action == defaultControl.Action)
				{
					return control;
				}
			}
			return defaultControl;
		}

		private void ReloadKeys()
		{
			ControlOptions._currentLayout = this.GetCurrentKeyboardLayout();
			for (int i = 0; i < ControlOptions._defaultLayout.Controls.Length; i++)
			{
				Control controlInputFromLayout = this.GetControlInputFromLayout(ControlOptions._defaultLayout.Controls[i], ControlOptions._currentLayout);
				ControlOptions.ControlInputs controlInputs = this.ControlMap(controlInputFromLayout);
				cInput.ReloadDefaultKey(controlInputs.Action, controlInputs.PrimaryKey, controlInputs.SecondaryKey, controlInputs.PrimaryModifier, controlInputs.SecondaryModifier, i);
			}
			this.ChangeInputLanguage();
		}

		private void ChangeInputLanguage()
		{
			int keyboardLayoutId = WindowsKeyboardLayoutDetection.GetKeyboardLayoutId();
			Language.ChangeInputLanguage(WindowsKeyboardLayoutDetection.GetKeyboardLanguageCode(keyboardLayoutId));
		}

		private ControlOptions.ControlInputs ControlMap(Control input)
		{
			string action = input.Action.ToString();
			string text = input.PrimaryKey;
			string text2 = input.PrimaryModifier;
			string text3 = input.SecondaryKey;
			string text4 = input.SecondaryModifier;
			if (string.IsNullOrEmpty(text))
			{
				text = "None";
			}
			if (string.IsNullOrEmpty(text2))
			{
				text2 = text;
			}
			if (string.IsNullOrEmpty(text3))
			{
				text3 = "None";
			}
			if (string.IsNullOrEmpty(text4))
			{
				text4 = text3;
			}
			return new ControlOptions.ControlInputs
			{
				Action = action,
				PrimaryKey = text,
				PrimaryModifier = text2,
				SecondaryKey = text3,
				SecondaryModifier = text4
			};
		}

		public KeyboarLayout GetCurrentKeyboardLayout()
		{
			string layoutName = WindowsKeyboardLayoutDetection.KeyboardLayoutName();
			KeyboarLayout keyboarLayout = this.GetLayoutByKeyboardName(layoutName);
			if (keyboarLayout == null)
			{
				keyboarLayout = this.GetDefaultKeyboardLayout();
			}
			return keyboarLayout;
		}

		private KeyboarLayout GetLayoutByKeyboardName(string layoutName)
		{
			for (int i = 0; i < this._layouts.Length; i++)
			{
				KeyboarLayout keyboarLayout = this._layouts[i];
				if (keyboarLayout.ContainsKeyboardLayout(layoutName))
				{
					ControlOptions.Log.InfoFormat("load keyboardLayout {0} from layout {1}", new object[]
					{
						layoutName,
						keyboarLayout.LayoutName
					});
					return keyboarLayout;
				}
			}
			return null;
		}

		private KeyboarLayout GetLayoutByName(string layoutName)
		{
			for (int i = 0; i < this._layouts.Length; i++)
			{
				KeyboarLayout keyboarLayout = this._layouts[i];
				if (keyboarLayout.LayoutName == layoutName)
				{
					return keyboarLayout;
				}
			}
			ControlOptions.Log.ErrorFormat("Layout Not Fount: {0}", new object[]
			{
				layoutName
			});
			return null;
		}

		public KeyboarLayout GetDefaultKeyboardLayout()
		{
			return this.GetLayoutByName("QWERTY");
		}

		private void SaveInJson()
		{
			ControlOptions.KeyboardLayoutWrapper keyboardLayoutWrapper = new ControlOptions.KeyboardLayoutWrapper();
			keyboardLayoutWrapper.Init(this._layouts.Length);
			keyboardLayoutWrapper.Layouts = this._layouts;
			string contents = JsonUtility.ToJson(keyboardLayoutWrapper);
			File.WriteAllText(GameHubBehaviour.Hub.Config.keyboardLayoutsPath, contents);
		}

		private void LoadFromJson()
		{
			string keyboardLayoutsPath = GameHubBehaviour.Hub.Config.keyboardLayoutsPath;
			if (File.Exists(keyboardLayoutsPath))
			{
				string json = File.ReadAllText(keyboardLayoutsPath);
				ControlOptions.KeyboardLayoutWrapper keyboardLayoutWrapper = JsonUtility.FromJson<ControlOptions.KeyboardLayoutWrapper>(json);
				this._layouts = keyboardLayoutWrapper.Layouts;
			}
			else
			{
				ControlOptions.Log.ErrorFormat("File KeyboardLayout not found {0}", new object[]
				{
					keyboardLayoutsPath
				});
			}
		}

		public void CancelScan()
		{
			cInput.CancelScan();
		}

		public void ResetDefault()
		{
			this.ReloadKeys();
			cInput.ResetInputs();
			if (this.OnResetDefaultCallback != null)
			{
				this.OnResetDefaultCallback();
			}
		}

		public void ResetPrimaryDefault()
		{
			this.ReloadKeys();
			cInput.ResetPrimaryInputs();
			if (this.OnResetPrimaryDefaultCallback != null)
			{
				this.OnResetPrimaryDefaultCallback();
			}
		}

		public void ResetSecundaryDefault()
		{
			cInput.ResetSecundaryInputs();
			if (this.OnResetSecondaryDefaultCallback != null)
			{
				this.OnResetSecondaryDefaultCallback();
			}
		}

		private Control GetControlActionFromLayout(ControlAction controlAction)
		{
			for (int i = 0; i < ControlOptions._currentLayout.Controls.Length; i++)
			{
				Control control = ControlOptions._currentLayout.Controls[i];
				if (control.Action == controlAction)
				{
					return control;
				}
			}
			for (int j = 0; j < ControlOptions._defaultLayout.Controls.Length; j++)
			{
				Control control2 = ControlOptions._defaultLayout.Controls[j];
				if (control2.Action == controlAction)
				{
					return control2;
				}
			}
			return null;
		}

		public void ResetDefault(ControlAction controlAction, ControlOptions.ControlActionInputType inputType)
		{
			Control controlActionFromLayout = this.GetControlActionFromLayout(controlAction);
			if (controlActionFromLayout == null)
			{
				ControlOptions.Log.ErrorFormat("Could not find control Action From Layout. ControlAction={0}, InputType={1}", new object[]
				{
					controlAction,
					inputType
				});
				return;
			}
			if (inputType != ControlOptions.ControlActionInputType.Primary)
			{
				if (inputType == ControlOptions.ControlActionInputType.Secondary)
				{
					string text = ControlOptions.GetText(controlAction, ControlOptions.ControlActionInputType.Secondary);
					cInput.ChangeKey(controlAction.ToString(), text, controlActionFromLayout.SecondaryKey, controlActionFromLayout.PrimaryModifier, controlActionFromLayout.SecondaryModifier);
				}
			}
			else
			{
				string text2 = ControlOptions.GetText(controlAction, ControlOptions.ControlActionInputType.Secondary);
				cInput.ChangeKey(controlAction.ToString(), controlActionFromLayout.PrimaryKey, text2, controlActionFromLayout.PrimaryModifier, controlActionFromLayout.SecondaryModifier);
			}
		}

		public void ChangePrimaryKey(ControlAction controlAction)
		{
			if (this.CheckBusy(controlAction))
			{
				ControlOptions.Log.Warn(string.Format("ControlOption is busy waiting for input option: {0}. Ignoring request for new key: {1}", this.currentControlAction, controlAction));
				return;
			}
			this.PrepareCancelChange(controlAction, ControlOptions.ControlActionInputType.Primary);
			cInput.ChangeKey(controlAction.ToString(), 1, false, true, false, false, true);
		}

		public void ChangeSecondaryKey(ControlAction controlAction)
		{
			if (this.CheckBusy(controlAction))
			{
				ControlOptions.Log.Warn(string.Format("ControlOption is busy waiting for input option: {0}. Ignoring request for new key: {1}", this.currentControlAction, controlAction));
				return;
			}
			this.PrepareCancelChange(controlAction, ControlOptions.ControlActionInputType.Secondary);
			cInput.ChangeKey(controlAction.ToString(), 2, false, false, true, true, false);
		}

		public void ChangePrimaryKeyDirectly(ControlAction controlAction, string primary)
		{
			if (this.CheckBusy(controlAction))
			{
				ControlOptions.Log.Warn(string.Format("ControlOption is busy waiting for input option: {0}. Ignoring request for new key: {1}", this.currentControlAction, controlAction));
				return;
			}
			string text = ControlOptions.GetText(controlAction, ControlOptions.ControlActionInputType.Secondary);
			cInput.ChangeKey(controlAction.ToString(), primary, text, string.Empty, string.Empty);
		}

		public void ChangeSecondaryKeyDirectly(ControlAction controlAction, string secondary)
		{
			if (this.CheckBusy(controlAction))
			{
				ControlOptions.Log.Warn(string.Format("ControlOption is busy waiting for input option: {0}. Ignoring request for new key: {1}", this.currentControlAction, controlAction));
				return;
			}
			string text = ControlOptions.GetText(controlAction, ControlOptions.ControlActionInputType.Primary);
			cInput.ChangeKey(controlAction.ToString(), text, secondary, string.Empty, string.Empty);
		}

		public void CancelChangeInput()
		{
			cInput.CancelChangeKeyScan();
			this.isBusy = false;
			if (this._controlActionInputTypeBeforeChange == ControlOptions.ControlActionInputType.Primary)
			{
				string text = ControlOptions.GetText(this._controlActionBeforeChange, ControlOptions.ControlActionInputType.Secondary);
				cInput.ChangeKey(this._controlActionBeforeChange.ToString(), this._inputBeforeChange, text, string.Empty, string.Empty);
			}
			else if (this._controlActionInputTypeBeforeChange == ControlOptions.ControlActionInputType.Secondary)
			{
				string text2 = ControlOptions.GetText(this._controlActionBeforeChange, ControlOptions.ControlActionInputType.Primary);
				cInput.ChangeKey(this._controlActionBeforeChange.ToString(), text2, this._inputBeforeChange, string.Empty, string.Empty);
			}
		}

		private bool CheckBusy(ControlAction controlAction)
		{
			if (this.isBusy)
			{
				return true;
			}
			this.currentControlAction = controlAction;
			this.isBusy = true;
			return false;
		}

		private void PrepareCancelChange(ControlAction controlAction, ControlOptions.ControlActionInputType controlActionInputType)
		{
			this._inputBeforeChange = ControlOptions.GetText(controlAction, controlActionInputType);
			this._controlActionBeforeChange = controlAction;
			this._controlActionInputTypeBeforeChange = controlActionInputType;
		}

		private void Update()
		{
			if (!this.isBusy)
			{
				return;
			}
			if (cInput.scanning)
			{
				return;
			}
			this.isBusy = false;
			if (this.OnKeyChangedCallback != null)
			{
				this.OnKeyChangedCallback(this.currentControlAction);
			}
		}

		public static CarInput.DrivingStyleKind DefaultMovementMode
		{
			get
			{
				return ControlOptions.instance.DefaultSetting.DefaultMovementMode;
			}
		}

		public static bool DefaultInvertReverseControl
		{
			get
			{
				return ControlOptions.instance.DefaultSetting.InvertReverseControl;
			}
		}

		public static void LogBILanguageConfig()
		{
			ControlOptions.WrapperBI wrapperBI = new ControlOptions.WrapperBI
			{
				ClientLanguage = Language.CurrentLanguage().ToString(),
				KeyboardLayout = WindowsKeyboardLayoutDetection.KeyboardLayoutName(),
				SteamID = GameHubBehaviour.Hub.User.UniversalId
			};
			string msg = JsonUtility.ToJson(wrapperBI);
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.LanguageConfig, msg, false);
		}

		public static bool GetButton(ControlAction controlAction)
		{
			return cInput.GetButton(ControlActionStringMap.Map[(int)controlAction]) && ControlOptions.IsControlActionUnlocked(controlAction);
		}

		public static bool GetButtonDown(ControlAction controlAction)
		{
			return cInput.GetButtonDown(ControlActionStringMap.Map[(int)controlAction]) && ControlOptions.IsControlActionUnlocked(controlAction);
		}

		public static bool GetButtonUp(ControlAction controlAction)
		{
			return cInput.GetButtonUp(ControlActionStringMap.Map[(int)controlAction]) && ControlOptions.IsControlActionUnlocked(controlAction);
		}

		public static string GetText(ControlAction controlAction)
		{
			return cInput.GetText(ControlActionStringMap.Map[(int)controlAction], false);
		}

		public static string GetTextlocalized(ControlAction controlAction, ControlOptions.ControlActionInputType controlActionInputType)
		{
			string text = cInput.GetText(ControlActionStringMap.Map[(int)controlAction], (int)controlActionInputType, false);
			return ControlOptions.GetTextlocalized(text);
		}

		public static string GetCInputText(ControlAction controlAction, ControlOptions.ControlActionInputType controlActionInputType)
		{
			return cInput.GetText(ControlActionStringMap.Map[(int)controlAction], (int)controlActionInputType, false);
		}

		public static string GetNGUIIconOrTextLocalized(ControlAction controlAction, ControlOptions.ControlActionInputType controlActionInputType)
		{
			string text = cInput.GetText(ControlActionStringMap.Map[(int)controlAction], (int)controlActionInputType, false);
			if (ControlOptions.inputNGUIIconDictionary.ContainsKey(text))
			{
				return ControlOptions.inputNGUIIconDictionary[text];
			}
			return ControlOptions.GetTextlocalized(text);
		}

		private static string GetTextlocalized(string input)
		{
			if (input.Contains(" + "))
			{
				string[] array = input.Split(new char[]
				{
					'+'
				});
				string input2 = array[0].Replace(" ", string.Empty);
				string input3 = array[1].Replace(" ", string.Empty);
				string layoutTranslation = ControlOptions.GetLayoutTranslation(input2);
				string layoutTranslation2 = ControlOptions.GetLayoutTranslation(input3);
				return string.Format("{0} + {1}", layoutTranslation, layoutTranslation2);
			}
			return ControlOptions.GetLayoutTranslation(input);
		}

		private static string GetLayoutTranslation(string input)
		{
			string text = ControlOptions._currentLayout.TranslationKeyByLayout(input, ControlOptions._defaultLayout);
			if (text == null)
			{
				text = Language.Get(input, TranslationSheets.Inputs);
			}
			return text;
		}

		public static bool IsActionScanning(ControlAction controlAction, ControlOptions.ControlActionInputType controlActionInputType)
		{
			return cInput.IsActionScanning(cInput.GetText(ControlActionStringMap.Map[(int)controlAction], (int)controlActionInputType, false), (int)controlActionInputType);
		}

		public static string GetText(ControlAction controlAction, ControlOptions.ControlActionInputType controlActionInputType)
		{
			return cInput.GetText(ControlActionStringMap.Map[(int)controlAction], (int)controlActionInputType, false);
		}

		public static string GetShortText(ControlAction controlAction)
		{
			return cInput.GetText(ControlActionStringMap.Map[(int)controlAction], true);
		}

		public static string GetShortText(ControlAction controlAction, ControlOptions.ControlActionInputType controlActionInputType)
		{
			return cInput.GetText(ControlActionStringMap.Map[(int)controlAction], (int)controlActionInputType, true);
		}

		public static bool IsUsingControllerJoystick(HMMHub hub)
		{
			return hub.Options.Game.MovementModeIndex == 1;
		}

		public static bool IsMouseInput(ControlAction controlAction)
		{
			string text = ControlOptions.GetText(controlAction);
			return text.ToLower().Contains("mouse");
		}

		public static bool IsMouseInput(ControlAction controlAction, out KeyCode keyCode)
		{
			string text = ControlOptions.GetText(controlAction);
			if (text.ToLower().Contains(KeyCode.Mouse0.ToString().ToLower()))
			{
				keyCode = KeyCode.Mouse0;
				return true;
			}
			if (text.ToLower().Contains(KeyCode.Mouse1.ToString().ToLower()))
			{
				keyCode = KeyCode.Mouse1;
				return true;
			}
			if (text.ToLower().Contains(KeyCode.Mouse2.ToString().ToLower()))
			{
				keyCode = KeyCode.Mouse2;
				return true;
			}
			keyCode = KeyCode.None;
			return false;
		}

		public static void LockAllInputs(bool lockAll)
		{
			ControlOptions._isInputLocked = lockAll;
		}

		public static void UnlockAllControlActions()
		{
			IEnumerator enumerator = Enum.GetValues(typeof(ControlAction)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					ControlAction controlAction = (ControlAction)obj;
					ControlOptions.UnlockControlAction(controlAction);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public static void LockAllControlActions()
		{
			IEnumerator enumerator = Enum.GetValues(typeof(ControlAction)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					ControlAction controlAction = (ControlAction)obj;
					ControlOptions.LockControlAction(controlAction);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public static void UnlockControlAction(ControlAction[] controlActions)
		{
			foreach (ControlAction controlAction in controlActions)
			{
				ControlOptions.UnlockControlAction(controlAction);
				ControlOptions.NotifyListeners(controlAction, true);
			}
		}

		public static void LockControlAction(ControlAction[] controlActions)
		{
			foreach (ControlAction controlAction in controlActions)
			{
				ControlOptions.LockControlAction(controlAction);
				ControlOptions.NotifyListeners(controlAction, false);
			}
		}

		private static void NotifyListeners(ControlAction controlAction, bool enable)
		{
			if (!ControlOptions.ActionListeners.ContainsKey(controlAction))
			{
				return;
			}
			ControlOptions.ActionListeners[controlAction](enable);
		}

		public static void UnlockControlAction(ControlAction controlAction)
		{
			ControlOptions.lockedControlActions |= 1 << (int)controlAction;
		}

		public static void LockControlAction(ControlAction controlAction)
		{
			ControlOptions.lockedControlActions &= ~(1 << (int)controlAction);
		}

		public static bool IsControlActionUnlocked(ControlAction controlAction)
		{
			return !ControlOptions._isInputLocked && (ControlOptions.lockedControlActions | 1 << (int)controlAction) == ControlOptions.lockedControlActions;
		}

		public static float GetAxis(ControlOptions.JoystickInput joystickInput)
		{
			float num = 0f;
			if (joystickInput != ControlOptions.JoystickInput.Joy1Axis1)
			{
				if (joystickInput == ControlOptions.JoystickInput.Jo1Axis2)
				{
					num = Input.GetAxis("Joy1 Axis 2");
					num = Mathf.Clamp(num, (float)((!ControlOptions.IsControlActionUnlocked(ControlAction.MovementForward)) ? 0 : -1), (float)((!ControlOptions.IsControlActionUnlocked(ControlAction.MovementBackward)) ? 0 : 1));
				}
			}
			else
			{
				num = Input.GetAxis("Joy1 Axis 1");
				num = Mathf.Clamp(num, (float)((!ControlOptions.IsControlActionUnlocked(ControlAction.MovementLeft)) ? 0 : -1), (float)((!ControlOptions.IsControlActionUnlocked(ControlAction.MovementRight)) ? 0 : 1));
			}
			return num;
		}

		public void OnCleanup(CleanupMessage msg)
		{
			ControlOptions.ActionListeners.Clear();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ControlOptions));

		private string _inputBeforeChange;

		private ControlAction _controlActionBeforeChange;

		private ControlOptions.ControlActionInputType _controlActionInputTypeBeforeChange;

		public ControlSetting DefaultSetting;

		private KeyboarLayout[] _layouts;

		private static KeyboarLayout _currentLayout;

		private static KeyboarLayout _defaultLayout;

		public ControlAction currentControlAction;

		public static Dictionary<ControlAction, Action<bool>> ActionListeners = new Dictionary<ControlAction, Action<bool>>();

		public static Dictionary<string, string> inputNGUIIconDictionary = new Dictionary<string, string>();

		private static ControlOptions instance;

		public bool isBusy;

		[SerializeField]
		public ControlOptions.JoystickButtonIcons joystickButtonIcons;

		private static bool _isInputLocked;

		[CompilerGenerated]
		private static Action <>f__mg$cache0;

		[CompilerGenerated]
		private static Action <>f__mg$cache1;

		[CompilerGenerated]
		private static Action <>f__mg$cache2;

		public delegate void OnKeyChangedDelegate(ControlAction controlAction);

		[Serializable]
		public struct JoystickButtonIcons
		{
			public Sprite A;

			public Sprite B;

			public Sprite X;

			public Sprite Y;

			public Sprite Start;

			public Sprite Back;

			public Sprite LB;

			public Sprite LT;

			public Sprite RB;

			public Sprite RT;
		}

		public enum ControlActionInputType
		{
			Primary = 1,
			Secondary,
			MouseKeyboard = 1,
			Joystick
		}

		private struct ControlInputs
		{
			public string Action;

			public string PrimaryKey;

			public string PrimaryModifier;

			public string SecondaryKey;

			public string SecondaryModifier;
		}

		[Serializable]
		private struct WrapperBI
		{
			public string SteamID;

			public string KeyboardLayout;

			public string ClientLanguage;
		}

		public enum JoystickInput
		{
			Joy1Axis1,
			Jo1Axis2
		}

		[Serializable]
		public class KeyboardLayoutWrapper
		{
			public void Init(int size)
			{
				this.Layouts = new KeyboarLayout[size];
			}

			public KeyboarLayout[] Layouts;
		}
	}
}
