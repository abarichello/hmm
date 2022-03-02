using System;
using System.Collections.Generic;
using System.Diagnostics;
using ClientAPI.Objects;
using FMod;
using HeavyMetalMachines.Audio.Music;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Customization;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.MainMenuPresenting;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.OpenUrl;
using HeavyMetalMachines.ParentalControl;
using HeavyMetalMachines.Players.Business;
using HeavyMetalMachines.Presenting.Navigation;
using HeavyMetalMachines.VFX;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class MainMenuGui : StateGuiController
	{
		public MainMenu Main
		{
			get
			{
				if (this._main == null)
				{
					this._main = (MainMenu)GameHubBehaviour.Hub.State.Current;
				}
				return this._main;
			}
			set
			{
				this._main = value;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action<bool> OnLobbyUpdate;

		protected void OnEnable()
		{
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientConnectedEvent += this.OnSearchingForAMatch;
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientDisconnectedEvent += this.OnClientDisconnectedFromMatchMaking;
			GameHubBehaviour.Hub.CursorManager.ShowAndSetCursor(true, CursorManager.CursorTypes.MainMenuCursor);
			this.CheckHardCurrencyBought();
			if (GameHubBehaviour.Hub.State.Last is Game)
			{
				SingletonMonoBehaviour<RegionController>.Instance.TryRequestRegionsPing(false);
			}
			this._lockButtons = false;
			BattlepassComponent battlepassComponent = this._battlepassComponent;
			battlepassComponent.OnBattlepassTransactionSuccess = (Action)Delegate.Combine(battlepassComponent.OnBattlepassTransactionSuccess, new Action(this.OnBattlepassRequestMainMenuData));
		}

		protected void Start()
		{
			GameHubBehaviour.Hub.User.IsReconnecting = false;
		}

		private void CheckHardCurrencyBought()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				return;
			}
			GameHubBehaviour.Hub.ClientApi.billing.GetMyProductsNotSaw(null, delegate(object state, UserHardCurrencyProduct[] hardCurrencyProducts)
			{
				List<long> list = new List<long>(hardCurrencyProducts.Length);
				if (hardCurrencyProducts.Length == 0)
				{
					return;
				}
				UserHardCurrencyProduct userHardCurrencyProduct = hardCurrencyProducts[0];
				MainMenuGui.Log.DebugFormat("ClientApi billing GetMyProductsNotSaw. Item:[{0}]", new object[]
				{
					userHardCurrencyProduct.Id
				});
				list.Add(userHardCurrencyProduct.Id);
				string text = string.Empty;
				for (int i = 0; i < userHardCurrencyProduct.Images.Length; i++)
				{
					HardCurrencyProductImage hardCurrencyProductImage = userHardCurrencyProduct.Images[i];
					if (hardCurrencyProductImage.Type == "in-game")
					{
						text = userHardCurrencyProduct.Images[i].Url;
						break;
					}
				}
				GameHubBehaviour.Hub.ClientApi.billing.UpdateUserSawProductList(null, list.ToArray(), delegate(object obj)
				{
					MainMenuGui.Log.Debug("ClientApi billing UpdateUserSawProductList done.");
				}, delegate(object obj, Exception exception)
				{
					MainMenuGui.Log.ErrorFormat("Error on LoadItems. Swordfish UpdateUserSawProductList - exception: {0}", new object[]
					{
						exception
					});
				});
			}, delegate(object state, Exception exception)
			{
				MainMenuGui.Log.ErrorFormat("Error on LoadItems. Swordfish GetMyProductsNotSaw - exception: {0}", new object[]
				{
					exception
				});
			});
		}

		public void OnAllItemsReload()
		{
		}

		public void JoinQueue(GameModeTabs queue)
		{
			this.Main.SearchForAMatch(queue.ToString());
		}

		protected void OnDestroy()
		{
			this.Shop.CleanUp();
			this._battlepassDetailComponent.HideDetailWindow(false);
			if (this._mainMenuPresenterTree != null)
			{
				this._mainMenuPresenterTree.Dispose();
			}
			if (this._tryToOpenMetalpassWindowDisposable != null)
			{
				this._tryToOpenMetalpassWindowDisposable.Dispose();
			}
		}

		protected void OnDisable()
		{
			try
			{
				if (GameHubBehaviour.Hub.Swordfish.Msg.Ready)
				{
					GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientConnectedEvent -= this.OnSearchingForAMatch;
					GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientDisconnectedEvent -= this.OnClientDisconnectedFromMatchMaking;
				}
				if (this._disposableCustomizationChange != null)
				{
					this._disposableCustomizationChange.Dispose();
					this._disposableCustomizationChange = null;
				}
				GameHubBehaviour.Hub.GuiScripts.Loading.OnHidingAnimationCompleted -= this.AnimateEnterMainMenu;
				UICamera.onCustomInput = null;
				BattlepassComponent battlepassComponent = this._battlepassComponent;
				battlepassComponent.OnBattlepassTransactionSuccess = (Action)Delegate.Remove(battlepassComponent.OnBattlepassTransactionSuccess, new Action(this.OnBattlepassRequestMainMenuData));
			}
			catch (NullReferenceException ex)
			{
				if (HMMHub.IsEditorLeavingPlayMode())
				{
					MainMenuGui.Log.Warn(string.Format("Safe to ignore exception, since we are in editor: {0}", ex));
				}
				else
				{
					MainMenuGui.Log.Error(ex);
				}
			}
		}

		public void AnimateEnterMainMenu()
		{
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.AnimateEnterMainMenu();
			this.PlayMainMenuTheme();
			this.UpdateFounderInfo();
			this.OnReturnLobby();
		}

		private void CheckAndGoToNodeOnInicialization()
		{
			switch (this._mainMenuInitialization.NodeToGo)
			{
			case MainMenuNode.None:
				break;
			case MainMenuNode.TrainingScreen:
				ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.MainMenuTrainingSelectionNode));
				goto IL_B5;
			case MainMenuNode.Battlepass:
				ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.BattlepassNode));
				goto IL_B5;
			case MainMenuNode.AutoMatch:
				this.JoinQueue(this._autoMatch.GetGameModeTab());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.MainMenuNode));
			IL_B5:
			this._mainMenuInitialization.ClearNodeToGo();
		}

		private void InitializeMainMenuPresenterTree()
		{
			this._mainMenuPresenterTree = this._diContainer.Resolve<IMainMenuPresenterTree>();
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.Initialize(), delegate(Unit _)
			{
				this.CheckAndGoToNodeOnInicialization();
			});
			ObservableExtensions.Subscribe<IPresenterNode>(this._mainMenuPresenterTree.PresenterTree.ObserveNodeLeave(this._mainMenuPresenterTree.MainMenuNode));
		}

		public void AnimateReturnToLobby(bool forceMatchmakingStateUpdate = false, bool forcePlayButtonUnlock = false)
		{
			if (forceMatchmakingStateUpdate)
			{
				this.MatchStats.UpdateMatchmakingNow();
			}
			this.OnReturnLobby();
		}

		public void AnimateReturnToLobbySearchingMatch()
		{
			this.OnReturnLobby();
		}

		private void OnReturnLobby()
		{
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.MainMenuNode));
			this._isInLobby = true;
			this._lockButtons = false;
			this.GameModesGui.ForceHideGameModesGuiBackground();
			this.DispatchLobbyUpdatedEvent();
		}

		public void SearchForAMatch()
		{
			GameModeTabs gameMode = this.GameModesGui.GameMode;
			this.Main.SearchForAMatch(gameMode.ToString());
		}

		private void OnSearchingForAMatch()
		{
			this.MatchStats.AnimateShowMatchSearchingPanel();
			string queueName = this.GetQueueName();
			if (queueName != null)
			{
				if (queueName == "Normal" || queueName == "Novice" || queueName == "NormalPSN" || queueName == "NormalXboxLive" || queueName == "CoopVsBots")
				{
					goto IL_B4;
				}
				if (queueName == "CustomMatch")
				{
					MainMenuGui.Log.Warn("Calling custom match inside OnSearchingForAMatch");
					goto IL_B4;
				}
			}
			MainMenuGui.Log.WarnFormat("Unknown queue name: {0}", new object[]
			{
				queueName
			});
			IL_B4:
			if (this.GameModesGui.RootGameObject.activeSelf)
			{
				this.GameModesGui.ReturnToMainMenu();
			}
		}

		public void OnMatchFound()
		{
			HudWindowManager.Instance.CloseAll();
			FMODAudioManager.PlayOneShotAt(this.MatchFoundSfx, Vector3.zero, 0);
		}

		public string GetQueueName()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				return string.Empty;
			}
			return GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.MatchMadeQueue;
		}

		[Obsolete("Use MainMenuPresenterTree.PlayModesNode")]
		public void OnPlayButtonClick()
		{
			MainMenuGui.Log.Warn("calling obsolete OnPlayButtonClick. Use MainMenuPresenterTree.PlayModesNode");
		}

		[Obsolete]
		private void SetTutorialButtonState(bool isEnabled, string reason = null)
		{
			MainMenuGui.Log.WarnStackTrace("calling obsolete SetTutorialButtonState.");
		}

		private void PlayMainMenuTheme()
		{
			MusicManager.PlayMusic(MusicManager.State.MainMenu);
		}

		public void CancelMatchMaking()
		{
			this.Main.CancelMatchMaking();
		}

		public void OnClientDisconnectedFromMatchMaking()
		{
			MainMenuGui.Log.Info("OnClientDisconnectedFromMatchMaking");
			if (this.MatchStats.MMInterfaceState != MatchStatsGui.MatchmakingInterfaceState.None && this.MatchStats.MMInterfaceState != MatchStatsGui.MatchmakingInterfaceState.WaitingServer)
			{
				this.MatchStats.AnimateHideMatchSearchingPanel();
			}
		}

		public void RejectMatch()
		{
			this.Main.SendRejectMatch();
			this.MatchStats.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting;
		}

		public void ClearCurrentServer()
		{
			this.Main.ClearCurrentServer();
		}

		public void SendMatchAccepted(string queueName)
		{
			this.Main.SendMatchAccepted(queueName);
		}

		[Obsolete("Use MainMenuPresenterTree.StoreNode")]
		public void OnShopButtonClick()
		{
			MainMenuGui.Log.Warn("calling obsolete OnShopButtonClick. Use MainMenuPresenterTree.StoreNode");
		}

		[Obsolete("Use MainMenuPresenterTree.StoreNode")]
		public void OpenShop(ShopGUI.Tab tab, Guid itemTypeId)
		{
			MainMenuGui.Log.Warn("calling obsolete OnShopButtonClick. Use MainMenuPresenterTree.StoreNode");
		}

		[Obsolete("Use MainMenuPresenterTree.StoreCashNode")]
		public void OnCashTopButtonClick()
		{
			MainMenuGui.Log.Warn("calling obsolete OnCashTopButtonClick. Use MainMenuPresenterTree.StoreCashNode");
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.StoreCashNode), delegate(Unit _)
			{
				MainMenuGui.Log.Debug("OnCashTopButtonClick navigation end");
			});
		}

		[Obsolete("Use MainMenuPresenterTree.MainMenu")]
		public void ReturnToMainMenuLobby()
		{
			MainMenuGui.Log.Warn("calling obsolete ReturnToMainMenuLobby. Use MainMenuPresenterTree.MainMenu");
		}

		[Obsolete("Use MainMenuPresenterTree.ProfileNode")]
		public void OnProfileButtonClick()
		{
			MainMenuGui.Log.Warn("calling obsolete OnProfileButtonClick. Use MainMenuPresenterTree.ProfileNode");
		}

		[Obsolete("Use MainMenuPresenterTree.BattlepassNode")]
		public void OnMetalpassButtonClick()
		{
			MainMenuGui.Log.Warn("calling obsolete OnMetalpassButtonClick. Use MainMenuPresenterTree.BattlepassNode");
		}

		[Obsolete("Use MainMenuPresenterTree.BattlepassNode")]
		public void TryToOpenMetallpassPremiumShopWindow()
		{
			MainMenuGui.Log.WarnStackTrace("calling obsolete TryToOpenMetallpassPremiumShopWindow. Use MainMenuPresenterTree.BattlepassNode");
		}

		[Obsolete("Use MainMenuPresenterTree.BattlepassNode")]
		private void OpenMetallpassWindow(bool showPremiumShop = false)
		{
			MainMenuGui.Log.WarnStackTrace("calling obsolete OpenMetallpassWindow. Use MainMenuPresenterTree.BattlepassNode");
			ObservableExtensions.Subscribe<Unit>(this._mainMenuPresenterTree.PresenterTree.NavigateToNode(this._mainMenuPresenterTree.BattlepassNode));
		}

		[Obsolete("Use MainMenuPresenterTree.BattlepassNode")]
		private void CheckForOpenAutomaticMetalpassWindow()
		{
			MainMenuGui.Log.WarnStackTrace("calling obsolete CheckForOpenAutomaticMetalpassWindow. Use MainMenuPresenterTree.BattlepassNode");
		}

		[Obsolete("Use MainMenuPresenterTree.BattlepassNode")]
		private void TryToOpenMetallpassWindow()
		{
			MainMenuGui.Log.WarnStackTrace("calling obsolete TryToOpenMetallpassWindow. Use MainMenuPresenterTree.BattlepassNode");
		}

		[Obsolete("Use MainMenuPresenterTree.InventoryNode")]
		public void OnInventoryButtonClick()
		{
			MainMenuGui.Log.WarnStackTrace("calling obsolete OnInventoryButtonClick. Use MainMenuPresenterTree.InventoryNode");
		}

		private void OnInventoryWindowClosed()
		{
			this.AnimateReturnToLobby(false, false);
		}

		public void OnHelpButtonClick()
		{
			this._openUrl.Open(4);
			this._buttonBILogger.LogButtonClick(ButtonName.Help);
		}

		public void OnLeaderboardButtonClick()
		{
			ObservableExtensions.Subscribe<bool>(this._openUrlUgcRestricted.OpenUrlAfterRestrictionCheck(6));
			this._buttonBILogger.LogButtonClick(ButtonName.Leaderboard);
		}

		public void NotPartyOwnerFeedback()
		{
			MainMenuGui.OkWindowFeedback("YouAreNotPartyOwner", TranslationContext.MainMenuGui, new object[0]);
		}

		public void PlayerAlreadyInvitedToThePartyFeedback(string playerName)
		{
			MainMenuGui.OkWindowFeedback("PlayerAlreadyInvitedToTheParty", TranslationContext.MainMenuGui, new object[]
			{
				playerName
			});
		}

		public void PlayerAlreadyJoinedPartyFeedback(string playerName)
		{
			MainMenuGui.OkWindowFeedback("PlayerAlreadyJoinedParty", TranslationContext.MainMenuGui, new object[]
			{
				playerName
			});
		}

		public static void OkWindowFeedback(string key, ContextTag tab, params object[] param)
		{
			MainMenuGui.OkWindowFeedback(key, string.Empty, tab, param);
		}

		public static void OkWindowFeedback(string questionKey, string titleKey, ContextTag tab, params object[] questionParams)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.GetFormatted(questionKey, tab, questionParams),
				TileText = Language.Get(titleKey, tab),
				OkButtonText = Language.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void UpdateFounderInfo()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				return;
			}
			PlayerBag playerBag = (PlayerBag)GameHubBehaviour.Hub.User.PlayerSF.Bag;
			FounderLevel founderPackLevel = playerBag.FounderPackLevel;
			FoundersBoosterGui.UpdateHMM2DDynamicSprite(founderPackLevel, this.FounderNameSprite, FoundersBoosterGui.SpriteType.MainMenuName);
		}

		private void DispatchLobbyUpdatedEvent()
		{
			if (MainMenuGui.OnLobbyUpdate != null)
			{
				MainMenuGui.OnLobbyUpdate(this._isInLobby);
			}
		}

		private void OnBattlepassRequestMainMenuData()
		{
			ObservableExtensions.Subscribe<MainMenuData>(this.Main.GetMainMenuData());
		}

		public void OnGetMainMenuDataSuccess(MainMenuData mainMenuData)
		{
			Debug.Log("OnGetMainMenuDataSuccess. _isInitialized=" + this._isInitialized);
			if (!this._isInitialized)
			{
				this._isInitialized = true;
				this.InitializeMainMenuPresenterTree();
				this.AnimateEnterMainMenu();
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				return;
			}
			this._battlepassComponent.RefreshData(mainMenuData);
		}

		public void OnCustomizationItemBought()
		{
			this._customizationInventoryComponent.OnCustomizationItemBought();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MainMenuGui));

		[Header("External Referencies")]
		public MatchAcceptGui MatchAccept;

		public AudioEventAsset MatchFoundSfx;

		[Header("Internal Referencies")]
		public GameModesGUI GameModesGui;

		public MatchStatsGui MatchStats;

		public ShopGUI Shop;

		[Header("[Battlepass]")]
		[SerializeField]
		private BattlepassComponent _battlepassComponent;

		[SerializeField]
		private BattlepassDetailComponent _battlepassDetailComponent;

		[Header("[Inventory]")]
		[SerializeField]
		private CustomizationInventoryComponent _customizationInventoryComponent;

		[InjectOnClient]
		private DiContainer _diContainer;

		[InjectOnClient]
		private IClientButtonBILogger _buttonBILogger;

		[InjectOnClient]
		private IMainMenuInitialization _mainMenuInitialization;

		[InjectOnClient]
		private IGetUGCRestrictionIsEnabled _getUgcRestrictionIsEnabled;

		[InjectOnClient]
		private IUGCRestrictionDialogPresenter _ugcRestrictionDialogPresenter;

		[Inject]
		private ILocalPlayerStorage _localPlayer;

		[Inject]
		private IAutoMatch _autoMatch;

		[Inject]
		private IOpenUrl _openUrl;

		[Inject]
		private IOpenUrlUgcRestricted _openUrlUgcRestricted;

		private IDisposable _disposableCustomizationChange;

		private bool _lockButtons;

		private bool _isInitialized;

		private bool _isInLobby;

		private IMainMenuPresenterTree _mainMenuPresenterTree;

		private MainMenu _main;

		[Header("[Founder Info]")]
		public HMMUI2DDynamicSprite FounderNameSprite;

		private IDisposable _tryToOpenMetalpassWindowDisposable;
	}
}
