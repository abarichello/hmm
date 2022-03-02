using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.DriverHelper;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.MuteSystem;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.Spectator;
using HeavyMetalMachines.Spectator.View;
using HeavyMetalMachines.VFX;
using Hoplon.Input;
using Hoplon.Input.UiNavigation.InputDeliverer;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudWindowManager : GameHubBehaviour, ICleanupListener, IHudWindowManager
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GuiGameStateChange OnGuiStateChange;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnHelp;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<IHudWindow> OnNewWindowAdded;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<IHudWindow> OnWindowRemoved;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event HudWindowManager.DriverHelperInstantiatedDelegate EvtDriverHelperEnabled;

		public bool IsGameTransitioning
		{
			get
			{
				return this.State == GuiGameState.Transitioning;
			}
		}

		public GuiGameState State
		{
			get
			{
				return (!GameHubBehaviour.Hub.GuiScripts.Loading.IsLoading && !GameHubBehaviour.Hub.GuiScripts.LoadingVersus.WindowGameObject.activeInHierarchy) ? this._state : GuiGameState.Transitioning;
			}
			set
			{
				this._state = value;
			}
		}

		public static bool DoesInstanceExist()
		{
			return HudWindowManager._singleton != null;
		}

		public static HudWindowManager Instance
		{
			get
			{
				if (HudWindowManager._singleton != null)
				{
					return HudWindowManager._singleton;
				}
				if (GameHubBehaviour.Hub == null || !GameHubBehaviour.Hub.Net.IsClient())
				{
					Debug.LogWarning("[HUDWindowManager] - Dangerous stuff. Can cause an NPE if not expecting null.");
					return null;
				}
				HudWindowManager._singleton = GameHubBehaviour.Hub.GetComponent<HudWindowManager>();
				if (HudWindowManager._singleton == null)
				{
					HudWindowManager._singleton = GameHubBehaviour.Hub.gameObject.AddComponent<HudWindowManager>();
				}
				HudWindowManager._singleton.hudWindows = new List<IHudWindow>();
				return HudWindowManager._singleton;
			}
		}

		public void Push(IHudWindow addingWindow)
		{
			if (this._disableOpenWindows)
			{
				HudWindowManager.Log.Warn("Trying to Push IHudWindow when disableOpenWindows is true");
				return;
			}
			if (!addingWindow.CanOpen())
			{
				HudWindowManager.Log.Debug("Trying to Push IHudWindow when it cannot open");
				return;
			}
			if (addingWindow.CanBeHiddenByEscKey())
			{
				for (int i = this.hudWindows.Count - 1; i >= 0; i--)
				{
					IHudWindow hudWindow = this.hudWindows[i];
					if (hudWindow == null)
					{
						HudWindowManager.Log.Error("null IHudWindow found inside Push. This is unexpected behaviour");
						this.RemoveAt(i);
					}
					else if (hudWindow.CanBeHiddenByEscKey())
					{
						if (!hudWindow.IsStackableWithType(addingWindow.GetType()))
						{
							this.Remove(hudWindow);
						}
					}
				}
			}
			addingWindow.ChangeWindowVisibility(true);
			if (!this.hudWindows.Contains(addingWindow))
			{
				this.hudWindows.Add(addingWindow);
			}
			else
			{
				HudWindowManager.Log.Warn(string.Format("Trying add a hudwindow that is already in the list {0}", addingWindow));
			}
			this.hudWindows.Sort(new Comparison<IHudWindow>(this.HudWindwsCompare));
			if (this.OnNewWindowAdded != null)
			{
				this.OnNewWindowAdded(addingWindow);
			}
		}

		public int GetWindowsStackCount()
		{
			return this.hudWindows.Count;
		}

		private int HudWindwsCompare(IHudWindow x, IHudWindow y)
		{
			return x.GetDepth().CompareTo(y.GetDepth());
		}

		public void CloseAll()
		{
			this._disableOpenWindows = true;
			HudWindowManager.Log.Debug("CloseAll. disableOpenWindows true");
			for (int i = 0; i < this.hudWindows.Count; i++)
			{
				IHudWindow hudWindow = this.hudWindows[i];
				if (hudWindow == null)
				{
					HudWindowManager.Log.Error("null IHudWindow found inside CloseAll. This is unexpected behaviour");
				}
				else
				{
					if (hudWindow.IsWindowVisible())
					{
						HudWindowManager.Log.DebugFormat("Will close:{0}", new object[]
						{
							hudWindow.GetType()
						});
						hudWindow.ChangeWindowVisibility(false);
					}
					if (this.OnWindowRemoved != null)
					{
						this.OnWindowRemoved(hudWindow);
					}
				}
			}
			this.hudWindows.Clear();
			this._diContainer.Resolve<IMuteSystemPresenter>().Hide();
			this._disableOpenWindows = false;
			HudWindowManager.Log.Debug("CloseAll. disableOpenWindows false");
		}

		public bool IsWindowVisible<T>() where T : IHudWindow
		{
			bool result = false;
			for (int i = 0; i < this.hudWindows.Count; i++)
			{
				IHudWindow hudWindow = this.hudWindows[i];
				if (hudWindow is T)
				{
					result = hudWindow.IsWindowVisible();
					break;
				}
			}
			return result;
		}

		private void RemoveAt(int i)
		{
			IHudWindow obj = null;
			if (this.hudWindows[i] == null)
			{
				HudWindowManager.Log.Error("[RemoveAt] null IHudWindow found. Is this an unexpected behaviour? Maybe.");
			}
			else
			{
				obj = this.hudWindows[i];
			}
			this.hudWindows.RemoveAt(i);
			if (this.OnWindowRemoved != null)
			{
				this.OnWindowRemoved(obj);
			}
		}

		public void Remove(IHudWindow removingWindow)
		{
			if (removingWindow == null)
			{
				return;
			}
			if (removingWindow.IsWindowVisible())
			{
				HudWindowManager.Log.DebugFormat("Will close:{0}", new object[]
				{
					removingWindow.GetType()
				});
				removingWindow.ChangeWindowVisibility(false);
			}
			for (int i = this.hudWindows.Count - 1; i >= 0; i--)
			{
				if (removingWindow == this.hudWindows[i])
				{
					this.hudWindows.RemoveAt(i);
					break;
				}
			}
			if (this.OnWindowRemoved != null)
			{
				this.OnWindowRemoved(removingWindow);
			}
		}

		private void InstallListeners()
		{
			if (this._stateListenersInstalled)
			{
				return;
			}
			this._stateListenersInstalled = true;
			GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.MainMenu).ListenToStateSceneLevelLoaded += this.Game_ListenToStateSceneLevelLoaded;
			GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick).ListenToStateSceneLevelLoaded += this.Game_ListenToStateSceneLevelLoaded;
			Game game = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Game) as Game;
			game.OnGameOver += this.Game_OnGameOver;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManager_ListenToPhaseChange;
			HudWindowManager.Log.Debug("State listeners installed.");
		}

		private void OnEnable()
		{
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				this._driverHelperController = GameHubBehaviour.Hub.GuiScripts.DriverHelper;
			}
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.GameState_ListenToStateChanged;
			HudWindowManager.Log.Debug("Created");
		}

		private void OnDisable()
		{
			this._driverHelperController = null;
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.GameState_ListenToStateChanged;
			GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.MainMenu).ListenToStateSceneLevelLoaded -= this.Game_ListenToStateSceneLevelLoaded;
			GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick).ListenToStateSceneLevelLoaded -= this.Game_ListenToStateSceneLevelLoaded;
			Game game = GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Game) as Game;
			game.OnGameOver -= this.Game_OnGameOver;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManager_ListenToPhaseChange;
			this.CleanGameListeners();
			this._stateListenersInstalled = false;
			HudWindowManager.Log.Debug("Destroyed");
		}

		private void BombManager_ListenToPhaseChange(BombScoreboardState obj)
		{
			if (obj != BombScoreboardState.Warmup)
			{
				this.State = GuiGameState.Game;
			}
		}

		private void Game_OnGameOver(MatchData.MatchState matchWinner)
		{
			this.State = GuiGameState.Transitioning;
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.Remove(GameHubBehaviour.Hub.GuiScripts.Esc);
				return;
			}
			this.CloseAll();
		}

		private void GameState_ListenToStateChanged(GameState ChangedState)
		{
			this.CleanGameListeners();
			switch (GameHubBehaviour.Hub.State.Current.StateKind)
			{
			case GameState.GameStateKind.MainMenu:
				this.State = GuiGameState.MainMenu;
				return;
			case GameState.GameStateKind.Game:
				UIProgressionController.OnProgressionControllerVisibilityChange += this.EndGame_OnProgressionControllerVisibilityChange;
				if (this._spectatorService.IsSpectating)
				{
					ObservableExtensions.Subscribe<Unit>(this._spectatorHelper.LoadSpectatorHelper());
				}
				return;
			case GameState.GameStateKind.Profile:
				this.State = GuiGameState.Profile;
				return;
			case GameState.GameStateKind.Splash:
			case GameState.GameStateKind.HORTA:
			case GameState.GameStateKind.Welcome:
				this.InstallListeners();
				return;
			case GameState.GameStateKind.Loading:
				this.State = GuiGameState.Transitioning;
				this.CloseAll();
				return;
			case GameState.GameStateKind.Reconnect:
				this.State = GuiGameState.Reconnect;
				return;
			}
		}

		private void Game_ListenToStateSceneLevelLoaded()
		{
			GameState.GameStateKind stateKind = GameHubBehaviour.Hub.State.Current.StateKind;
			if (stateKind != GameState.GameStateKind.MainMenu)
			{
				if (stateKind == GameState.GameStateKind.Pick)
				{
					this.State = GuiGameState.PickScreen;
				}
			}
			else
			{
				this.State = GuiGameState.MainMenu;
			}
			this.CloseAll();
		}

		private void EndGame_OnProgressionControllerVisibilityChange(bool visibility)
		{
			if (visibility)
			{
				this.State = GuiGameState.Debriefing;
				return;
			}
			this.State = GuiGameState.Transitioning;
			this.CloseAll();
			this.CleanGameListeners();
		}

		private void Update()
		{
			if (!GameHubBehaviour.Hub || !GameHubBehaviour.Hub.State || GameHubBehaviour.Hub.Net.IsServer())
			{
				this.State = GuiGameState.Transitioning;
				return;
			}
			GuiGameState state = this.State;
			if (this.previous != state && this.OnGuiStateChange != null)
			{
				this.OnGuiStateChange(state);
			}
			this.previous = state;
			if (GameHubBehaviour.Hub.GuiScripts == null)
			{
				return;
			}
			if (SingletonMonoBehaviour<PanelController>.Instance.IsModalOfTypeOpened<StoreConfirmationWindow>())
			{
				return;
			}
			if (this.IsGameTransitioning)
			{
				return;
			}
			this.OptionsInputUpdate();
			this.HelpInputUpdate();
		}

		private void OptionsInputUpdate()
		{
			EscMenuGui esc = GameHubBehaviour.Hub.GuiScripts.Esc;
			this.UiNavigationOptionsInputUpdate(esc);
		}

		private void UiNavigationOptionsInputUpdate(EscMenuGui escMenuGui)
		{
			if (this._inputActionPoller.GetButtonDown(50))
			{
				this.TryToToggleOptionsWindowVisibility(escMenuGui);
			}
		}

		private void TryToToggleOptionsWindowVisibility(EscMenuGui escMenuGui)
		{
			if (this._optionsControllerTabPresenter.IsBinding())
			{
				return;
			}
			if (escMenuGui.IsOptionsWindowVisible())
			{
				escMenuGui.HideOptionsWindow();
			}
			else
			{
				escMenuGui.SetWindowVisibility(!escMenuGui.Visible);
			}
		}

		private void HelpInputUpdate()
		{
			if (!this._inputActionPoller.GetButtonDown(0))
			{
				return;
			}
			if (this.OnHelp != null)
			{
				this.OnHelp();
			}
			else if (this._driverHelperController)
			{
				if (this._driverHelperController.IsWindowVisible())
				{
					this.Remove(this._driverHelperController);
				}
				else
				{
					this.Push(this._driverHelperController);
					if (HudWindowManager.EvtDriverHelperEnabled != null)
					{
						HudWindowManager.EvtDriverHelperEnabled();
					}
				}
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.CloseAll();
			this.CleanGameListeners();
		}

		private void OnDestroy()
		{
			this.OnGuiStateChange = null;
		}

		public void CleanGameListeners()
		{
			UIProgressionController.OnProgressionControllerVisibilityChange -= this.EndGame_OnProgressionControllerVisibilityChange;
		}

		private const string AddFriendsSceneName = "UI_ADD_Social_Add_friends";

		private static readonly BitLogger Log = new BitLogger(typeof(HudWindowManager));

		[InjectOnClient]
		private IControllerInputActionPoller _inputActionPoller;

		[InjectOnClient]
		private ISpectatorService _spectatorService;

		[InjectOnClient]
		private ISpectatorHelperFactory _spectatorHelper;

		[InjectOnClient]
		private IIsFeatureToggled _isFeatureToggled;

		[InjectOnClient]
		private IUiNavigationInputDeliverer _uiNavigationInputDeliverer;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private IOptionsControllerTabPresenter _optionsControllerTabPresenter;

		[InjectOnClient]
		private DiContainer _diContainer;

		private bool _stateListenersInstalled;

		private static HudWindowManager _singleton;

		private GuiGameState _state;

		private DriverHelperController _driverHelperController;

		private List<IHudWindow> hudWindows;

		private GuiGameState previous;

		private bool _disableOpenWindows;

		public delegate void DriverHelperInstantiatedDelegate();
	}
}
