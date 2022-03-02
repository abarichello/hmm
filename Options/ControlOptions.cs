using System;
using System.Collections;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.VoiceChat.Business;
using Pocketverse;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Options
{
	public class ControlOptions : GameHubBehaviour
	{
		public static long lockedControlActions { get; private set; }

		public void Init()
		{
			ControlOptions.UnlockAllControlActions();
			this.ChangeInputLanguage();
			if (!GameHubBehaviour.Hub.Net.isTest)
			{
				GameHubBehaviour.Hub.PlayerPrefs.ExecOnceOnPrefsLoaded(new Action(this._voiceChatPreferences.LoadPreferences));
			}
			if (!GameHubBehaviour.Hub.Net.isTest)
			{
				GameHubBehaviour.Hub.PlayerPrefs.ExecOnPrefsWrongVersion(new Action(this.OnPlayerPrefsWrongVersion));
			}
		}

		private void OnPlayerPrefsWrongVersion()
		{
			HMMHub hub = GameHubBehaviour.Hub;
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("PLAYERPREFS_WRONG_VERSION", TranslationContext.MainMenuGui),
				OkButtonText = Language.Get("PLAYERPREFS_CONFIRM", TranslationContext.MainMenuGui),
				OnOk = delegate()
				{
					hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void ChangeInputLanguage()
		{
			int keyboardLayoutId = Platform.Current.GetKeyboardLayoutDetection().GetKeyboardLayoutId();
			Language.ChangeInputLanguage(Platform.Current.GetKeyboardLayoutDetection().GetKeyboardLanguageCode(keyboardLayoutId));
		}

		[Obsolete]
		public static CarInput.DrivingStyleKind DefaultMovementMode
		{
			get
			{
				return CarInput.DrivingStyleKind.FollowMouse;
			}
		}

		[Obsolete]
		public static bool DefaultInvertReverseControl
		{
			get
			{
				return true;
			}
		}

		public static void LogBICurrentLanguage()
		{
			ControlOptions.LogBILanguage(84, true);
		}

		public static void LogBILanguageConfig()
		{
			ControlOptions.LogBILanguage(64, false);
		}

		private static void LogBILanguage(ClientBITags tag, bool forceSendLogs = false)
		{
			ControlOptions.WrapperBI wrapperBI = new ControlOptions.WrapperBI
			{
				ClientLanguage = Language.CurrentLanguage.ToString(),
				KeyboardLayout = Platform.Current.GetKeyboardLayoutDetection().GetKeyboardLayoutName(),
				SteamID = GameHubBehaviour.Hub.User.UniversalId
			};
			string msg = JsonUtility.ToJson(wrapperBI);
			GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(tag, msg, forceSendLogs);
		}

		public static void LockAllInputs(bool lockAll)
		{
			ControlOptions._isAllInputsLocked = lockAll;
			ControlOptions.Log.DebugFormat("Lock all inputs Set to {0}", new object[]
			{
				lockAll
			});
		}

		public static void UnlockAllControlActions()
		{
			ControlOptions.Log.Debug("UnlockAllControlActions");
			IEnumerator enumerator = Enum.GetValues(typeof(ControllerInputActions)).GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					ControllerInputActions inputAction = (ControllerInputActions)obj;
					ControlOptions.UnlockControlAction(inputAction);
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

		public static void UnlockControlAction(ControllerInputActions[] inputActions)
		{
			foreach (ControllerInputActions inputAction in inputActions)
			{
				ControlOptions.UnlockControlAction(inputAction);
			}
		}

		public static void UnlockControlAction(ControllerInputActions inputAction)
		{
			ControlOptions.lockedControlActions |= 1L << inputAction;
		}

		public static void LockControlAction(ControllerInputActions inputAction)
		{
			ControlOptions.lockedControlActions &= ~(1L << inputAction);
		}

		public static bool IsControlActionUnlocked(ControllerInputActions inputAction)
		{
			return !ControlOptions._isAllInputsLocked && (ControlOptions.lockedControlActions | 1L << inputAction) == ControlOptions.lockedControlActions;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ControlOptions));

		private static bool _isAllInputsLocked;

		[Inject]
		private IVoiceChatPreferences _voiceChatPreferences;

		[Serializable]
		private struct WrapperBI
		{
			public string SteamID;

			public string KeyboardLayout;

			public string ClientLanguage;
		}
	}
}
