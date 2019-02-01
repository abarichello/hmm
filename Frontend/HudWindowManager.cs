using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.VFX;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudWindowManager : GameHubBehaviour, ICleanupListener
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event HudWindowManager.GuiGameStateChange OnGuiStateChange;

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
				return this.State == HudWindowManager.GuiGameState.Transitioning;
			}
		}

		public HudWindowManager.GuiGameState State
		{
			get
			{
				return (!GameHubBehaviour.Hub.GuiScripts.Loading.IsLoading && !GameHubBehaviour.Hub.GuiScripts.LoadingVersus.WindowGameObject.activeInHierarchy) ? this._state : HudWindowManager.GuiGameState.Transitioning;
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
					UnityEngine.Debug.LogWarning("[HUDWindowManager] - Dangerous stuff. Can cause an NPE if not expecting null.");
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

		public void CloseAll(Func<IHudWindow, bool> filter = null)
		{
			this._disableOpenWindows = true;
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
						hudWindow.ChangeWindowVisibility(false);
					}
					if (this.OnWindowRemoved != null)
					{
						this.OnWindowRemoved(hudWindow);
					}
				}
			}
			if (filter != null)
			{
				this.hudWindows.RemoveAll((IHudWindow x) => !filter(x));
			}
			else
			{
				this.hudWindows.Clear();
			}
			this._disableOpenWindows = false;
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
		}

		private void OnEnable()
		{
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				this._driverHelperController = GameHubBehaviour.Hub.GuiScripts.DriverHelper;
			}
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.GameState_ListenToStateChanged;
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
		}

		private void BombManager_ListenToPhaseChange(BombScoreBoard.State obj)
		{
			if (obj != BombScoreBoard.State.Warmup)
			{
				this.State = HudWindowManager.GuiGameState.Game;
			}
		}

		private void Game_OnGameOver(MatchData.MatchState matchWinner)
		{
			this.State = HudWindowManager.GuiGameState.Transitioning;
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.Remove(GameHubBehaviour.Hub.GuiScripts.Esc);
				return;
			}
			this.CloseAll(null);
		}

		private void GameState_ListenToStateChanged(GameState ChangedState)
		{
			this.CleanGameListeners();
			switch (GameHubBehaviour.Hub.State.Current.StateKind)
			{
			case GameState.GameStateKind.MainMenu:
				this.State = HudWindowManager.GuiGameState.MainMenu;
				return;
			case GameState.GameStateKind.Game:
				UIProgressionController.OnProgressionControllerVisibilityChange += this.EndGame_OnProgressionControllerVisibilityChange;
				return;
			case GameState.GameStateKind.Splash:
			case GameState.GameStateKind.HORTA:
				this.InstallListeners();
				return;
			case GameState.GameStateKind.Loading:
				this.State = HudWindowManager.GuiGameState.Transitioning;
				this.CloseAll(null);
				return;
			case GameState.GameStateKind.Reconnect:
				this.State = HudWindowManager.GuiGameState.Reconnect;
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
					this.State = HudWindowManager.GuiGameState.PickScreen;
				}
			}
			else
			{
				this.State = HudWindowManager.GuiGameState.MainMenu;
			}
			this.CloseAll(null);
		}

		private void EndGame_OnProgressionControllerVisibilityChange(bool visibility)
		{
			if (visibility)
			{
				this.State = HudWindowManager.GuiGameState.Debriefing;
				return;
			}
			this.State = HudWindowManager.GuiGameState.Transitioning;
			this.CloseAll(null);
			this.CleanGameListeners();
		}

		private void Update()
		{
			if (!GameHubBehaviour.Hub || !GameHubBehaviour.Hub.State || GameHubBehaviour.Hub.Net.IsServer())
			{
				this.State = HudWindowManager.GuiGameState.Transitioning;
				return;
			}
			HudWindowManager.GuiGameState state = this.State;
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
			if (!Input.GetKeyDown(KeyCode.Escape))
			{
				if (ControlOptions.GetButtonDown(ControlAction.Help) && this._driverHelperController)
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
				return;
			}
			if (this.hudWindows.Count == 0)
			{
				this.Push(GameHubBehaviour.Hub.GuiScripts.Esc);
				return;
			}
			for (int i = this.hudWindows.Count - 1; i >= 0; i--)
			{
				if (this.hudWindows[i].CanBeHiddenByEscKey())
				{
					this.Remove(this.hudWindows[i]);
					return;
				}
			}
			this.Push(GameHubBehaviour.Hub.GuiScripts.Esc);
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.CloseAll(null);
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

		private static readonly BitLogger Log = new BitLogger(typeof(HudWindowManager));

		private bool _stateListenersInstalled;

		private static HudWindowManager _singleton;

		private HudWindowManager.GuiGameState _state;

		private DriverHelperController _driverHelperController;

		private List<IHudWindow> hudWindows;

		private HudWindowManager.GuiGameState previous;

		private bool _disableOpenWindows;

		public delegate void GuiGameStateChange(HudWindowManager.GuiGameState currentGuiGameState);

		public enum GuiGameState
		{
			Transitioning,
			MainMenu,
			PickScreen,
			Game,
			Debriefing,
			Reconnect
		}

		public delegate void DriverHelperInstantiatedDelegate();
	}
}
