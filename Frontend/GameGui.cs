using System;
using System.Collections;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.CharacterHelp.Presenting;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Input.NoInputDetection.Presenting;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MuteSystem;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.QuickChat;
using HeavyMetalMachines.RadialMenu.View;
using HeavymetalMachines.ReportSystem;
using HeavyMetalMachines.VFX;
using Hoplon.Input;
using Hoplon.Input.UiNavigation;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class GameGui : StateGuiController
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action ListenToGameGuiCreation;

		private UiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void Awake()
		{
			this.UiNavigationGroupHolder.AddGroup();
			this._inputCancelDownDisposable = ObservableExtensions.Subscribe<Unit>(this.UiNavigationGroupHolder.ObserveInputCancelDown(), delegate(Unit _)
			{
				if (this._inputGetActiveDevicePoller.GetActiveDevice() != 3)
				{
					this._optionsPresenter.Show();
				}
			});
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.CursorManager.ShowAndSetCursor(true, CursorManager.CursorTypes.GameCursor);
			this.HudChatController.gameObject.SetActive(!GameHubBehaviour.Hub.Match.LevelIsTutorial());
			if (GameGui.ListenToGameGuiCreation != null)
			{
				GameGui.ListenToGameGuiCreation();
				GameGui.ListenToGameGuiCreation = null;
			}
			this.HudTabController.OnVisibilityChange += this.OnOtherVisibilityChange;
			SpectatorModalGUI.OnModalVisibilityChanged += this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.OnVisibilityChange += this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.OnVisibilityChange += this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn += this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning += this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn += this.ShowElementsAfterRespawn;
		}

		private void OnDisable()
		{
			this.HudTabController.OnVisibilityChange -= this.OnOtherVisibilityChange;
			SpectatorModalGUI.OnModalVisibilityChanged -= this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.OnVisibilityChange -= this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.OnVisibilityChange -= this.OnOtherVisibilityChange;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToPreObjectSpawn -= this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectRespawning -= this.HideSomeElementsToRespawn;
			GameHubBehaviour.Hub.Events.Players.ListenToObjectSpawn -= this.ShowElementsAfterRespawn;
		}

		private void Start()
		{
			GameGui.StaticUIGadgetConstructor = this.UIGadgetConstructor;
			if (this._configLoader.GetBoolValue(ConfigAccess.HORTA))
			{
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.MainMenuCursor);
				this._hortaComponent.ShowTimelineWindow();
			}
			this.InitializeEmotesMenuPresenter();
			this.InitializeQuickChatMenu();
			this.InitializeOverlayShowing();
			this.InitializeMuteSystemPresenter();
			this.InitializeCharacterHelp();
		}

		private void InitializeCharacterHelp()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial() || SpectatorController.IsSpectating)
			{
				return;
			}
			this._characterHelpPresenter.Set(GameHubBehaviour.Hub.Players.CurrentPlayerData.CharacterItemType.Id);
		}

		private void InitializeEmotesMenuPresenter()
		{
			this._emotesMenuPresenter = this._diContainer.Resolve<IEmotesMenuPresenter>();
			ObservableExtensions.Subscribe<Unit>(this._emotesMenuPresenter.Initialize());
		}

		private void InitializeQuickChatMenu()
		{
			this._quickChatMenuPresenter = this._diContainer.Resolve<IQuickChatMenuPresenter>();
			ObservableExtensions.Subscribe<Unit>(this._quickChatMenuPresenter.Initialize());
		}

		private IObservable<Unit> InitializeReportSystemPresenter()
		{
			this._retortSystemPresenter = this._diContainer.Resolve<IReportSystemPresenter>();
			return this._retortSystemPresenter.Initialize();
		}

		private void InitializeMuteSystemPresenter()
		{
			this._muteSystemPresenter = this._diContainer.Resolve<IMuteSystemPresenter>();
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this.InitializeReportSystemPresenter(), this._muteSystemPresenter.Initialize()));
		}

		private void InitializeOverlayShowing()
		{
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(Observable.Switch<Unit>(Observable.Select<bool, IObservable<Unit>>(this._diContainer.Resolve<ICanShowInGameOverlay>().GetThenObserveCanShow(), new Func<bool, IObservable<Unit>>(this.ListenAndExecuteOverlays)))), this);
		}

		private IObservable<Unit> ListenAndExecuteOverlays(bool canShowOverlays)
		{
			if (!canShowOverlays)
			{
				return Observable.ReturnUnit();
			}
			return Observable.Repeat<Unit>(Observable.ContinueWith<IObservable<Unit>, Unit>(Observable.First<IObservable<Unit>>(Observable.Merge<IObservable<Unit>>(new IObservable<IObservable<Unit>>[]
			{
				this.ListenAndExecuteEmotesPresenter(),
				this.ListenAndExecuteQuickChatPresenter()
			})), (IObservable<Unit> operation) => operation));
		}

		private IObservable<IObservable<Unit>> ListenAndExecuteEmotesPresenter()
		{
			return this.ListenAndExecuteRadialMenuPresenter(this._onShowHideEmotesMenu, this._emotesMenuPresenter);
		}

		private IObservable<IObservable<Unit>> ListenAndExecuteQuickChatPresenter()
		{
			return this.ListenAndExecuteRadialMenuPresenter(this._onShowHideQuickChatMenu, this._quickChatMenuPresenter);
		}

		private IObservable<IObservable<Unit>> ListenAndExecuteRadialMenuPresenter(IObservable<bool> onShowHideMenu, IRadialMenuPresenter radialMenuPresenter)
		{
			return Observable.Select<Unit, IObservable<Unit>>(Observable.First<Unit>(this.OnRadialMenuButtonDown(onShowHideMenu)), delegate(Unit _)
			{
				radialMenuPresenter.Show();
				return Observable.AsUnitObservable<Unit>(Observable.DoOnTerminate<Unit>(Observable.DoOnCancel<Unit>(Observable.Do<Unit>(this.WaitForEmoteInput(onShowHideMenu, radialMenuPresenter), delegate(Unit __)
				{
					radialMenuPresenter.SendSelectedItem();
				}), delegate()
				{
					radialMenuPresenter.Hide();
				}), delegate()
				{
					radialMenuPresenter.Hide();
				}));
			});
		}

		private IObservable<Unit> WaitForEmoteInput(IObservable<bool> onShowHideMenu, IRadialMenuPresenter radialMenuPresenter)
		{
			if (this._inputGetActiveDevicePoller.GetActiveDevice() == 3)
			{
				return Observable.TakeUntil<Unit, Unit>(Observable.First<Unit>(this.OnRadialMenuConfirmed(onShowHideMenu, radialMenuPresenter)), this.OnRadialMenuCanceled(radialMenuPresenter));
			}
			return Observable.AsUnitObservable<bool>(Observable.First<bool>(onShowHideMenu, (bool shouldShow) => !shouldShow));
		}

		private IObservable<Unit> OnRadialMenuButtonDown(IObservable<bool> onShowHideMenu)
		{
			return Observable.AsUnitObservable<bool>(Observable.ContinueWith<bool, bool>(Observable.First<bool>(onShowHideMenu, (bool shouldShow) => !shouldShow), Observable.Where<bool>(onShowHideMenu, (bool shouldShow) => shouldShow)));
		}

		private IObservable<Unit> OnRadialMenuConfirmed(IObservable<bool> onShowHideMenu, IRadialMenuPresenter radialMenuPresenter)
		{
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				Observable.AsUnitObservable<Unit>(this.OnRadialMenuButtonDown(onShowHideMenu)),
				radialMenuPresenter.OnConfirmed()
			});
		}

		private IObservable<Unit> OnRadialMenuCanceled(IRadialMenuPresenter radialMenuPresenter)
		{
			return radialMenuPresenter.OnCanceled();
		}

		private void OnDestroy()
		{
			this.DisposeEmoteMenu();
			this.DisposeQuickChatMenu();
			this.DisposeMuteSystem();
			this.DisposeReportSystem();
			GameGui.ListenToGameGuiCreation = null;
			if (this._configLoader.GetBoolValue(ConfigAccess.HORTA))
			{
				if (GameHubBehaviour.Hub.CursorManager != null)
				{
					GameHubBehaviour.Hub.CursorManager.Pop();
				}
				this._hortaComponent.DisposeTimelineWindow();
			}
			this.UiNavigationGroupHolder.RemoveGroup();
			if (this._inputCancelDownDisposable != null)
			{
				this._inputCancelDownDisposable.Dispose();
				this._inputCancelDownDisposable = null;
			}
		}

		public void ShowEmotesMenu()
		{
			this._onShowHideEmotesMenu.OnNext(true);
		}

		public void HideEmotesMenu()
		{
			this._onShowHideEmotesMenu.OnNext(false);
		}

		public void ShowQuickChatMenu()
		{
			this._onShowHideQuickChatMenu.OnNext(true);
		}

		public void HideQuickChatMenu()
		{
			this._onShowHideQuickChatMenu.OnNext(false);
		}

		private void DisposeEmoteMenu()
		{
			ObservableExtensions.Subscribe<Unit>(this._emotesMenuPresenter.Dispose());
		}

		private void DisposeQuickChatMenu()
		{
			ObservableExtensions.Subscribe<Unit>(this._quickChatMenuPresenter.Dispose());
		}

		private void DisposeMuteSystem()
		{
			ObservableExtensions.Subscribe<Unit>(this._muteSystemPresenter.Dispose());
		}

		private void DisposeReportSystem()
		{
			ObservableExtensions.Subscribe<Unit>(this._retortSystemPresenter.Dispose());
		}

		private void HideSomeElementsToRespawn(PlayerEvent data)
		{
			if (data.TargetId != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId || GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery || GameHubBehaviour.Hub.Players.CurrentPlayerData.IsBotControlled)
			{
				return;
			}
			GameGui.HudElement hudElements = (GameGui.HudElement)2147483479;
			this.SetHudVisibility(hudElements, false);
		}

		private void ShowElementsAfterRespawn(PlayerEvent data)
		{
			if (data.TargetId != GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId || GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.BombDelivery || GameHubBehaviour.Hub.Players.CurrentPlayerData.IsBotControlled)
			{
				return;
			}
			this.SetHudVisibility(GameGui.HudElement.All, true);
		}

		public void ClearBackToMain()
		{
			((Game)GameHubBehaviour.Hub.State.Current).ClearBackToMain();
		}

		private void Minimize()
		{
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.Minimize();
		}

		public void OnFriendInviteSent()
		{
			GameGui.Log.Debug("OnFriendInviteSent - show");
			this.OkWindowFeedback("InviteSent", TranslationContext.MainMenuGui, new object[0]);
		}

		public void OkWindowFeedback(string key, ContextTag tab, params object[] param)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.GetFormatted(key, tab, param),
				OkButtonText = Language.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void OnOtherVisibilityChange(bool otherVisibility)
		{
			this.ShowGameHud(!otherVisibility);
		}

		public void ShowGameHud(bool visibility)
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return;
			}
			bool flag = GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.BombDelivery || GameHubBehaviour.Hub.BombManager.CurrentBombGameState == BombScoreboardState.PreBomb;
			bool flag2 = visibility && flag && !GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible() && !this.HudTabController.IsWindowVisible() && !GameHubBehaviour.Hub.GuiScripts.DriverHelper.IsWindowVisible();
			this.SetHudVisibility(GameGui.HudElement.All, flag2);
			GameGui.Log.DebugFormat("Exit ShowGameHud. Visibility: {0} ShouldShow: {1}", new object[]
			{
				visibility,
				flag2
			});
		}

		public void SetHudVisibility(GameGui.HudElement hudElements, bool visibility)
		{
			if (hudElements.HasFlag(GameGui.HudElement.Gadget))
			{
				this.UIGadgetConstructor.SetWindowVisibility(visibility);
			}
			if (hudElements.HasFlag(GameGui.HudElement.MiniMap) && this.HudMinimapUiController != null)
			{
				this.HudMinimapUiController.SetVisibility(visibility, false);
			}
			if (hudElements.HasFlag(GameGui.HudElement.Players))
			{
				this.HudPlayersController.SetWindowVisibility(visibility);
			}
			if (hudElements.HasFlag(GameGui.HudElement.TopScore))
			{
				this.HudScoreController.SetWindowVisibility(visibility);
				if (this.OvertimeTextController != null)
				{
					this.OvertimeTextController.SetVisibility(visibility);
				}
			}
			if (hudElements.HasFlag(GameGui.HudElement.Respawn))
			{
				this.RespawnController.SetVisibility(visibility);
			}
		}

		public void OnEndGame()
		{
			ObservableExtensions.Subscribe<Unit>(this._characterHelpPresenter.Hide());
		}

		public void ShowEndGameBackground(Action callback)
		{
			base.StartCoroutine(this.ShowEndGameBackgroundCoroutine(callback));
		}

		private IEnumerator ShowEndGameBackgroundCoroutine(Action callback)
		{
			this._endGameBackground.gameObject.SetActive(true);
			TweenAlpha.Begin(this._endGameBackground.gameObject, 0.5f, 1f);
			yield return new WaitForSeconds(0.5f);
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GameGui));

		[InjectOnClient]
		private DiContainer _diContainer;

		public HudChatController HudChatController;

		public HudLifebarController HudLifebarController;

		public HudTabController HudTabController;

		public CombatTextManager CombatTextManager;

		public HudMegafeedbacksController HudMegafeedbacksController;

		public BombTipWindow bombTipWindow;

		public BattlepassProgressScriptableObject BattlepassProgressScriptableObject;

		[NonSerialized]
		public UIProgressionController EndGame;

		[NonSerialized]
		public HudWinnerController HudWinnerController;

		[Header("Game Hud Elements To Be Show OnStartup")]
		[SerializeField]
		public HudPlayersController HudPlayersController;

		[SerializeField]
		public UIGadgetConstructor UIGadgetConstructor;

		public GadgetHud GadgetHud;

		[SerializeField]
		public HudMinimapUiController HudMinimapUiController;

		[SerializeField]
		public HudScoreController HudScoreController;

		[SerializeField]
		public OvertimeTextController OvertimeTextController;

		[SerializeField]
		public HudRespawnController RespawnController;

		public GameObject Hud;

		public HudKillfeedControllerUnityUI KillFeedController;

		[Header("[End Game]")]
		[SerializeField]
		private UI2DSprite _endGameBackground;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		public static UIGadgetConstructor StaticUIGadgetConstructor;

		private IEmotesMenuPresenter _emotesMenuPresenter;

		private IQuickChatMenuPresenter _quickChatMenuPresenter;

		private IMuteSystemPresenter _muteSystemPresenter;

		private INoInputDetectedPresenter _noInputDetectedPresenter;

		private IReportSystemPresenter _retortSystemPresenter;

		private readonly Subject<bool> _onShowHideEmotesMenu = new Subject<bool>();

		private readonly Subject<bool> _onShowHideQuickChatMenu = new Subject<bool>();

		[InjectOnClient]
		private IConfigLoader _configLoader;

		[InjectOnClient]
		private HORTAComponent _hortaComponent;

		[InjectOnClient]
		private IOptionsPresenter _optionsPresenter;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private ICharacterHelpPresenter _characterHelpPresenter;

		private IDisposable _inputCancelDownDisposable;

		[Flags]
		public enum HudElement
		{
			None = 0,
			Portrait = 1,
			Gadget = 2,
			MiniMap = 4,
			TopScore = 8,
			ShopButton = 16,
			Players = 32,
			Radar = 64,
			Respawn = 128,
			All = 2147483647
		}
	}
}
