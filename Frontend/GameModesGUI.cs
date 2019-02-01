using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.Objects;
using HeavyMetalMachines.Frontend.Region;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GameModesGUI : GameHubBehaviour
	{
		private bool IsPvpUnlokedByLevel
		{
			get
			{
				return this._isPvpUnlokedByLevel;
			}
			set
			{
				this.NormalButton.GetComponent<Collider>().enabled = (value && this.IsPvpQueueOpened);
				this._isPvpUnlokedByLevel = value;
			}
		}

		private bool IsPvpQueueOpened
		{
			get
			{
				return this._isPvpQueueOpened;
			}
			set
			{
				this.NormalButton.GetComponent<Collider>().enabled = (value && this.IsPvpUnlokedByLevel);
				this._isPvpQueueOpened = value;
			}
		}

		private static MainMenuGui MainMenuGui
		{
			get
			{
				return GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
			}
		}

		private void Awake()
		{
			this._rootPanel = this.rootGameObject.GetComponent<UIPanel>();
			this._rootPanel.alpha = 0f;
			this.BotsButton.GetComponent<Collider>().enabled = true;
			this.rootGameObject.SetActive(false);
			MainMenu.PlayerReloadedEvent += this.MainMenuOnPlayerReloadedEvent;
			ManagerController.Get<GroupManager>().OnGroupUpdate += this.SocialControllerOnGroupUpdate;
			SingletonMonoBehaviour<RegionController>.Instance.OnRegionServerChanged += this.OnRegionServerChanged;
			this.SetupMatchTooltipsInfo();
		}

		private void Start()
		{
			this.CreateInfos();
			this.OptionsScreen = UnityEngine.Object.FindObjectOfType<EscMenuGui>();
			this.TrySubscribeLobbyEvents(true);
		}

		protected void OnDestroy()
		{
			this.CleanUp();
			MainMenu.PlayerReloadedEvent -= this.MainMenuOnPlayerReloadedEvent;
			if (ManagerController.Get<GroupManager>() != null)
			{
				ManagerController.Get<GroupManager>().OnGroupUpdate -= this.SocialControllerOnGroupUpdate;
			}
			this.TrySubscribeLobbyEvents(false);
			if (SingletonMonoBehaviour<RegionController>.DoesInstanceExist())
			{
				SingletonMonoBehaviour<RegionController>.Instance.OnRegionServerChanged -= this.OnRegionServerChanged;
			}
			if (GameHubBehaviour.Hub == null || GameHubBehaviour.Hub.Swordfish == null || GameHubBehaviour.Hub.Swordfish.Msg == null || GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking == null)
			{
				return;
			}
		}

		private void CleanUp()
		{
			this._normalGameInfo.icoTexture = null;
			this._normalGameInfo.backgroundTexture = null;
			this._botGameInfo.icoTexture = null;
			this._botGameInfo.backgroundTexture = null;
			this._customMatchGameInfo.icoTexture = null;
			this._customMatchGameInfo.backgroundTexture = null;
			this.gameTypeTexture.mainTexture = null;
			this.gameTypeTexture = null;
			this.gameTypeBackgroundTexture.mainTexture = null;
			this.gameTypeBackgroundTexture = null;
			this.normalTexture = null;
			this.normalBackgroundTexture = null;
			this.botsTexture = null;
			this.botsBackgroundTexture = null;
			this.customMatchTexture = null;
			this.CustomMatchBackgroundTexture = null;
		}

		private void OnRegionServerChanged(RegionServerPing regionServerPing)
		{
			this.SetupPvpTimerInfo();
		}

		private void SetGameModesCardsColliders(bool isEnabled, bool customHoverOut = false)
		{
			this.NormalButton.GetComponent<Collider>().enabled = (isEnabled && this.IsPvpQueueOpened && this.IsPvpUnlokedByLevel);
			this.BotsButton.GetComponent<Collider>().enabled = isEnabled;
			this.CustomMatchButton.GetComponent<Collider>().enabled = isEnabled;
			if (customHoverOut)
			{
				this._gameModesHoverView.OnCustomMatchButtonHoverOut();
			}
			this.NormalButton.SetState(UIButtonColor.State.Normal, true);
			this.BotsButton.SetState(UIButtonColor.State.Normal, true);
			this.CustomMatchButton.SetState(UIButtonColor.State.Normal, true);
		}

		private void SetCustommatchButtonsColliders(bool isEnabled)
		{
			this.CustomMatchCreateButton.GetComponent<Collider>().enabled = isEnabled;
			this.CustomMatchEnterButton.GetComponent<Collider>().enabled = isEnabled;
			this.CustomMatchCreateButton.SetState(UIButtonColor.State.Normal, true);
			this.CustomMatchEnterButton.SetState(UIButtonColor.State.Normal, true);
		}

		public void GameModesOnBackButton()
		{
			this.ReturnToMainMenu();
		}

		public void ReturnToMainMenu()
		{
			this.AnimateReturnToMainMenu();
			GameModesGUI.MainMenuGui.AnimateReturnToLobby(false, false);
		}

		public void AnimateReturnToMainMenu()
		{
			this.SetGameModesCardsColliders(false, false);
			this.SetCustommatchButtonsColliders(false);
			if (this.RegionGroupAnimation != null)
			{
				GUIUtils.PlayAnimation(this.RegionGroupAnimation, true, 1f, string.Empty);
			}
			this.CurrentTab = GameModeTabs.None;
			base.StartCoroutine(this.AnimateReturnToMainMenuCoroutine());
		}

		private IEnumerator AnimateReturnToMainMenuCoroutine()
		{
			GUIUtils.PlayAnimation(this._mainAnimation, true, 1f, string.Empty);
			this._modesAnimation.Play("gameModeOut");
			while (this._mainAnimation.isPlaying || this._modesAnimation.isPlaying)
			{
				yield return null;
			}
			this.rootGameObject.SetActive(false);
			this._rootPanel.alpha = 0f;
			yield break;
		}

		public void AnimateShowSelection(bool returningTo)
		{
			if (!this.gameModesGameObject.activeSelf)
			{
				this.gameModesGameObject.SetActive(true);
				base.StartCoroutine(this.CheckIfPlayerHaveEntryMatchModesScreen());
			}
			if (this.RegionGroupAnimation != null && !returningTo)
			{
				GUIUtils.PlayAnimation(this.RegionGroupAnimation, false, 1f, string.Empty);
			}
			this._rootPanel.alpha = 1f;
			this._modesAnimation.Play("gameModeIn");
			if (!returningTo)
			{
				if (this.gameSelectedGameObject.activeSelf)
				{
					this.gameSelectedGameObject.SetActive(false);
				}
				base.StopAllCoroutines();
				GUIUtils.PlayAnimation(this._mainAnimation, false, 1f, string.Empty);
				this.UpdatePvpUnlockOnEnterGameMode();
				this.SetupStartMatchScreen(GameModeTabs.Selection);
			}
			this.SetupPvpTimerInfo();
			this.CurrentTab = GameModeTabs.Selection;
			this.SetGameModesCardsColliders(true, false);
			this._acessCodeUIInput.value = null;
		}

		private void UpdatePvpUnlockOnEnterGameMode()
		{
			bool unlockSeen = GameHubBehaviour.Hub.User.Bag.GetUnlockSeen(ProgressionInfo.UnlockFlag.PVP);
			bool isLocked = !unlockSeen;
			this.UpdatePvpUnlock(isLocked);
			this.PvpLockLabelGroupGameObject.SetActive(false);
			if (!unlockSeen)
			{
				this.ShowPvpUnlockAnimation();
			}
		}

		private void UpdatePvpUnlock(bool isLocked)
		{
			this.NormalButton.SetState((!isLocked) ? UIButtonColor.State.Normal : UIButtonColor.State.Disabled, true);
			this.NormalButton.GetComponent<Collider>().enabled = !isLocked;
			this.NormalButtonHoverSprite.enabled = !isLocked;
			this.NormalButtonHoverSprite.alpha = 0f;
			this.IsPvpUnlokedByLevel = !isLocked;
		}

		private void ShowPvpUnlockAnimation()
		{
			this.NormalButton.SetState(UIButtonColor.State.Disabled, true);
			this.NormalButton.GetComponent<Collider>().enabled = false;
			this.NormalButtonHoverSprite.enabled = false;
			GameHubBehaviour.Hub.User.Bag.SetUnlockSeen(ProgressionInfo.UnlockFlag.PVP);
			PlayerCustomWS.UpdatePlayerUnlockMask(GameHubBehaviour.Hub.User.Bag, new SwordfishClientApi.ParameterizedCallback<string>(this.OnUpdatePlayerUnlockMaskSuccess), new SwordfishClientApi.ErrorCallback(this.OnUpdatePlayerUnlockMaskError));
			this.OnUnlockPvpAnimationFinished();
		}

		public void OnUnlockPvpAnimationFinished()
		{
			this.IsPvpUnlokedByLevel = true;
			this.NormalButtonHoverSprite.enabled = true;
			this.NormalButtonHoverSprite.alpha = 0f;
		}

		private void OnUpdatePlayerUnlockMaskError(object state, Exception exception)
		{
			GameModesGUI.Log.ErrorFormat("Error on OnUpdatePlayerUnlockMaskError. PlayerId: {0}, Error: {1}", new object[]
			{
				GameHubBehaviour.Hub.User.PlayerSF.Id,
				exception.Message
			});
		}

		private void OnUpdatePlayerUnlockMaskSuccess(object state, string obj)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<T>)obj);
			if (!netResult.Success)
			{
				GameModesGUI.Log.ErrorFormat("Error on OnUpdatePlayerUnlockMaskSuccess. PlayerId: {0}, Error: {1}", new object[]
				{
					GameHubBehaviour.Hub.User.PlayerSF.Id,
					netResult.Msg
				});
			}
		}

		public void AnimateGameModeSelected(GameModeTabs mode)
		{
			if (mode != GameModeTabs.CoopVsBots)
			{
				if (mode == GameModeTabs.CustomMatch)
				{
					this._customMatchAnimation.Play("customGroupIn");
					this._modesAnimation.Play("gameModeOut");
				}
			}
		}

		public void AnimateShowStartMatchScreen()
		{
			if (!this.gameSelectedGameObject.gameObject.activeSelf)
			{
				this.gameSelectedGameObject.SetActive(true);
			}
			if (GameHubBehaviour.Hub.User.Bag.LeaverStatus > 0)
			{
				GameModesGUI.Log.Info("Player has penalty state");
				this.AFKPivot.SetActive(true);
			}
			else
			{
				this.AFKPivot.SetActive(false);
			}
			this.SetupPlayButton();
		}

		public void AnimateShowStartCustomMatchScreen()
		{
			if (!this.gameSelectedGameObject.gameObject.activeSelf)
			{
				this.gameSelectedGameObject.SetActive(true);
			}
		}

		private void SetupPlayButton()
		{
			bool flag = ManagerController.Get<GroupManager>().CurrentGroupID == Guid.Empty || ManagerController.Get<GroupManager>().GetSelfGroupStatus() == GroupStatus.Owner;
			this.PlayButton.GetComponent<BoxCollider>().enabled = flag;
			this.PlayButton.GetComponent<UIButton>().SetState((!flag) ? UIButtonColor.State.Disabled : UIButtonColor.State.Normal, true);
		}

		public void AnimateLeavingStartMatchScreen()
		{
			this._customMatchAnimation.Play("customGroupOut");
		}

		public void AnimateStartMatchScreenGoDirectToMainMenu()
		{
			if (this.RegionGroupAnimation != null)
			{
				GUIUtils.PlayAnimation(this.RegionGroupAnimation, true, 1f, string.Empty);
			}
		}

		public void SetupStartMatchScreen(GameModeTabs tab = GameModeTabs.Selection)
		{
			if (this.CurrentTab == tab)
			{
				return;
			}
			switch (tab)
			{
			case GameModeTabs.Selection:
				this.SetupRewardsInfo();
				break;
			case GameModeTabs.Normal:
			case GameModeTabs.CoopVsBots:
			{
				GameModesGUI.GameModeInfo info = (tab != GameModeTabs.Normal) ? this._botGameInfo : this._normalGameInfo;
				this.ConfigPanelInfo(info, tab);
				break;
			}
			case GameModeTabs.CustomMatch:
			{
				GameModesGUI.GameModeInfo customMatchGameInfo = this._customMatchGameInfo;
				this.ConfigPanelInfo(customMatchGameInfo, tab);
				break;
			}
			}
			this.CurrentTab = tab;
			this.rootGameObject.SetActive(true);
		}

		private void ConfigPanelInfo(GameModesGUI.GameModeInfo info, GameModeTabs tab)
		{
			this.gameTypeTexture.mainTexture = info.icoTexture;
			this.gameDescriptionLabel.text = info.description;
			this.playersCountLabel.text = info.playersCount;
			this.gameAvgTimeLabel.text = info.gameAvgTime;
			this.PlayerVSPlayerGroup.SetActive(tab == GameModeTabs.Normal);
			this.PlayerVSAIGroup.SetActive(tab == GameModeTabs.CoopVsBots);
			this.CustomMatchGroup.SetActive(tab == GameModeTabs.CustomMatch);
			this.StartMatchButtonsGroup.SetActive(tab != GameModeTabs.CustomMatch);
		}

		public void BackToInit()
		{
			this._acessCodeUIInput.value = null;
			this.AnimateLeavingStartMatchScreen();
			this.AnimateShowSelection(true);
			this.OnRaiseCreateOrJoinLobbyError(false, null);
		}

		public void OnClickedNormalGame()
		{
			if (this.ValidateSearchForMatch())
			{
				GameHubBehaviour.Hub.Match.Kind = MatchData.MatchKind.PvP;
				this.SearchForAMatchAndReturnToMainMenu(GameModeTabs.Normal, false);
				GameHubBehaviour.Hub.GuiScripts.TopMenu.PreventGroupMemberRemoval();
			}
		}

		private void InnerOnClickedNormalGame()
		{
			this.ConfirmAndSetupAnimSelectGameMode(GameModeTabs.Normal);
		}

		public void OnClickedBotsGame()
		{
			this.ConfirmAndSetupAnimSelectGameMode(GameModeTabs.CoopVsBots);
		}

		public void OnClickedCustomMatch()
		{
			GameHubBehaviour.Hub.Match.Kind = MatchData.MatchKind.Custom;
			this.ConfirmAndSetupAnimSelectGameMode(GameModeTabs.CustomMatch);
			this.SetGameModesCardsColliders(false, true);
			this.SetCustommatchButtonsColliders(true);
		}

		private void ConfirmAndSetupAnimSelectGameMode(GameModeTabs modeTab)
		{
			if (modeTab != GameModeTabs.CoopVsBots)
			{
				if (modeTab != GameModeTabs.CustomMatch)
				{
					GameHubBehaviour.Hub.Match.Kind = MatchData.MatchKind.PvP;
				}
				else
				{
					GameHubBehaviour.Hub.Match.Kind = MatchData.MatchKind.Custom;
				}
			}
			else
			{
				GameHubBehaviour.Hub.Match.Kind = MatchData.MatchKind.PvE;
			}
			this.AnimateGameModeSelected(modeTab);
			this.AnimateShowStartMatchScreen();
			this.SetupStartMatchScreen(modeTab);
		}

		private void Update()
		{
			this.ChangeGameTab();
		}

		private void ChangeGameTab()
		{
			GameModeTabs currentTab = this.CurrentTab;
			if (currentTab != GameModeTabs.None)
			{
				if (currentTab == GameModeTabs.Selection)
				{
					this.PvpTimerInfoUpdate();
				}
				return;
			}
		}

		public void OnClickedPlayBtn()
		{
			if (!this.ValidateSearchForMatch())
			{
				return;
			}
			this.SearchForAMatchAndReturnToMainMenu(this.CurrentTab, true);
		}

		private bool ValidateSearchForMatch()
		{
			if (GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Visible || this.CurrentTab == GameModeTabs.None)
			{
				return false;
			}
			if (ManagerController.Get<GroupManager>().CurrentGroupID != Guid.Empty && ManagerController.Get<GroupManager>().GetSelfGroupStatus() != GroupStatus.Owner)
			{
				CustomMatchController.ShowCustomMatchDialog(Language.Get("YouAreNotPartyOwner", TranslationSheets.MainMenuGui));
				return false;
			}
			for (int i = 0; i < ManagerController.Get<GroupManager>().GroupMembersSortedList.Count; i++)
			{
				GroupMember groupMember = ManagerController.Get<GroupManager>().GroupMembersSortedList[i];
				if (groupMember.GroupId.Equals(Guid.Empty))
				{
					string textContent = string.Format(Language.Get("CANT_QUEUE_PENDING_INVITE", TranslationSheets.MainMenuGui), groupMember.PlayerName);
					BaseHintContent baseHintContent = new BaseHintContent(textContent, 5f, true, null, "SystemMessage");
					SingletonMonoBehaviour<PanelController>.Instance.ShowMessageHint(baseHintContent, StackableHintKind.None, HintColorScheme.System);
					return false;
				}
			}
			if (GameModesGUI.MatchBlocker.IsBlocked() && Time.time < GameModesGUI.MatchBlocker.GetEndBlockingTime())
			{
				CustomMatchController.ShowCustomMatchDialog(Language.Get("MatchBlockerWarning", TranslationSheets.MainMenuGui));
				return false;
			}
			return true;
		}

		private void SearchForAMatchAndReturnToMainMenu(GameModeTabs currentTab, bool fromMatchScreen)
		{
			this.GameMode = currentTab;
			this.CurrentTab = GameModeTabs.None;
			GameModesGUI.MatchBlocker.UnblockPlayer();
			this.MatchStats.OnClickedPlayBtn(this.GameMode);
			if (fromMatchScreen)
			{
				GameModesGUI.MainMenuGui.SearchForAMatch();
				GameModesGUI.MainMenuGui.AnimateReturnToLobbySearchingMatch();
				this.AnimateStartMatchScreenGoDirectToMainMenu();
			}
			else
			{
				this.SetGameModesCardsColliders(false, false);
				GameModesGUI.MainMenuGui.SearchForAMatch();
				GameModesGUI.MainMenuGui.AnimateReturnToLobbySearchingMatch();
				if (this.RegionGroupAnimation != null)
				{
					GUIUtils.PlayAnimation(this.RegionGroupAnimation, true, 1f, string.Empty);
				}
			}
		}

		public void OnFriendsBtn()
		{
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SocialModalGUI>();
		}

		public void OnConfigControlsBtn()
		{
			this.OptionsScreen.OpenOnControlsScreen();
		}

		private void OnLobbyReady(object sender, MatchmakingLobbyCreatedEventArgs matchmakingLobbyCreatedEventArgs)
		{
			this.OpenLobbyScreen();
		}

		private void OnLobbyJoined(object sender, MatchmakingUpdateLobbyMembersEventArgs matchmakingUpdateLobbyMembersEventArgs)
		{
			this.OpenLobbyScreen();
		}

		private void OpenLobbyScreen()
		{
			this._acessCodeUIInput.value = null;
			this._acessCodeUIInput.isSelected = false;
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.HideWaitinWindow(typeof(CustomMatchController));
			this.gameSelectedGameObject.SetActive(false);
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryCloseAll();
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryToSetNewsButtonEnabled(false);
			Future future2 = SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<CustomMatchLobby_Modal_UI>();
			future2.WhenDone(delegate(IFuture future)
			{
				this.AnimateStartMatchScreenGoDirectToMainMenu();
			});
		}

		private void TrySubscribeLobbyEvents(bool targetState)
		{
			if (this._subscribedToLobbyEvents == targetState)
			{
				return;
			}
			this._subscribedToLobbyEvents = targetState;
			if (targetState)
			{
				GameHubBehaviour.Hub.ClientApi.lobby.LobbyReady += this.OnLobbyReady;
				GameHubBehaviour.Hub.ClientApi.lobby.JoinedLobby += this.OnLobbyJoined;
				CustomMatchController.EvtCreateOrJoinLobbyError += this.OnRaiseCreateOrJoinLobbyError;
				return;
			}
			GameHubBehaviour.Hub.ClientApi.lobby.JoinedLobby -= this.OnLobbyJoined;
			GameHubBehaviour.Hub.ClientApi.lobby.LobbyReady -= this.OnLobbyReady;
			CustomMatchController.EvtCreateOrJoinLobbyError -= this.OnRaiseCreateOrJoinLobbyError;
		}

		public void OnSubmit_SendAcessCodeToVerifyEntranceToCustomMatch()
		{
			this.TryEnterCustomMatch();
		}

		public void OnButtonClick_EnterLobby()
		{
			this.TryEnterCustomMatch();
		}

		private void TryEnterCustomMatch()
		{
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryCloseNews();
			this.CheckCustomMatchAcessCode(this._acessCodeUIInput.value);
		}

		public void CheckCustomMatchAcessCode(string accessCode)
		{
			this._acessCodeUIInput.isSelected = false;
			if (string.IsNullOrEmpty(this._acessCodeUIInput.value))
			{
				this.OnRaiseCreateOrJoinLobbyError(true, Language.Get("GAMEMODE_EMPTYCODE", TranslationSheets.MainMenuGui));
				return;
			}
			SingletonMonoBehaviour<CustomMatchController>.Instance.CreateOrJoinLobby(accessCode, 2);
		}

		public void OnButtonClick_CreatedLobby()
		{
			GameHubBehaviour.Hub.GuiScripts.TopRightButtonsMenu.TryCloseNews();
			SingletonMonoBehaviour<CustomMatchController>.Instance.CreateLobby((!this._allowSpectatorsToggle.value) ? 0 : 2);
		}

		private void OnRaiseCreateOrJoinLobbyError(bool showErrorText, string errorMsg = null)
		{
			this._customMatchFeedbackBorderInfocodeAcessSprite.gameObject.SetActive(showErrorText);
			this._customMatchFeedbackLabel.gameObject.SetActive(showErrorText);
			if (!showErrorText)
			{
				return;
			}
			if (string.IsNullOrEmpty(errorMsg))
			{
				return;
			}
			this._customMatchFeedbackLabel.text = errorMsg;
		}

		public void OnToggleClick_HideCodeAcess()
		{
			this._acessCodeUIInput.value = this._acessCodeUIInput.value.Trim();
			this._acessCodeUIInput.isSelected = true;
			this._acessCodeUIInput.defaultText = Language.Get("GAMEMODE_ACCESS_CODE_TITLE", TranslationSheets.MainMenuGui);
			if (this._hideCustomMatchCodeInputToggle.value)
			{
				this._acessCodeUIInput.inputType = UIInput.InputType.Password;
				return;
			}
			this._acessCodeUIInput.inputType = UIInput.InputType.Standard;
		}

		private void SetAndSaveCustomMatchByTheFirstTime()
		{
			PlayerPrefs.SetInt("CustomMatchAlreadyOpened", 1);
			PlayerPrefs.Save();
		}

		private IEnumerator CheckIfPlayerHaveEntryMatchModesScreen()
		{
			if (PlayerPrefs.HasKey("CustomMatchAlreadyOpened"))
			{
				if (this._newCustomMatchUILabelUILabel.gameObject.activeSelf)
				{
					this._newCustomMatchUILabelUILabel.gameObject.SetActive(false);
				}
				yield break;
			}
			yield return UnityUtils.WaitForOneSecond;
			this._newCustomMatchUILabelUILabel.gameObject.SetActive(true);
			this.SetAndSaveCustomMatchByTheFirstTime();
			yield break;
		}

		private void DeleteSaveFirstTimePlayerOpenCustomMatchGameMode()
		{
			PlayerPrefs.DeleteKey("CustomMatchAlreadyOpened");
		}

		private void CreateInfos()
		{
			this._normalGameInfo.queueName = GameModeTabs.Normal.ToString();
			this._normalGameInfo.icoTexture = this.normalTexture;
			this._normalGameInfo.backgroundTexture = this.normalBackgroundTexture;
			this._normalGameInfo.playersCount = Language.Get("SELECTION_MODE_PLAYERS", TranslationSheets.MainMenuGui);
			this._normalGameInfo.gameAvgTime = Language.Get("SELECTION_MODE_TIMER", TranslationSheets.MainMenuGui);
			this._normalGameInfo.description = Language.Get("SELECTION_MODE_PLAYERS_DESC", TranslationSheets.MainMenuGui);
			this._normalGameInfo.mode = GameModesGUI.GameType.Normal;
			this._botGameInfo.queueName = GameModeTabs.CoopVsBots.ToString();
			this._botGameInfo.icoTexture = this.botsTexture;
			this._botGameInfo.backgroundTexture = this.botsBackgroundTexture;
			this._botGameInfo.playersCount = Language.Get("SELECTION_MODE_COOP_PLAYERS", TranslationSheets.MainMenuGui);
			this._botGameInfo.gameAvgTime = Language.Get("SELECTION_MODE_COOP_TIMER", TranslationSheets.MainMenuGui);
			this._botGameInfo.description = Language.Get("SELECTION_MODE_COOP_DESCRIPTION_2", TranslationSheets.MainMenuGui);
			this._botGameInfo.mode = GameModesGUI.GameType.Bots;
			this._customMatchGameInfo.queueName = GameModeTabs.CustomMatch.ToString();
			this._customMatchGameInfo.icoTexture = this.customMatchTexture;
			this._customMatchGameInfo.backgroundTexture = this.CustomMatchBackgroundTexture;
			this._customMatchGameInfo.playersCount = Language.Get("SELECTION_MODE_PLAYERS", TranslationSheets.MainMenuGui);
			this._customMatchGameInfo.gameAvgTime = Language.Get("SELECTION_MODE_CUSTOM_MATCH_TIMER", TranslationSheets.MainMenuGui);
			this._customMatchGameInfo.description = Language.Get("SELECTION_MODE_CUSTOM_MATCH_DESCRIPTION_2", TranslationSheets.MainMenuGui);
			this._customMatchGameInfo.mode = GameModesGUI.GameType.Normal;
		}

		private void SetupRewardsInfo()
		{
			if (!this._isPlayerReloaded)
			{
				return;
			}
			float num = 0f;
			float num2 = 0f;
			InventoryAdapter inventory = GameHubBehaviour.Hub.User.Inventory.GetInventory((InventoryAdapter i) => ((InventoryBag)((JsonSerializeable<T>)i.Inventory.Bag)).Kind == InventoryBag.InventoryKind.Boosters);
			if (inventory != null)
			{
				InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<T>)inventory.Inventory.Bag);
				if (inventoryBag != null)
				{
					BoostersContent boostersContent = (BoostersContent)((JsonSerializeable<T>)inventoryBag.Content);
					if (boostersContent != null && (boostersContent.ScHours > 0 || boostersContent.XpHours > 0))
					{
						BoosterConfig boosterConfigs = GameHubBehaviour.Hub.SharedConfigs.BoosterConfigs;
						num2 = (float)boosterConfigs.XpBounsPercentage / 100f;
						num = (float)boosterConfigs.ScrapBounsPercentage / 100f;
					}
				}
			}
			AfkConfig.LevelInfo penalty = GameHubBehaviour.Hub.SharedConfigs.AfkConfigs.GetPenalty(GameHubBehaviour.Hub.User.Bag.LeaverStatus);
			float num3 = (penalty == null) ? 0f : ((float)penalty.PenaltyPercent / 100f);
			float num4 = 1f;
			num4 = num4 - num3 + num;
			RewardsInfo.BotXpMultiplier botXpMultiplier = GameHubBehaviour.Hub.Server.Rewards.GetBotXpMultiplier(GameHubBehaviour.Hub.User.GetTotalPlayerLevel());
			float num5 = botXpMultiplier.Multiplier;
			num5 = num5 - num3 + num2;
			float num6 = 1f;
			num6 = num6 - num3 + num;
			float num7 = 1f;
			num7 = num7 - num3 + num2;
			this.SetupXpRewardsTooltipInfo(num7 - 1f, num5 - 1f);
		}

		private void MainMenuOnPlayerReloadedEvent()
		{
			this._isPlayerReloaded = true;
			this.SetupRewardsInfo();
			this.SetupPvpTimerInfo();
			this.AfkPvpIcon.SetActive(GameHubBehaviour.Hub.User.Bag.LeaverStatus > 0);
			this.AfkCoopIcon.SetActive(GameHubBehaviour.Hub.User.Bag.LeaverStatus > 0);
		}

		private void SocialControllerOnGroupUpdate()
		{
			this.SetupPlayButton();
		}

		public void SetupPvpTimerInfo()
		{
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.SetPvpTimerOpenFulltime();
				this.IsPvpQueueOpened = true;
				return;
			}
			this._pvpTimerInfoFormat = Language.Get("MAIN_MENU_PVP_TIMER_FORMAT", TranslationSheets.MainMenuGui);
			RegionController instance = SingletonMonoBehaviour<RegionController>.Instance;
			if (instance == null || instance.CurrentRegionServerPing == null)
			{
				GameModesGUI.Log.Warn("RegionController not ready! Waiting for UpdateCurrentRegionOnSFServer to set CurrentRegionServerPing");
				return;
			}
			RegionAvailability.AvailabilityPeriod nextAvailableTime = instance.RegionAvailability.GetNextAvailableTime(DateTime.UtcNow, instance.CurrentRegionServerPing.Region.RegionNameI18N);
			if (nextAvailableTime.OpenTime == TimeSpan.Zero && nextAvailableTime.CloseTime == TimeSpan.Zero)
			{
				this.UpdatePVPTimeInfo(true, false);
				return;
			}
			if (nextAvailableTime.OpenTime == TimeSpan.Zero)
			{
				this.PvpTimerInfoGui.TitleLabel.text = Language.Get("MAIN_MENU_PVP_TIMER_TITLE_1", TranslationSheets.MainMenuGui);
				this._pvpTimerNextTime = nextAvailableTime.CloseTime;
				this.UpdatePVPTimeInfo(true, true);
			}
			else
			{
				this.PvpTimerInfoGui.TitleLabel.text = Language.Get("MAIN_MENU_PVP_TIMER_TITLE_2", TranslationSheets.MainMenuGui);
				this._pvpTimerNextTime = nextAvailableTime.OpenTime;
				this.UpdatePVPTimeInfo(false, true);
			}
			this._pvpTimerBaseDateTime = DateTime.UtcNow;
			this._pvpTimerLastSecond = -1;
		}

		public void SetPvpTimerOpenFulltime()
		{
			this.PvpTimerInfoGui.TitleLabel.gameObject.SetActive(false);
			this.PvpTimerInfoGui.TimeInfoLabel.gameObject.SetActive(false);
			if (this.PvpTimerInfoGui.BackgroundSprite == null)
			{
				return;
			}
			this.PvpTimerInfoGui.BackgroundSprite.sprite2D = this.PvpTimerInfoGui.GreenSprite;
			this.PvpTimerInfoGui.BackgroundSprite.alpha = this.PvpTimerInfoGui.GreenAlpha;
		}

		private void PvpTimerInfoUpdate()
		{
			if (!this.PvpTimerInfoGui.TitleLabel.gameObject.activeSelf)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				return;
			}
			DateTime utcNow = DateTime.UtcNow;
			if (this._pvpTimerLastSecond == utcNow.Second)
			{
				return;
			}
			this._pvpTimerLastSecond = utcNow.Second;
			TimeSpan timeSpan = this._pvpTimerNextTime.Subtract(utcNow - this._pvpTimerBaseDateTime);
			this.PvpTimerInfoGui.TimeInfoLabel.text = string.Format(this._pvpTimerInfoFormat, new object[]
			{
				timeSpan.Days.ToString("00"),
				timeSpan.Hours.ToString("00"),
				timeSpan.Minutes.ToString("00"),
				timeSpan.Seconds.ToString("00")
			});
			if (timeSpan.TotalSeconds < 0.0)
			{
				this.SetupPvpTimerInfo();
			}
		}

		private void UpdatePVPTimeInfo(bool isQueueOpen, bool showLabels)
		{
			this.IsPvpQueueOpened = isQueueOpen;
			this.PvpTimerInfoGui.TitleLabel.gameObject.SetActive(showLabels);
			this.PvpTimerInfoGui.TimeInfoLabel.gameObject.SetActive(showLabels);
			if (this.PvpTimerInfoGui.BackgroundSprite == null)
			{
				return;
			}
			this.PvpTimerInfoGui.BackgroundSprite.sprite2D = ((!isQueueOpen) ? this.PvpTimerInfoGui.YellowSprite : this.PvpTimerInfoGui.GreenSprite);
			this.PvpTimerInfoGui.BackgroundSprite.alpha = ((!isQueueOpen) ? this.PvpTimerInfoGui.YellowAlpha : this.PvpTimerInfoGui.GreenAlpha);
		}

		private void SetupXpRewardsTooltipInfo(float pvpReward, float pveReward)
		{
			bool flag = pvpReward > 0f;
			bool flag2 = pveReward > 0f;
			SimpleTooltipInfoData simpleTooltipInfoData = default(SimpleTooltipInfoData);
			simpleTooltipInfoData.TooltipType = SimpleTooltipType.Xp;
			simpleTooltipInfoData.TitleText = Language.Get("SELECTION_MODE_NORMAL", TranslationSheets.MainMenuGui);
			simpleTooltipInfoData.TitleTextColor = this.PvpTooltipTitleColor;
			simpleTooltipInfoData.DescriptionText = string.Format("+" + Language.Get("BOOSTERS_XP_GENERIC_DESCRIPTION", TranslationSheets.MainMenuGui), Mathf.RoundToInt(pvpReward * 100f));
			simpleTooltipInfoData.ExtraDescriptionText = Language.Get("SELECTION_MODE_NORMAL_DESCRIPTION", TranslationSheets.MainMenuGui);
			SimpleTooltipInfoData simpleTooltipInfoData2 = default(SimpleTooltipInfoData);
			simpleTooltipInfoData2.TooltipType = SimpleTooltipType.Xp;
			simpleTooltipInfoData2.TitleText = Language.Get("SELECTION_MODE_COOP", TranslationSheets.MainMenuGui);
			simpleTooltipInfoData2.TitleTextColor = this.PveTooltipTitleColor;
			simpleTooltipInfoData2.DescriptionText = string.Format("+" + Language.Get("BOOSTERS_XP_GENERIC_DESCRIPTION", TranslationSheets.MainMenuGui), Mathf.RoundToInt(pveReward * 100f));
			simpleTooltipInfoData2.ExtraDescriptionText = Language.Get("SELECTION_MODE_COOP_DESCRIPTION", TranslationSheets.MainMenuGui);
			SimpleTooltipInfoData simpleTooltipInfo = default(SimpleTooltipInfoData);
			simpleTooltipInfo.TooltipType = SimpleTooltipType.Xp;
			simpleTooltipInfo.TitleText = Language.Get("SELECTION_MODE_CUSTOM_MATCH", TranslationSheets.MainMenuGui);
			simpleTooltipInfo.TitleTextColor = this.CustomTooltipTitleColor;
			simpleTooltipInfo.DescriptionText = string.Format(Language.Get("DESCRIPTION_AFK_VALUES", TranslationSheets.MainMenuGui), 0, 0);
			this.CustomMatchButton.GetComponent<HMMTooltipTrigger>().TooltipText = simpleTooltipInfo.DescriptionText.Replace("0%", "0%");
			simpleTooltipInfo.ExtraDescriptionText = Language.Get("SELECTION_MODE_CUSTOM_MATCH_DESCRIPTION", TranslationSheets.MainMenuGui);
			this.CustomTooltipTrigger.Config.SimpleTooltipInfo = simpleTooltipInfo;
		}

		private void SetupMatchTooltipsInfo()
		{
			this.CustomTooltipTrigger.enabled = false;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GameModesGUI));

		public MatchStatsGui MatchStats;

		public GameModeTabs CurrentTab;

		public GameModeTabs GameMode;

		public GameObject rootGameObject;

		public GameObject gameModesGameObject;

		public GameObject gameSelectedGameObject;

		[SerializeField]
		private GameModesHoverView _gameModesHoverView;

		[Header("[Animation]")]
		[SerializeField]
		private Animation _mainAnimation;

		[SerializeField]
		private Animation _modesAnimation;

		[SerializeField]
		private Animation _customMatchAnimation;

		public Animation RegionGroupAnimation;

		public UIButton NormalButton;

		public UI2DSprite NormalButtonHoverSprite;

		public UIButton BotsButton;

		public UIButton CustomMatchButton;

		public UIButton CustomMatchCreateButton;

		public UIButton CustomMatchEnterButton;

		[Header("Game Selected")]
		public UILabel gameDescriptionLabel;

		public UILabel playersCountLabel;

		public UILabel gameAvgTimeLabel;

		public UITexture gameTypeTexture;

		public UITexture gameTypeBackgroundTexture;

		public GameObject FriendsButton;

		public GameObject PlayerVSPlayerGroup;

		public GameObject PlayerVSAIGroup;

		public GameObject CustomMatchGroup;

		public GameObject StartMatchButtonsGroup;

		public GameObject PlayButton;

		[Header("Resources")]
		public Texture normalTexture;

		public Texture botsTexture;

		public Texture customMatchTexture;

		public Texture normalBackgroundTexture;

		public Texture botsBackgroundTexture;

		public Texture CustomMatchBackgroundTexture;

		[Header("[Unlock PVP]")]
		public Animation PvpUiAnimation;

		public GameObject PvpLockLabelGroupGameObject;

		[Header("[Rewards]")]
		[SerializeField]
		private bool ShowMatchTooltipTriggerInfo;

		[SerializeField]
		private float DisabledRewardAlpha;

		[SerializeField]
		private PenaltyTooltipTrigger CustomTooltipTrigger;

		[SerializeField]
		private Color PvpTooltipTitleColor;

		[SerializeField]
		private Color PveTooltipTitleColor;

		[SerializeField]
		private Color CustomTooltipTitleColor;

		[Header("[AFK gamemodes]")]
		public GameObject AfkPvpIcon;

		public GameObject AfkCoopIcon;

		[Header("[AFK window]")]
		public GameObject AFKPivot;

		[Header("[PvP Timer]")]
		[SerializeField]
		private GameModesGUI.PvpTimerInfo PvpTimerInfoGui;

		private EscMenuGui OptionsScreen;

		private bool _isPlayerReloaded;

		private DateTime _pvpTimerBaseDateTime;

		private TimeSpan _pvpTimerNextTime;

		private int _pvpTimerLastSecond;

		private string _pvpTimerInfoFormat;

		private bool _isPvpUnlokedByLevel;

		private bool _isPvpQueueOpened;

		private GameModesGUI.GameModeInfo _normalGameInfo;

		private GameModesGUI.GameModeInfo _botGameInfo;

		private GameModesGUI.GameModeInfo _customMatchGameInfo;

		private UIPanel _rootPanel;

		[Header("Custom Match")]
		[SerializeField]
		private UILabel _customMatchFeedbackLabel;

		[SerializeField]
		private UI2DSprite _customMatchFeedbackBorderInfocodeAcessSprite;

		[SerializeField]
		private UILabel _enterLobbyButtonUILabel;

		[SerializeField]
		private UILabel _newCustomMatchUILabelUILabel;

		[SerializeField]
		private UIInput _acessCodeUIInput;

		private bool _subscribedToLobbyEvents;

		[SerializeField]
		private UIToggle _allowSpectatorsToggle;

		[SerializeField]
		private UIToggle _hideCustomMatchCodeInputToggle;

		private const string GameModesAlreadyOpenedPlayerPrefsKey = "CustomMatchAlreadyOpened";

		[Serializable]
		private struct RewardInfo
		{
			public GameObject RewardInfoGroupGameObject;

			public UILabel FamePercentLabel;

			public UILabel XpPercentLabel;
		}

		[Serializable]
		private struct PvpTimerInfo
		{
			public GameObject MainGroupGameObject;

			public UILabel TitleLabel;

			public UILabel TimeInfoLabel;

			public UI2DSprite BackgroundSprite;

			public Sprite GreenSprite;

			public float GreenAlpha;

			public Sprite YellowSprite;

			public float YellowAlpha;
		}

		private struct GameModeInfo
		{
			public string queueName;

			public Texture icoTexture;

			public Texture backgroundTexture;

			public string playersCount;

			public string gameAvgTime;

			public string description;

			public string queueAvgTime;

			public string queueAvgTimeFull;

			public GameModesGUI.GameType mode;
		}

		public enum GameType
		{
			Normal,
			Bots
		}

		public static class MatchBlocker
		{
			public static void BlockPlayer()
			{
				GameModesGUI.MatchBlocker._isPlayerBlocked = true;
				GameModesGUI.MatchBlocker._playerBlockedTimeInSec = Time.time;
			}

			public static void UnblockPlayer()
			{
				GameModesGUI.MatchBlocker._isPlayerBlocked = false;
			}

			public static bool IsBlocked()
			{
				return GameModesGUI.MatchBlocker._isPlayerBlocked;
			}

			public static float GetEndBlockingTime()
			{
				return GameModesGUI.MatchBlocker._playerBlockedTimeInSec + 180f;
			}

			public const int MaxBlockedTimeInSec = 180;

			private static bool _isPlayerBlocked;

			private static float _playerBlockedTimeInSec;
		}
	}
}
