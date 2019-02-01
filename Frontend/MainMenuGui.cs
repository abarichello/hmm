using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Objects;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.Audio.Music;
using HeavyMetalMachines.Battlepass;
using HeavyMetalMachines.Customization;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Tutorial;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

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
		public static event Action OnMenuDisplayed;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action<bool> OnLobbyUpdate;

		protected void OnEnable()
		{
			GameHubBehaviour.Hub.GuiScripts.Loading.OnHidingAnimationCompleted += this.AnimateEnterMainMenu;
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientConnectedEvent += this.OnSearchingForAMatch;
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientDisconnectedEvent += this.MatchMakingCanceled;
			ManagerController.Get<GroupManager>().OnGroupUpdate += this.SocialControllerOnGroupUpdate;
			GameHubBehaviour.Hub.GuiScripts.TopMenu.CashButtonClicked += this.OnCashTopButtonClick;
			this._customizationInventoryComponent.OnItemEquiped += this.UpdateEquipedItems;
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				GameHubBehaviour.Hub.ClientApi.friend.OnFriendBagUpdated += this.OnPlayerBagUpdated;
			}
			GameHubBehaviour.Hub.CursorManager.ShowAndSetCursor(true, CursorManager.CursorTypes.MainMenuCursor);
			this.CheckHardCurrencyBought();
			if (GameHubBehaviour.Hub.State.Last is Game)
			{
				SingletonMonoBehaviour<RegionController>.Instance.TryRequestRegionsPing(false);
			}
			this.UpdatePlayButton();
			this._lockButtons = false;
			BattlepassComponent battlepassComponent = this._battlepassComponent;
			battlepassComponent.OnBattlepassTransactionSuccess = (Action)Delegate.Combine(battlepassComponent.OnBattlepassTransactionSuccess, new Action(this.OnBattlepassRequestMainMenuData));
		}

		protected void Awake()
		{
			this._battlepassComponent.LoadMetalpassWindow();
		}

		protected void Start()
		{
			base.StartCoroutine(this.WaitBagToUpdateNameAndIcon());
			GameHubBehaviour.Hub.User.IsReconnecting = false;
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.Lobby;
		}

		private void UpdateEquipedItems(PlayerCustomizationSlot slot)
		{
			if (slot == PlayerCustomizationSlot.Portrait)
			{
				GameHubBehaviour.Hub.GuiScripts.TopMenu.UpdatePortrait();
			}
		}

		private IEnumerator WaitBagToUpdateNameAndIcon()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				GameHubBehaviour.Hub.GuiScripts.TopMenu.SetPlayerName(GameHubBehaviour.Hub.User.Name + "_skipSF");
				yield break;
			}
			while (GameHubBehaviour.Hub.User.PlayerSF.Name == "!NO_NAME_SET!")
			{
				yield return null;
			}
			GameHubBehaviour.Hub.GuiScripts.TopMenu.SetPlayerName(GameHubBehaviour.Hub.User.PlayerSF.Name);
			GameHubBehaviour.Hub.GuiScripts.TopMenu.UpdatePlayerIcon();
			yield break;
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

		private void SocialControllerOnGroupUpdate()
		{
			this.UpdatePlayButton();
		}

		public void OnAllItemsReload()
		{
		}

		public void JoinQueue(GameModeTabs queue)
		{
			this.UpdatePlayButton();
			this.SetLobbyGuiWaiting(Language.Get("FINDINGMATCH", TranslationSheets.MainMenuGui));
			this.Main.SearchForAMatch(queue.ToString(), new Action(this.OnConnectFail));
		}

		protected void OnDestroy()
		{
			this.Shop.CleanUp();
			this._battlepassComponent.UnloadMetalpassWindow();
			this._battlepassDetailComponent.HideDetailWindow(false);
		}

		protected void OnDisable()
		{
			try
			{
				GameHubBehaviour.Hub.GuiScripts.TopMenu.CashButtonClicked -= this.OnCashTopButtonClick;
				if (GameHubBehaviour.Hub.Swordfish.Msg.Ready)
				{
					GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientConnectedEvent -= this.OnSearchingForAMatch;
					GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientDisconnectedEvent -= this.MatchMakingCanceled;
				}
				GroupManager groupManager = ManagerController.Get<GroupManager>();
				if (groupManager != null)
				{
					groupManager.OnGroupUpdate -= this.SocialControllerOnGroupUpdate;
				}
				this._customizationInventoryComponent.OnItemEquiped -= this.UpdateEquipedItems;
				GameHubBehaviour.Hub.GuiScripts.Loading.OnHidingAnimationCompleted -= this.AnimateEnterMainMenu;
				if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
				{
					GameHubBehaviour.Hub.ClientApi.friend.OnFriendBagUpdated -= this.OnPlayerBagUpdated;
				}
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
			this._lobbyGui.ButtonsAnimatorUpdate(MainMenuLobbyGui.MainMenuLobbyAnimatorAction.AllButtonsIn);
			GameHubBehaviour.Hub.GuiScripts.TopMenu.AnimateEnterMainMenu();
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.AnimateEnterMainMenu();
			this.PlayMainMenuTheme();
			this.UpdateFounderInfo();
			this.OnReturnLobby();
			this.CheckForOpenAutomaticMetalpassWindow();
			if (MainMenuGui.OnMenuDisplayed != null)
			{
				MainMenuGui.OnMenuDisplayed();
			}
			this.UpdatePlayButton();
		}

		public void AnimateLeaveLobby()
		{
			this._lockButtons = true;
			this._lobbyGui.ButtonsAnimatorUpdate(MainMenuLobbyGui.MainMenuLobbyAnimatorAction.AllButtonsOut);
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryCloseNews();
			this.OnExitLobby();
		}

		public void AnimateReturnToLobby(bool forceMatchmakingStateUpdate = false, bool forcePlayButtonUnlock = false)
		{
			if (forceMatchmakingStateUpdate)
			{
				this.MatchStats.UpdateMatchmakingNow();
			}
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.Lobby;
			this._lobbyGui.ButtonsAnimatorUpdate(MainMenuLobbyGui.MainMenuLobbyAnimatorAction.AllButtonsIn);
			this.OnReturnLobby();
		}

		private void OnReturnLobby()
		{
			this._isInLobby = true;
			this._lockButtons = false;
			this.DispatchLobbyUpdatedEvent();
			this.UpdatePlayButton();
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryOpenNewsOnLobbyReturn();
		}

		private void OnExitLobby()
		{
			this._isInLobby = false;
			this.DispatchLobbyUpdatedEvent();
		}

		public void AnimateReturnToLobbySearchingMatch()
		{
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.Lobby;
			this.SetLobbyGuiWaiting(Language.Get("FINDINGMATCH", TranslationSheets.MainMenuGui));
			this._lobbyGui.ButtonsAnimatorUpdate(MainMenuLobbyGui.MainMenuLobbyAnimatorAction.AllButtonsIn);
			this.UpdatePlayButton();
			this.OnReturnLobby();
		}

		private void SetLobbyGuiWaiting(string labelText)
		{
			this._lobbyGui.SetWaitingButtonLabelText(labelText);
			this._lobbyGui.EnableTimerSprite(true);
			this._lobbyGui.EnableWaitingButtonLabel(true);
			this._lobbyGui.EnablePlayButtonLabel(false);
		}

		public void BuyHardCurrency()
		{
			OpenUrlUtils.OpenUrl(string.Format("{0}&email={1}", Language.Get("STORE_BUYHARDCOINS", TranslationSheets.Links), GameHubBehaviour.Hub.User.UserSF.Email));
		}

		public void QuitApplication()
		{
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenCloseGameConfirmWindow(delegate
			{
				try
				{
					if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
					{
						GameHubBehaviour.Hub.Swordfish.Msg.Cleanup();
					}
				}
				catch (Exception ex)
				{
				}
			});
		}

		public void SearchForAMatch()
		{
			GameModeTabs gameMode = this.GameModesGui.GameMode;
			this.Main.SearchForAMatch(gameMode.ToString(), new Action(this.OnConnectFail));
			this.UpdatePlayButton();
		}

		private void OnConnectFail()
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("MatchmakingFailedToConnect", "MainMenuGui"),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					this.AnimateReturnToLobby(false, false);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void OnSearchingForAMatch()
		{
			this.MatchStats.AnimateShowMatchSearchingPanel();
			this.SetLobbyGuiWaiting(Language.Get("FINDINGMATCH", TranslationSheets.MainMenuGui));
			GameModeTabs gameMode = GameModeTabs.None;
			string queueName = this.GetQueueName();
			if (queueName != null)
			{
				if (queueName == "Normal")
				{
					gameMode = GameModeTabs.Normal;
					goto IL_A0;
				}
				if (queueName == "CoopVsBots")
				{
					gameMode = GameModeTabs.CoopVsBots;
					goto IL_A0;
				}
				if (queueName == "CustomMatch")
				{
					MainMenuGui.Log.Warn("Calling custom match inside OnSearchingForAMatch");
					goto IL_A0;
				}
			}
			MainMenuGui.Log.WarnFormat("Unknown queue name: {0}", new object[]
			{
				queueName
			});
			IL_A0:
			this.MatchStats.UpdateGameModePingLabel(gameMode);
			RegionServerPing region;
			if (SingletonMonoBehaviour<RegionController>.Instance.RegionDictionary.TryGetValue(GameHubBehaviour.Hub.ClientApi.GetCurrentRegionName(), out region))
			{
				this.MatchStats.UpdateGameModePingSprite(region);
			}
			else
			{
				MainMenuGui.Log.ErrorFormat("Couldn't find regionServerPing. searched for {0}", new object[]
				{
					GameHubBehaviour.Hub.ClientApi.GetCurrentRegionName()
				});
				this.MatchStats.UpdateGameModePingSprite(SingletonMonoBehaviour<RegionController>.Instance.GetBestServerSaved());
			}
			this.UpdatePlayButton();
			if (this.GameModesGui.rootGameObject.activeSelf || (this.GameModesGui.CurrentTab != GameModeTabs.None && this.GameModesGui.CurrentTab != GameModeTabs.Selection && this.GameModesGui.CurrentTab != GameModeTabs.CustomMatch))
			{
				this.GameModesGui.ReturnToMainMenu();
			}
		}

		public void OnMatchFound()
		{
			HudWindowManager.Instance.CloseAll(null);
		}

		public bool IsInNormalQueue()
		{
			return this.GetQueueName().Equals(GameModeTabs.Normal.ToString());
		}

		public string GetQueueName()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				return string.Empty;
			}
			return GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.MatchMadeQueue;
		}

		public void OnPlayButtonClick()
		{
			if (this.ActiveScreen != MainMenuGui.ActiveScreenKind.Lobby && this._lockButtons)
			{
				return;
			}
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.GameMode;
			this.AnimateLeaveLobby();
			this.GameModesGui.AnimateShowSelection(false);
		}

		private void UpdatePlayButton()
		{
			bool flag = true;
			Guid currentGroupID = ManagerController.Get<GroupManager>().CurrentGroupID;
			if (currentGroupID != Guid.Empty && ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner)
			{
				GroupMember[] members = GameHubBehaviour.Hub.ClientApi.group.GetMembers(currentGroupID);
				Dictionary<string, FriendHolder> friendsDictionary = ManagerController.Get<FriendManager>().FriendsDictionary;
				for (int i = 0; i < members.Length; i++)
				{
					string universalID = members[i].UniversalID;
					if (friendsDictionary.ContainsKey(universalID) && friendsDictionary[universalID].FriendBag.IsInMatchOrQueue())
					{
						flag = false;
						break;
					}
				}
			}
			this._lobbyGui.EnableGroupNotReadyTooltip(!flag);
			SwordfishMatchmaking matchmaking = GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking;
			if (matchmaking == null)
			{
				MainMenuGui.Log.Warn("UpdatePlayButton: Swordfish.Msg.Matchmaking null. This is unexpected unless client is closing");
				return;
			}
			bool flag2 = matchmaking.Connected || matchmaking.WaitingForMatchResult || matchmaking.Undefined || GameHubBehaviour.Hub.ClientApi.lobby.IsInLobby() || !flag;
			this._lobbyGui.EnablePlayButton(!flag2);
			if (flag2)
			{
				if (matchmaking.Connected || matchmaking.WaitingForMatchResult || matchmaking.Undefined)
				{
					this._lobbyGui.SetWaitingButtonLabelText(Language.Get("WAITINGMATCH", TranslationSheets.MainMenuGui));
				}
				else
				{
					this._lobbyGui.SetWaitingButtonLabelText(Language.Get("WAITINGGROUPMEMBER", TranslationSheets.MainMenuGui));
				}
			}
			else
			{
				this._lobbyGui.SetPlayButtonLabelText(Language.Get("PLAY_BUTTON", TranslationSheets.MainMenuGui));
			}
			this._lobbyGui.EnablePlayButtonLabel(!flag2);
			this._lobbyGui.EnableWaitingButtonLabel(flag2);
			this._lobbyGui.EnableTimerSprite(flag2);
			this._lobbyGui.EnableRegionTooltip(flag2);
			string reason = string.Empty;
			if (ManagerController.Get<GroupManager>().GetSelfGroupStatus() != GroupStatus.None)
			{
				reason = "TUTORIAL_TOOLTIP_BLOCKED_BY_PARTY";
				this.SetTutorialButtonState(false, reason);
				return;
			}
			if (flag2)
			{
				if (this.MatchStats.MMInterfaceState != MatchStatsGui.MatchmakingInterfaceState.Play)
				{
					MainMenuGui.Log.WarnFormat("UpdateTutorialBtn called in unexpected MMInterfaceState. MMInterfaceState={0}", new object[]
					{
						this.MatchStats.MMInterfaceState
					});
				}
				reason = "TUTORIAL_TOOLTIP_BLOCKED_BY_QUEUE";
			}
			this.SetTutorialButtonState(!flag2, reason);
		}

		private void SetTutorialButtonState(bool isEnabled, string reason = null)
		{
			UIButtonColor.State state = (!isEnabled) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal;
			foreach (UIButton uibutton in this.TutorialButtonGameObject.GetComponents<UIButton>())
			{
				uibutton.GetComponent<Collider>().enabled = isEnabled;
				uibutton.SetState(state, true);
			}
			if (isEnabled)
			{
				this.TutorialTooltip.gameObject.SetActive(false);
			}
			else
			{
				this.TutorialTooltip.TooltipText = Language.Get(reason, TranslationSheets.MainMenuGui);
				this.TutorialTooltip.gameObject.SetActive(true);
			}
		}

		private void PlayMainMenuTheme()
		{
			MusicManager.PlayMusic(MusicManager.State.MainMenu);
		}

		public void CancelMatchMaking()
		{
			this.Main.CancelMatchMaking();
			this.UpdatePlayButton();
		}

		public void MatchMakingCanceled()
		{
			this.UpdatePlayButton();
			if (this.MatchStats.MMInterfaceState != MatchStatsGui.MatchmakingInterfaceState.None && this.MatchStats.MMInterfaceState != MatchStatsGui.MatchmakingInterfaceState.WaitingServer)
			{
				this.MatchStats.AnimateHideMatchSearchingPanel();
			}
		}

		public void RejectMatch()
		{
			this.Main.SendRejectMatch();
			this.MatchStats.MMInterfaceState = MatchStatsGui.MatchmakingInterfaceState.MatchMadeWaiting;
			this.UpdatePlayButton();
		}

		public void ClearCurrentServer()
		{
			this.Main.ClearCurrentServer();
		}

		public void SendMatchAccepted()
		{
			this.Main.SendMatchAccepted();
		}

		public void OnShopButtonClick()
		{
			if (this.ActiveScreen != MainMenuGui.ActiveScreenKind.Lobby && this._lockButtons)
			{
				return;
			}
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.Shop;
			this.AnimateLeaveLobby();
			this.Shop.AnimateShow();
		}

		public void OpenShop(ShopGUI.Tab tab, Guid skinId)
		{
			if (GameHubBehaviour.Hub.ClientApi.lobby.IsInLobby())
			{
				return;
			}
			if (this.ActiveScreen == MainMenuGui.ActiveScreenKind.Shop)
			{
				if (this.Shop.Details.IsVisible())
				{
					this.Shop.Details.Hide();
				}
				this.Shop.DisableTabs();
				this.Shop.AnimateShow(tab, skinId);
				return;
			}
			this.ReturnToMainMenuLobby(this.ActiveScreen);
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.Shop;
			this.AnimateLeaveLobby();
			this._battlepassComponent.HideMetalpassWindow(true);
			this.Shop.AnimateShow(tab, skinId);
		}

		public void OnCashTopButtonClick()
		{
			if (GameHubBehaviour.Hub.ClientApi.lobby.IsInLobby())
			{
				return;
			}
			MainMenuGui.ActiveScreenKind activeScreen = this.ActiveScreen;
			if (activeScreen == MainMenuGui.ActiveScreenKind.Shop)
			{
				if (this.Shop.Details.IsVisible())
				{
					this.Shop.Details.Hide();
				}
				this.Shop.SwitchToHoplonsScreen();
				return;
			}
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.Shop;
			this.ReturnToMainMenuLobby(activeScreen);
			this.Shop.AnimateShowAndOpenCash();
		}

		public void ReturnToMainMenuLobby()
		{
			this.ReturnToMainMenuLobby(this.ActiveScreen);
		}

		private void ReturnToMainMenuLobby(MainMenuGui.ActiveScreenKind currentActiveScreenKind)
		{
			if (this.GameModesGui.gameModesGameObject.activeInHierarchy)
			{
				this.GameModesGui.AnimateReturnToMainMenu();
			}
			else if (this.GameModesGui.gameSelectedGameObject.activeInHierarchy)
			{
				this.GameModesGui.AnimateStartMatchScreenGoDirectToMainMenu();
			}
			else
			{
				switch (currentActiveScreenKind)
				{
				case MainMenuGui.ActiveScreenKind.Shop:
					this.Shop.ReturnToMainMenu();
					return;
				case MainMenuGui.ActiveScreenKind.Profile:
					this.ProfileController.ReturnToMainMenu();
					break;
				case MainMenuGui.ActiveScreenKind.Metalpass:
					this._battlepassComponent.HideMetalpassWindow(true);
					break;
				case MainMenuGui.ActiveScreenKind.Inventory:
					this._customizationInventoryComponent.HideCustomizationInventoryWindow(true);
					break;
				}
				this.AnimateLeaveLobby();
			}
		}

		public void OnProfileButtonClick()
		{
			if (this._lockButtons)
			{
				return;
			}
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.Profile;
			this.AnimateLeaveLobby();
			this.ProfileController.AnimateShow();
		}

		public void OnMetalpassButtonClick()
		{
			if (this._lockButtons)
			{
				return;
			}
			this.OpenMetallpassWindow(false);
		}

		public void TryToOpenMetallpassPremiumShopWindow()
		{
			if (!this._battlepassComponent.IsMetalpassWindowVisible())
			{
				this.ReturnToMainMenuLobby();
				this.OpenMetallpassWindow(true);
			}
			else
			{
				this._battlepassComponent.ShowMetalpassPremiumShopWindow();
			}
		}

		private void OpenMetallpassWindow(bool showPremiumShop = false)
		{
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.Metalpass;
			this.AnimateLeaveLobby();
			if (!showPremiumShop)
			{
				this._battlepassComponent.ShowMetalpassWindow(null);
			}
			else
			{
				this._battlepassComponent.ShowMetalpassPremiumShopWindow();
			}
		}

		private void CheckForOpenAutomaticMetalpassWindow()
		{
			if (this._battlepassComponent.MustOpenMetalpassWindow())
			{
				this.OpenMetallpassWindow(false);
			}
		}

		public void OnInventoryButtonClick()
		{
			if (this._lockButtons)
			{
				return;
			}
			this.ActiveScreen = MainMenuGui.ActiveScreenKind.Inventory;
			this.AnimateLeaveLobby();
			this._customizationInventoryComponent.LoadInventoryScene(new Action(this.OnInventoryWindowClosed));
		}

		private void OnInventoryWindowClosed()
		{
			this.AnimateReturnToLobby(false, false);
		}

		public void OnHelpButtonClick()
		{
			OpenUrlUtils.OpenSteamUrl(GameHubBehaviour.Hub, ConfigAccess.SFHelpUrl, string.Format("?lang={0}", Language.CurrentLanguage()), OpenUrlUtils.HardcodedWidth, (int)((float)Screen.height / 100f * 90f), "Heavy Metal Machines");
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.HelpSiteOpenFromLobby, true);
			}
		}

		public void OnLeaderboardButtonClick()
		{
			OpenUrlUtils.OpenSteamUrl(GameHubBehaviour.Hub, ConfigAccess.SFLeaderboardUrl, string.Format("?lang={0}&id={1}&region={2}", Language.CurrentLanguage(), GameHubBehaviour.Hub.User.UniversalId, GameHubBehaviour.Hub.ClientApi.GetCurrentRegionName()), OpenUrlUtils.HardcodedWidth, (int)((float)Screen.height / 100f * 90f), "Heavy Metal Machines");
			if (!GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				GameHubBehaviour.Hub.Swordfish.Log.BILogClient(ClientBITags.LeaderboardSiteOpenFromTop, true);
			}
		}

		public void PartyErrorFeedback(PartyFeedbackEnum feedback, params object[] param)
		{
			switch (feedback)
			{
			case PartyFeedbackEnum.PartyInviteError:
				MainMenuGui.OkWindowFeedback("PartyInviteError", "MainMenuGui", new object[0]);
				break;
			case PartyFeedbackEnum.PartyInviteReceivedError:
				MainMenuGui.OkWindowFeedback("PartyInviteReceivedError", "MainMenuGui", new object[0]);
				break;
			case PartyFeedbackEnum.PartyInviteCouldNotGetUser:
				MainMenuGui.OkWindowFeedback("PartyInviteCouldNotGetUser", "MainMenuGui", new object[0]);
				break;
			case PartyFeedbackEnum.WebserviceCouldNotGetUser:
				MainMenuGui.OkWindowFeedback("WebserviceCouldNotGetUser", "MainMenuGui", new object[0]);
				break;
			case PartyFeedbackEnum.PartyFull:
				MainMenuGui.OkWindowFeedback("PartyIsFull", "MainMenuGui", new object[0]);
				break;
			case PartyFeedbackEnum.PartyAlreadyClosed:
				MainMenuGui.OkWindowFeedback("PartyAlreadyClosed", "MainMenuGui", new object[0]);
				break;
			case PartyFeedbackEnum.UserAlreadyPlaying:
				MainMenuGui.OkWindowFeedback("UserAlreadyPlaying", "MainMenuGui", new object[0]);
				break;
			}
		}

		public void NotPartyOwnerFeedback()
		{
			MainMenuGui.OkWindowFeedback("YouAreNotPartyOwner", "MainMenuGui", new object[0]);
		}

		public void PlayerAlreadyInvitedToThePartyFeedback(string playerName)
		{
			MainMenuGui.OkWindowFeedback("PlayerAlreadyInvitedToTheParty", "MainMenuGui", playerName, new object[0]);
		}

		public void PlayerAlreadyJoinedPartyFeedback(string playerName)
		{
			MainMenuGui.OkWindowFeedback("PlayerAlreadyJoinedParty", "MainMenuGui", playerName, new object[0]);
		}

		public static void OkWindowFeedback(string key, string tab, params object[] param)
		{
			MainMenuGui.OkWindowFeedback(key, string.Empty, tab, param);
		}

		public static void OkWindowFeedback(string questionKey, string titleKey, string tab, params object[] questionParams)
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = string.Format(Language.Get(questionKey, tab), questionParams),
				TileText = Language.Get(titleKey, tab),
				OkButtonText = Language.Get("Ok", "GUI"),
				OnOk = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void StartLoadingAndGoToTutorial()
		{
			if (this._lockButtons)
			{
				return;
			}
			Guid confirmWindowGuid = Guid.NewGuid();
			string key = TutorialController.HasPlayerDoneFirstTutorial() ? "AskPlayAgainTuto" : "AskFirstPlayOnStartGame";
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get(key, TranslationSheets.Tutorial),
				ConfirmButtonText = Language.Get("Yes", "GUI"),
				OnConfirm = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
					GameHubBehaviour.Hub.TutorialHub.TutorialControllerInstance.StartLoadingAndGoToTutorial();
				},
				RefuseButtonText = Language.Get("No", "GUI"),
				OnRefuse = delegate()
				{
					GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void OnPlayerBagUpdated(UserFriend userFriend)
		{
			this.UpdatePlayButton();
		}

		public void UpdateMainMenuButtons()
		{
			this.UpdatePlayButton();
		}

		private void UpdateFounderInfo()
		{
			PlayerBag playerBag = (PlayerBag)GameHubBehaviour.Hub.User.PlayerSF.Bag;
			FounderLevel founderPackLevel = (FounderLevel)playerBag.FounderPackLevel;
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
			this.Main.GetMainMenuData();
		}

		public void OnGetMainMenuDataSuccess(MainMenuData mainMenuData)
		{
			this._battlepassComponent.RefreshData(mainMenuData);
			if (!this._battlepassDetailComponent.TryToShowDetailWindow(new Action<bool>(this.OnBattlepassDetailWindowClose)))
			{
				BiUtils.MarkConversion(GameHubBehaviour.Hub);
			}
		}

		private void OnBattlepassDetailWindowClose(bool showMetalpassWindow)
		{
			BiUtils.MarkConversion(GameHubBehaviour.Hub);
			if (showMetalpassWindow)
			{
				this.OpenMetallpassWindow(false);
			}
		}

		public void OnCustomizationItemBought()
		{
			this._customizationInventoryComponent.OnCustomizationItemBought();
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MainMenuGui));

		[Header("External Referencies")]
		public MatchAcceptGui MatchAccept;

		[Header("Internal Referencies")]
		public GameModesGUI GameModesGui;

		public MatchStatsGui MatchStats;

		public ShopGUI Shop;

		public MainMenuProfileController ProfileController;

		public UIButton ShopButton;

		public GameObject TutorialButtonGameObject;

		public HMMTooltipTrigger TutorialTooltip;

		public TutorialModalWindowController TutorialModalWindowController;

		public ConnectionFeedbackGUI ConnectionFeedback;

		[Header("[Lobby]")]
		[SerializeField]
		private MainMenuLobbyGui _lobbyGui;

		[Header("[Battlepass]")]
		[SerializeField]
		private BattlepassComponent _battlepassComponent;

		[SerializeField]
		private BattlepassDetailComponent _battlepassDetailComponent;

		[Header("[Inventory]")]
		[SerializeField]
		private CustomizationInventoryComponent _customizationInventoryComponent;

		private bool _lockButtons;

		private bool _isInLobby;

		private MainMenu _main;

		[NonSerialized]
		public MainMenuGui.ActiveScreenKind ActiveScreen;

		[Header("[Founder Info]")]
		public HMMUI2DDynamicSprite FounderNameSprite;

		public enum ActiveScreenKind
		{
			Lobby,
			Shop,
			Profile,
			GameMode,
			SpectatorMatchSelection,
			Metalpass,
			Inventory
		}
	}
}
