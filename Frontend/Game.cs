using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using FMod;
using HeavyMetalMachines.Achievements;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.BackendCommunication;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.GameCamera.Behaviour;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Memory;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Playback;
using HeavyMetalMachines.PlayerTooltip.Presenting;
using HeavyMetalMachines.Presenting.ContextMenu;
using HeavyMetalMachines.Social.FriendDataUpdater.Business;
using HeavyMetalMachines.Social.Profile.Business;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.VFX;
using Hoplon.ToggleableFeatures;
using Hoplon.Unity.Loading;
using JetBrains.Annotations;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class Game : GameState, IStateGame
	{
		[Inject]
		private void InstallContainer(DiContainer container)
		{
			this._container = container.ParentContainers.First<DiContainer>();
		}

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
				GarbageCollector.Enable();
				this._toggleBackendCalling.Enable();
				ObservableExtensions.Subscribe<Unit>(this._diContainer.Resolve<IUploadPendingAchievements>().Upload());
				if (this.OnGameOver != null)
				{
					this.OnGameOver(this._hub.Match.State);
				}
				this._gameGui.Hud.SetActive(false);
				this._gameGui.KillFeedController.gameObject.SetActive(false);
				this._gameEnded = true;
				this._gameGui.HudLifebarController.gameObject.SetActive(false);
				if (this._hub.Match.LevelIsTutorial())
				{
					ControlOptions.UnlockAllControlActions();
					ControlOptions.LockAllInputs(false);
				}
				else if (this._hub.Match.State == MatchData.MatchState.MatchOverBluWins || this._hub.Match.State == MatchData.MatchState.MatchOverRedWins)
				{
					this._hub.GuiScripts.TooltipController.HideWindow();
					this._gameGui.EndGame = null;
					this._gameGui.HudWinnerController = null;
					this._gameGui.HudTabController.DontShowWindowOnGameOver();
					this._playerTooltipPresenter = this._container.Resolve<IPlayerTooltipPresenter>();
					this._disposables.Add(ObservableExtensions.Subscribe<Unit>(this._playerTooltipPresenter.Initialize()));
					this._disposables.Add(ObservableExtensions.Subscribe<Unit>(this._contextMenuPresenter.Initialize()));
					this._contextMenuInitialized = true;
					this.PreloadHudWinnerWindowAsync();
					this._gameGui.ShowEndGameBackground(new Action(this.ShowHudWinnerWindow));
				}
				this._gameGui.OnEndGame();
			}
		}

		private void PreloadEndMatchScreenAsync()
		{
			this._windowFactory.LoadWindow("End_Match_Screen", this._gameGui.transform, new Action<UIWindow>(this.OnLoadEndMatchScreenAsync));
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
			UIBundleWindow.OpenWindow(this._container, SingletonMonoBehaviour<PanelController>.Instance.MainDynamicParentTransform, "HUDWinner", ref this._hudWinnerUiBundleWindow, new UIBundleWindow.OnLoadDelegate(this.OnLoadHudWinnerWindow), delegate
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
				this._hub.Swordfish.Log.BILogClientMatch(17, false);
				this._hub.Swordfish.Log.BILogClientMatch(18, false);
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
			Game.Log.DebugFormat("Client Cleared Current server from PlayerBag", new object[0]);
			this.BackToMain();
			this._clearCall = false;
		}

		public void BackToMain()
		{
			if (this._contextMenuInitialized)
			{
				ObservableExtensions.Subscribe<Unit>(this._contextMenuPresenter.Dispose());
				ObservableExtensions.Subscribe<Unit>(this._playerTooltipPresenter.Dispose());
				this._contextMenuInitialized = false;
			}
			GameHubBehaviour.Hub.Match.State = MatchData.MatchState.Nothing;
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
			ScreenResolutionController.SetQualityLevel(ScreenResolutionController.QualityLevels.PreviewItem, false);
			this.GoToNextState();
		}

		private void GoToNextState()
		{
			if (this._shouldCreateProfile.Get())
			{
				base.GoToState(this._profile, false);
				return;
			}
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
			this._toggleBackendCalling.Disable();
			UIRoot.SetUiState(UIRoot.UiState.GamePlay);
			this._hub.UpdateManager.SetRunning(true);
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnListenToPhaseChange;
			GarbageCollector.Disable();
			this.InitializeFriendDataUpdater();
			this._listenerPositionUpdater.Start();
		}

		private void BombManagerOnListenToPhaseChange(BombScoreboardState scoreState)
		{
			if (scoreState == BombScoreboardState.PreBomb || scoreState == BombScoreboardState.Replay)
			{
				base.StartCoroutine(this.CollectGC("Game replay started."));
			}
		}

		[UsedImplicitly]
		private IEnumerator CollectGC(string reason)
		{
			for (int i = 0; i < 10; i++)
			{
				yield return null;
			}
			GarbageCollector.Collect(reason);
			for (int j = 0; j < 10; j++)
			{
				yield return null;
			}
			yield break;
		}

		private void InitializeFriendDataUpdater()
		{
			this._localFriendDataUpdater = this._container.ResolveId<ILocalFriendDataUpdater>(1);
			this._friendDataUpdaterDisposable = ObservableExtensions.Subscribe<Unit>(this._localFriendDataUpdater.ExecuteIndefinitely(), delegate(Unit onNext)
			{
				Game.Log.Debug("SwordfishLocalFriendDataService onNext");
			}, delegate(Exception onError)
			{
				Game.Log.ErrorFormat("SwordfishLocalFriendDataService Error={0}", new object[]
				{
					onError
				});
			});
		}

		protected override void OnMyLevelLoaded()
		{
			this._gameGui = this._hub.State.Current.GetStateGuiController<GameGui>();
			if (this._gameGui != null)
			{
				this.ApplyArenaConfig();
				this._gameGui.Hud.SetActive(true);
			}
			base.StartCoroutine(this.LoadMatchSceneCoroutine());
			this._hub.Server.ServerSetNotReadyLocal();
			this._hub.MatchHistory.WriteMatchStartInfo();
		}

		private void ApplyArenaConfig()
		{
			IGameArenaInfo currentArena = this._hub.ArenaConfig.GetCurrentArena();
			if (currentArena == null)
			{
				Game.Log.ErrorFormat("Failed to get ArenaInfo for arena {0}", new object[]
				{
					this._hub.Match.ArenaIndex
				});
				return;
			}
			this._gameCameraInversion.SetupArena(currentArena);
			this._deadBehaviour.SetupArena(currentArena);
		}

		private void DestroySceneColliders()
		{
			Collider[] array = Object.FindObjectsOfType<Collider>();
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
						Object.Destroy(array[(int)((IntPtr)num)]);
					}
				}
			}
		}

		private IEnumerator LoadMatchSceneCoroutine()
		{
			GameArenaConfig arenaConfig = GameHubBehaviour.Hub.ArenaConfig;
			string arenaSceneName = arenaConfig.GetSceneName(GameHubBehaviour.Hub.Match.ArenaIndex);
			Game.Log.DebugFormat("LOADING: client LoadLevel {0}", new object[]
			{
				arenaSceneName
			});
			this._arenaToken = new LoadingToken(base.GetType());
			this._arenaToken.AddLoadable(new SceneLoadable(arenaSceneName, true));
			AsyncRequest<LoadingResult> request = Loading.Engine.LoadToken(this._arenaToken);
			yield return request;
			if (LoadStatusExtensions.IsError(request.Result.Status))
			{
				Game.Log.ErrorFormat("Loading failed with status: {0}", new object[]
				{
					request.Result.Status
				});
				yield return LoadingFailedHandler.HandleFailure(request.Result);
				yield break;
			}
			GameHubBehaviour.Hub.Server.ClientSendPlayerLoadingInfo(1f);
			base.LoadingToken.InheritData(this._arenaToken);
			this._arenaToken = null;
			Scene scn = SceneManager.GetSceneByName(arenaSceneName);
			SceneManager.SetActiveScene(scn);
			base.StartCoroutine(this.LoadEvents());
			yield break;
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
			this._playback.Play();
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.HORTA))
			{
				this._levelLoaded = true;
				this.DestroySceneColliders();
				this.OnEventsLoaded();
				GameHubBehaviour.Hub.GameTime.SetTimeZero();
				GameHubBehaviour.Hub.Server.ServerSet();
				this._horta.StatePlayback.Running = true;
				this._horta.Behaviour.Running = true;
				this.OnEventsExecuted();
				yield break;
			}
			this._hub.Server.GetEvents(new Action(this.OnEventsLoaded), new Action(this.OnEventsExecuted));
			if (!this._isTutorial)
			{
				this._hub.Swordfish.Log.BILogClientMatchMsg(16, string.Format("IsReconnect={0}", this._hub.User.IsReconnecting), false);
				this._hub.Swordfish.MatchBI.ClientAnnouncerConfigured(0);
				this._clientAudioBiLogger.LogAudioOptions(this._hub.Options.Audio);
			}
			this.DestroySceneColliders();
			this._levelLoaded = true;
			yield return null;
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
			Game.Log.Debug("LOADING: Events loaded");
			if (!this._hub.Match.LevelIsTutorial())
			{
				this._hub.GuiScripts.Loading.ChangeText(Language.Get("LOADING_WAITING_PLAYERS", TranslationContext.GUI));
			}
		}

		private void OnEventsExecuted()
		{
			this._eventsExecuted = true;
			this._gameCamera.SnapToTarget();
			Game.Log.Debug("LOADING: Events executed");
			ObservableExtensions.Subscribe<bool>(this._restoreCurrentMatch.TryRestore());
			this._hub.Server.DispatchReliable(new byte[0]).OnServerPlayerReady();
		}

		protected override void OnStateDisabled()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnListenToPhaseChange;
			GarbageCollector.Enable();
			if (GameHubBehaviour.Hub.GameTime is NetworkTime)
			{
				((NetworkTime)GameHubBehaviour.Hub.GameTime).UpdateTimeScale(0L, 0, 0, 1f);
			}
			GameHubBehaviour.Hub.UpdateManager.SetRunning(false);
			FMODAudioManager.UnloadAllPreloaded();
			if (this._gameEnded && !this._isTutorial)
			{
				if (this._recapStarted)
				{
					this._hub.Swordfish.Log.BILogClientMatch(19, false);
				}
				else
				{
					this._hub.Swordfish.Log.BILogClientMatch(17, false);
				}
			}
			if (this._friendDataUpdaterDisposable != null)
			{
				this._friendDataUpdaterDisposable.Dispose();
				this._friendDataUpdaterDisposable = null;
			}
			this._disposables.Dispose();
			if (this._playerTooltipPresenter != null)
			{
				this._playerTooltipPresenter.Dispose();
				this._playerTooltipPresenter = null;
			}
			this._hudWinnerUiBundleWindow = null;
			this._gameGui.HudWinnerController = null;
			this._gameGui = null;
			this.FinishedLoading = null;
			this._listenerPositionUpdater.Stop();
		}

		public bool IsLoading()
		{
			return !this._levelLoaded || !this._eventsLoaded || !this._eventsExecuted || !Loading.Engine.IsIdle || !this._hub.Server.Ready;
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
				this._gameCameraEngine.Enable();
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
					HudWindowManager.Instance.State = GuiGameState.Game;
				}
				else
				{
					HudWindowManager.Instance.State = GuiGameState.Game;
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

		[SerializeField]
		private Profile _profile;

		[Inject]
		private HORTAComponent _horta;

		[Inject]
		private IPlayback _playback;

		[Inject]
		private IRestoreCurrentMatch _restoreCurrentMatch;

		[Inject]
		private IGameCamera _gameCamera;

		[Inject]
		private IGameCameraEngine _gameCameraEngine;

		[Inject]
		private IGameCameraInversion _gameCameraInversion;

		[Inject]
		private IPlayerDeadBehaviour _deadBehaviour;

		[Inject]
		private IShouldCreateProfile _shouldCreateProfile;

		[Inject]
		private IClientAudioBiLogger _clientAudioBiLogger;

		[Inject]
		private IContextMenuPresenter _contextMenuPresenter;

		[Inject]
		private IListenerPositionUpdater _listenerPositionUpdater;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		[Inject]
		private IToggleBackendCalling _toggleBackendCalling;

		[Inject]
		private DiContainer _diContainer;

		private DiContainer _container;

		private IPlayerTooltipPresenter _playerTooltipPresenter;

		private ILocalFriendDataUpdater _localFriendDataUpdater;

		private IDisposable _friendDataUpdaterDisposable;

		private CompositeDisposable _disposables = new CompositeDisposable();

		private HMMHub _hub;

		private bool _isTutorial;

		private bool _gameEnded;

		private bool _recapStarted;

		private bool _isLoading;

		private bool _levelLoaded;

		private bool _eventsLoaded;

		private bool _eventsExecuted;

		private bool _contextMenuInitialized;

		private LoadingToken _arenaToken;

		private GameGui _gameGui;

		[Inject]
		private IUIWindowFactory _windowFactory;

		private UIBundleWindow _hudWinnerUiBundleWindow;

		private bool _clearCall;

		public delegate void FinishedLoadingListener();

		public delegate void GameOver(MatchData.MatchState matchWinner);
	}
}
