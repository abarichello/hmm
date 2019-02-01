using System;
using System.Collections;
using System.Diagnostics;
using FMod;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.VFX;
using Pocketverse;
using Pocketverse.MuralContext;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class Game : GameState
	{
		public bool IsTutorial
		{
			get
			{
				return this._isTutorial;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Game.FinishedLoadingListener FinishedLoading;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Game.GameOver OnGameOver;

		public void EndMatch()
		{
			if (!this._gameEnded)
			{
				if (this.OnGameOver != null)
				{
					this.OnGameOver(this._hub.Match.State);
				}
				this._gameGui.Hud.SetActive(false);
				this._gameEnded = true;
				if (this._hub.Match.State == MatchData.MatchState.MatchOverBluWins || this._hub.Match.State == MatchData.MatchState.MatchOverRedWins)
				{
					if (this._hub.Match.LevelIsTutorial())
					{
						return;
					}
					int num = (this._hub.Match.State != MatchData.MatchState.MatchOverBluWins) ? 1 : 2;
					this._hub.GuiScripts.TooltipController.HideWindow();
					this._gameGui.EndGame = null;
					this._gameGui.HudWinnerController = null;
					this._gameGui.HudTabController.DontShowWindowOnGameOver();
					this.PreloadHudWinnerWindowAsync();
					this._gameGui.ShowEndGameBackground(new Action(this.ShowHudWinnerWindow));
				}
				else if (this._hub.Match.LevelIsTutorial())
				{
					ControlOptions.UnlockAllControlActions();
					ControlOptions.LockAllInputs(false);
				}
			}
		}

		private void PreloadEndMatchScreenAsync()
		{
			UIWindow.LoadWindow("End_Match_Screen", this._gameGui.transform, new UIWindow.OnLoadDelegate(this.OnLoadEndMatchScreenAsync));
		}

		private void OnLoadEndMatchScreenAsync(UIWindow window)
		{
			this._gameGui.EndGame = (UIProgressionController)window;
			this._gameGui.EndGame.gameObject.SetActive(false);
		}

		private void ShowHudWinnerWindow()
		{
			base.StartCoroutine(this.WaitAndShowHudWinnerWindow());
		}

		private void PreloadHudWinnerWindowAsync()
		{
			UIBundleWindow.OpenWindow(SingletonMonoBehaviour<PanelController>.Instance.MainDynamicParentTransform, "HUDWinner", ref this._hudWinnerUiBundleWindow, new UIBundleWindow.OnLoadDelegate(this.OnLoadHudWinnerWindow), delegate
			{
				Game.Log.Error("Window was not Load");
			});
		}

		private void OnLoadHudWinnerWindow(Transform windowtransform)
		{
			this._gameGui.HudWinnerController = windowtransform.GetComponent<HudWinnerController>();
			this._gameGui.HudWinnerController.gameObject.SetActive(false);
			this.PreloadEndMatchScreenAsync();
		}

		private IEnumerator WaitAndShowHudWinnerWindow()
		{
			while (this._gameGui.HudWinnerController == null)
			{
				yield return null;
			}
			this._gameGui.HudWinnerController.gameObject.SetActive(true);
			this._gameGui.HudWinnerController.SetWindowVisibility(true);
			HudWinnerController hudWinnerController = this._gameGui.HudWinnerController;
			hudWinnerController.OnDisableAction = (Action)Delegate.Combine(hudWinnerController.OnDisableAction, new Action(this.OnHudWinnerWindowDisable));
			VoiceOverController voControl = (!this._hub.User.IsNarrator) ? this._hub.Players.CurrentPlayerData.CharacterInstance.GetBitComponent<VoiceOverController>() : null;
			if (voControl != null)
			{
				voControl.TriggerMatchEndAudio();
			}
			yield break;
		}

		private void OnHudWinnerWindowDisable()
		{
			base.StartCoroutine(this.WaitAndShowEndMatchScreen());
		}

		private IEnumerator WaitAndShowEndMatchScreen()
		{
			while (this._gameGui.EndGame == null)
			{
				yield return null;
			}
			this._gameGui.EndGame.gameObject.SetActive(true);
			this._gameGui.EndGame.ShowProgression();
			if (!this._recapStarted && (this._hub.Match.State == MatchData.MatchState.MatchOverBluWins || this._hub.Match.State == MatchData.MatchState.MatchOverRedWins))
			{
				this._recapStarted = true;
				this._hub.Swordfish.Log.BILogClientMatch(ClientBITags.GameEnd, false);
				this._hub.Swordfish.Log.BILogClientMatch(ClientBITags.RecapStart, false);
			}
			this._hudWinnerUiBundleWindow.UnloadWindow();
			this._hudWinnerUiBundleWindow = null;
			this._gameGui.HudWinnerController = null;
			yield break;
		}

		public void ClearBackToMain()
		{
			if (this._clearCall)
			{
				return;
			}
			if (this._hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.BackToMain();
				return;
			}
			this._clearCall = true;
			this._hub.User.Bag.CurrentServerIp = null;
			this._hub.User.Bag.CurrentMatchId = null;
			this._hub.User.Bag.CurrentGroupId = null;
			this._hub.User.Bag.CurrentPort = 0;
			this.BackToMain();
			this._clearCall = false;
		}

		public void BackToMain()
		{
			Application.LoadLevel("Void");
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
			base.GoToState(this.Menu, false);
		}

		protected override void OnStateEnabled()
		{
			this._hub = GameHubBehaviour.Hub;
			this._isLoading = true;
			this._levelLoaded = false;
			this._eventsLoaded = false;
			this._eventsExecuted = false;
			this._isTutorial = this._hub.Match.LevelIsTutorial();
			this._gameEnded = false;
			this._recapStarted = false;
			this._hub.UpdateManager.SetRunning(true);
		}

		protected override void OnMyLevelLoaded()
		{
			this._gameGui = this._hub.State.Current.GetStateGuiController<GameGui>();
			if (this._gameGui != null)
			{
				this.ApplyArenaConfig();
				this._gameGui.Hud.SetActive(true);
				CarCamera.Singleton.SetMode(CarCamera.CarCameraMode.SkyView);
			}
			this._hub.Server.LoadLevel(new Action(this.OnGameLevelLoaded));
			this._hub.MatchHistory.WriteMatchStartInfo();
		}

		private void ApplyArenaConfig()
		{
			GameArenaInfo gameArenaInfo = this._hub.ArenaConfig.Arenas[this._hub.Match.ArenaIndex];
			if (gameArenaInfo == null)
			{
				Game.Log.ErrorFormat("Failed to get ArenaInfo for arena {0}", new object[]
				{
					this._hub.Match.ArenaIndex
				});
				return;
			}
			CarCamera singleton = CarCamera.Singleton;
			singleton.CameraInversionTeamAAngleY = (float)gameArenaInfo.CameraInversionTeamAAngleY;
			singleton.CameraInversionTeamBAngleY = (float)gameArenaInfo.CameraInversionTeamBAngleY;
		}

		private void DestroySceneColliders()
		{
			Collider[] array = UnityEngine.Object.FindObjectsOfType<Collider>();
			for (long num = 0L; num < array.LongLength; num += 1L)
			{
				bool flag = true;
				checked
				{
					int layer = array[(int)((IntPtr)num)].gameObject.layer;
					flag &= (layer != 10);
					flag &= (layer != 11);
					flag &= (layer != 27);
					flag &= (layer != 31);
					if (flag)
					{
						UnityEngine.Object.Destroy(array[(int)((IntPtr)num)]);
					}
				}
			}
		}

		private void OnGameLevelLoaded()
		{
			base.StartCoroutine(this.LoadEvents());
		}

		private IEnumerator LoadEvents()
		{
			if (this._hub.State.Current != this)
			{
				yield break;
			}
			if (!this._isTutorial)
			{
				while (!this._hub.Events.Players.CarCreationFinished && !this._hub.Events.Bots.CarCreationFinished)
				{
					yield return null;
				}
			}
			PlaybackSystem.PlayAndRecordPlayback();
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.HORTA))
			{
				this._levelLoaded = true;
				this.DestroySceneColliders();
				this.OnEventsLoaded();
				GameHubBehaviour.Hub.GameTime.SetTimeZero();
				GameHubBehaviour.Hub.Server.ServerSet();
				this.Horta.StatePlayback.Running = true;
				this.Horta.Behaviour.Running = true;
				this.OnEventsExecuted();
				yield break;
			}
			this._hub.Server.GetEvents(new Action(this.OnEventsLoaded), new Action(this.OnEventsExecuted));
			if (!this._isTutorial)
			{
				this._hub.Swordfish.Log.BILogClientMatchMsg(ClientBITags.GameStart, string.Format("IsReconnect={0}", this._hub.User.IsReconnecting), false);
				this._hub.Swordfish.MatchBI.ClientAnnouncerConfigured(0);
			}
			this.DestroySceneColliders();
			this._levelLoaded = true;
			base.StartCoroutine(this.CheckEventsExecutionTimeout());
			yield break;
		}

		private IEnumerator CheckEventsExecutionTimeout()
		{
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(120f));
			if (this._eventsLoaded && this._eventsExecuted)
			{
				yield break;
			}
			this._eventsLoaded = true;
			this._eventsExecuted = true;
			Game.Log.Warn("Timeout while waiting for all initial events to be executed. Will remove loading screen.");
			yield break;
		}

		private void OnEventsLoaded()
		{
			this._eventsLoaded = true;
			if (!this._hub.Match.LevelIsTutorial())
			{
				this._hub.GuiScripts.Loading.ChangeText(Language.Get("LOADING_WAITING_PLAYERS", TranslationSheets.GUI));
			}
			base.StartCoroutine(this.WaitEventExecution());
		}

		private IEnumerator WaitEventExecution()
		{
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(1f));
			AsyncOperation asyncOperation = Resources.UnloadUnusedAssets();
			yield return asyncOperation;
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(1f));
			GC.Collect();
			yield break;
		}

		private void OnEventsExecuted()
		{
			this._eventsExecuted = true;
			CarCamera.Singleton.ForceSnappingToTarget();
			this._hub.Server.DispatchReliable(new byte[0]).OnServerPlayerReady();
		}

		protected override void OnStateDisabled()
		{
			this._hub.UpdateManager.SetRunning(false);
			FMODAudioManager.UnloadAllPreloaded();
			if (this._gameEnded && !this._isTutorial)
			{
				if (this._recapStarted)
				{
					this._hub.Swordfish.Log.BILogClientMatch(ClientBITags.RecapEnd, false);
				}
				else
				{
					this._hub.Swordfish.Log.BILogClientMatch(ClientBITags.GameEnd, false);
				}
			}
			this._gameGui = null;
			this.FinishedLoading = null;
		}

		public bool IsLoading()
		{
			return !this._levelLoaded || !this._eventsLoaded || !this._eventsExecuted || LoadingManager.IsLoading || !this._hub.Server.Ready;
		}

		private void Update()
		{
			if (this._isLoading)
			{
				if (this.IsLoading())
				{
					return;
				}
				if (this.FinishedLoading != null)
				{
					this.FinishedLoading();
				}
				Game.Log.Info("LOADING: Finished loading game");
				this._isLoading = false;
				CarCamera.Singleton.enabled = true;
				if (!this._hub.Match.LevelIsTutorial())
				{
					if (this._hub.GuiScripts.Loading.IsLoading)
					{
						this._hub.GuiScripts.Loading.HideLoading();
					}
					this._gameGui.ShowGameHud(true);
					this._hub.GuiScripts.LoadingVersus.HideWindow();
					ControlOptions.UnlockAllControlActions();
					ControlOptions.LockAllInputs(false);
				}
				else if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.HORTA))
				{
					if (this._hub.GuiScripts.Loading.IsLoading)
					{
						this._hub.GuiScripts.Loading.HideLoading();
					}
					this._gameGui.ShowGameHud(true);
					this._hub.GuiScripts.LoadingVersus.HideWindow();
					ControlOptions.UnlockAllControlActions();
					ControlOptions.LockAllInputs(false);
					HudWindowManager.Instance.State = HudWindowManager.GuiGameState.Game;
				}
				else
				{
					HudWindowManager.Instance.State = HudWindowManager.GuiGameState.Game;
				}
			}
			MatchData.MatchState state = this._hub.Match.State;
			if (state != MatchData.MatchState.MatchOverBluWins && state != MatchData.MatchState.MatchOverRedWins)
			{
				return;
			}
			this.EndMatch();
		}

		public void SetGameEndMode()
		{
			PlayerController.LockedInputs = true;
		}

		private void OnApplicationQuit()
		{
			this._hub.Server.ClientSendPlayerDisconnectInfo();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(Game));

		public MainMenu Menu;

		public HORTAComponent Horta;

		private HMMHub _hub;

		private bool _isTutorial;

		private bool _gameEnded;

		private bool _recapStarted;

		private bool _isLoading;

		private bool _levelLoaded;

		private bool _eventsLoaded;

		private bool _eventsExecuted;

		private GameGui _gameGui;

		private UIBundleWindow _hudWinnerUiBundleWindow;

		private bool _clearCall;

		private MatchController match;

		public delegate void FinishedLoadingListener();

		public delegate void GameOver(MatchData.MatchState matchWinner);
	}
}
