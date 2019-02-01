using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HeavyMetalMachines.GameStates.Splashes.Hardware;
using HeavyMetalMachines.Platform;
using HeavyMetalMachines.Tutorial;
using Pocketverse;
using Steamworks;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class Splash : GameState
	{
		private Queue<ConfirmWindowProperties> ConfirmWindowQueue
		{
			get
			{
				Queue<ConfirmWindowProperties> result;
				if ((result = this._confirmWindowQueue) == null)
				{
					result = (this._confirmWindowQueue = new Queue<ConfirmWindowProperties>());
				}
				return result;
			}
		}

		protected override void OnMyLevelLoaded()
		{
			this._splashGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<SplashGui>();
			if (this._splashGui)
			{
				this._splashGui.PlaySplashes(new Action(this.SplashesEnded));
			}
			this._login = new Login(GameHubBehaviour.Hub, new Action<bool>(this.LoginEnded), new Action<ConfirmWindowProperties>(this.ShowConfirmWindow));
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				base.StartCoroutine(this.LoginCoroutine());
			}
			else if (GameHubBehaviour.Hub.Swordfish.Msg.MsgHub != null)
			{
				this._login.ConnectSteam();
			}
			else
			{
				Splash.Log.Error("No SkipSwordfish nor MsgHub. Will not call Login.ConnectSteam().");
			}
		}

		private IEnumerator LoginCoroutine()
		{
			yield return UnityUtils.WaitForOneSecond;
			this._login.ConnectSteam();
			yield break;
		}

		private void SplashesEnded()
		{
			GameHubBehaviour.Hub.Swordfish.Log.BILogFunnel(FunnelBITags.SplashVideoDone, null);
			this._splashEnded = true;
			this.JobDone();
		}

		private void LoginEnded(bool success)
		{
			if (success)
			{
				this._loginEnded = true;
				if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
				{
					GameHubBehaviour.Hub.ClientApi.RequestTimeOut = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.SFTimeout);
				}
				this.JobDone();
			}
		}

		private void ShowConfirmWindow(ConfirmWindowProperties confirmWindowProperties)
		{
			if (this._splashGui.HasFinished())
			{
				this.OpenConfirmationWindow(confirmWindowProperties);
				return;
			}
			if (confirmWindowProperties.IsTextOnly())
			{
				return;
			}
			this.EnqueueConfirmWindow(confirmWindowProperties);
		}

		private void EnqueueConfirmWindow(ConfirmWindowProperties confirmWindowProperties)
		{
			if (this.ConfirmWindowQueue.Count == 0)
			{
				this.ConfirmWindowQueue.Enqueue(confirmWindowProperties);
				GameHubBehaviour.Hub.StartCoroutine(this.ProcessConfirmWindowQueue());
			}
			else
			{
				this.ConfirmWindowQueue.Enqueue(confirmWindowProperties);
			}
		}

		private IEnumerator ProcessConfirmWindowQueue()
		{
			while (this.ConfirmWindowQueue.Count > 0)
			{
				while (!this._splashGui.HasFinished())
				{
					yield return null;
				}
				ConfirmWindowProperties confirmWindowProperties = this.ConfirmWindowQueue.Dequeue();
				this.OpenConfirmationWindow(confirmWindowProperties);
			}
			yield break;
		}

		private void OpenConfirmationWindow(ConfirmWindowProperties confirmWindowProperties)
		{
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(confirmWindowProperties);
		}

		private void JobDone()
		{
			if (!this._splashEnded || !this._loginEnded)
			{
				return;
			}
			Splash.LogBIPCSpecs(GameHubBehaviour.Hub);
			bool firstTime = !TutorialController.HasPlayerDoneFirstTutorial();
			NativePlugins.CrashReport.SetFirstTime(firstTime);
			this.SystemCheck(new Splash.SystemCheckCallback(this.OnSystemCheckFinished));
		}

		private void OnSystemCheckFinished(Splash.SystemCheckResult result)
		{
			if (result == Splash.SystemCheckResult.Pass || result == Splash.SystemCheckResult.Warn)
			{
				this.LoadNextState();
			}
		}

		private void LoadNextState()
		{
			if (this.IsReconnect())
			{
				base.GoToState(this._reconnectMenu, false);
				return;
			}
			if (this.IsForcedTutorial())
			{
				GameHubBehaviour.Hub.GuiScripts.Loading.ShowTutorialLoading(false);
				base.GoToState(this._matchmakingTutorial, false);
				return;
			}
			base.GoToState(this._main, true);
		}

		private bool IsReconnect()
		{
			return !string.IsNullOrEmpty(GameHubBehaviour.Hub.User.Bag.CurrentMatchId) && !GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false) && !GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.EnableNarrator, false);
		}

		private bool IsForcedTutorial()
		{
			return !GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish) && GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.PlayerMustGoToTutorial();
		}

		private static void LogBIPCSpecs(HMMHub hub)
		{
			WindowsPlatform.MEMORYSTATUSEX memorystatusex = new WindowsPlatform.MEMORYSTATUSEX();
			WindowsPlatform.GlobalMemoryStatusEx(memorystatusex);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("{");
			stringBuilder.AppendFormat("\"SystemMemorySize\": {0},", SystemInfo.systemMemorySize);
			stringBuilder.AppendFormat("\"FreeMemorySize\": {0},", memorystatusex.ullAvailPhys / 1048576UL);
			stringBuilder.AppendFormat("\"OperatingSystem\": \"{0}\",", SystemInfo.operatingSystem);
			stringBuilder.AppendFormat("\"ProcessorType\": \"{0}\",", SystemInfo.processorType);
			stringBuilder.AppendFormat("\"ProcessorCount\": {0},", SystemInfo.processorCount);
			stringBuilder.AppendFormat("\"ProcessorFrequency\": {0},", SystemInfo.processorFrequency);
			stringBuilder.AppendFormat("\"GraphicsDeviceName\": \"{0}\",", SystemInfo.graphicsDeviceName);
			stringBuilder.AppendFormat("\"GraphicsDeviceVendor\": \"{0}\",", SystemInfo.graphicsDeviceVendor);
			stringBuilder.AppendFormat("\"GraphicsMemorySize\": {0},", NativePlugins.GetDedicatedVideoMemorySize());
			stringBuilder.AppendFormat("\"GraphicsDeviceVersion\": \"{0}\",", SystemInfo.graphicsDeviceVersion);
			stringBuilder.AppendFormat("\"GraphicsDeviceID\": {0},", SystemInfo.graphicsDeviceID);
			stringBuilder.AppendFormat("\"GraphicsDeviceVendorID\": {0},", SystemInfo.graphicsDeviceVendorID);
			stringBuilder.AppendFormat("\"GraphicsDisplayCount\": {0},", (Display.displays == null) ? 1 : Display.displays.Length);
			stringBuilder.AppendFormat("\"PlayerId\": \"{0}\"", hub.User.UserSF.UniversalID);
			stringBuilder.Append("}");
			hub.Swordfish.Log.BILogClientMsg(ClientBITags.PCSpecsV2, stringBuilder.ToString(), true);
		}

		private void SystemCheck(Splash.SystemCheckCallback callback)
		{
			HardwareChecker hardwareChecker = new HardwareChecker(this.SystemCheckScriptableObject);
			if (!hardwareChecker.HasMinimumRequirements())
			{
				this.ShowConfirmWindowAndQuitApplication(callback);
				return;
			}
			if (!hardwareChecker.HasWarningRequirements())
			{
				this.ShowWarningWindowAndMovePlayerToMainMenu(callback);
				return;
			}
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.MinimumRequirementsOk, true);
			callback(Splash.SystemCheckResult.Pass);
		}

		private void ShowWarningWindowAndMovePlayerToMainMenu(Splash.SystemCheckCallback callback)
		{
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.MinimumRequirementsOk, true);
			NativePlugins.CrashReport.SetRamStatus("LIMITED");
			if (GameHubBehaviour.Hub.Options.Game.MemoryWarning)
			{
				ConfirmWindowProperties properties = new ConfirmWindowProperties
				{
					Guid = this._confirmWindowGuid,
					QuestionText = Language.Get("MEMORY_WARNING_DESCRIPTION", TranslationSheets.MainMenuGui),
					OkButtonText = Language.Get("MEMORY_WARNING_CONFIRM", TranslationSheets.MainMenuGui),
					CheckboxText = Language.Get("MEMORY_WARNING_DEACTIVE_CHECKBOX", TranslationSheets.MainMenuGui),
					CheckboxInitialState = false,
					OnOk = delegate()
					{
						if (GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Checkbox.value)
						{
							GameHubBehaviour.Hub.Options.Game.MemoryWarning = false;
							GameHubBehaviour.Hub.Options.Game.Apply();
							GameHubBehaviour.Hub.PlayerPrefs.SaveNow();
						}
						GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(this._confirmWindowGuid);
						callback(Splash.SystemCheckResult.Warn);
					}
				};
				GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
			}
			else
			{
				callback(Splash.SystemCheckResult.Warn);
			}
		}

		private void ShowConfirmWindowAndQuitApplication(Splash.SystemCheckCallback callback)
		{
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.MinimumRequirementsFail, true);
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = this._confirmWindowGuid,
				TileText = Language.Get(this.SystemCheckScriptableObject.TitleTextDraft, TranslationSheets.MainMenuGui),
				QuestionText = Language.Get(this.SystemCheckScriptableObject.QuestionTextDraft, TranslationSheets.MainMenuGui).Replace("\\n", "\n"),
				OkButtonText = Language.Get(this.SystemCheckScriptableObject.OkButtonTextDraft, TranslationSheets.MainMenuGui),
				CountDownTime = this.SystemCheckScriptableObject.LowSpecWindowTimeout,
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(this._confirmWindowGuid);
					callback(Splash.SystemCheckResult.Fail);
					GameHubBehaviour.Hub.Quit();
				},
				OnTimeOut = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(this._confirmWindowGuid);
					callback(Splash.SystemCheckResult.Fail);
					GameHubBehaviour.Hub.Quit();
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		protected override void OnStateEnabled()
		{
			this._splashStateElapsedTime = Time.time;
			if (GameHubBehaviour.Hub.Swordfish.Log == null)
			{
				Splash.Log.WarnFormat("SwordfishLog is null. Steam is running: {0}", new object[]
				{
					SteamAPI.IsSteamRunning()
				});
				return;
			}
			GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.LoginStart, true);
		}

		protected override void OnStateDisabled()
		{
			if (GameHubBehaviour.Hub.State.Last == this)
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClientMsg(ClientBITags.LoginEnd, string.Format("ConnectionTime={0}", Time.time - this._splashStateElapsedTime), true);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(Splash));

		[SerializeField]
		private MainMenu _main;

		[SerializeField]
		private ReconnectMenu _reconnectMenu;

		[SerializeField]
		private MatchmakingTutorial _matchmakingTutorial;

		private Login _login;

		private SplashGui _splashGui;

		private bool _splashEnded;

		private bool _loginEnded;

		private Queue<ConfirmWindowProperties> _confirmWindowQueue;

		private readonly Guid _confirmWindowGuid = Guid.NewGuid();

		public SystemCheckSettings SystemCheckScriptableObject;

		private float _splashStateElapsedTime;

		private delegate void SystemCheckCallback(Splash.SystemCheckResult result);

		private enum SystemCheckResult
		{
			Pass,
			Warn,
			Fail
		}
	}
}
