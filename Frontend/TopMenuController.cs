using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.CustomMatch;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Infra;
using Assets.Standard_Assets.Scripts.HMM.PlotKids.Social;
using ClientAPI.Exceptions;
using ClientAPI.Matchmaking;
using ClientAPI.Matchmaking.Lobby;
using ClientAPI.Objects;
using ClientAPI.Objects.Partial;
using Commons.Swordfish.Battlepass;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using HeavyMetalMachines.VFX.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class TopMenuController : GameHubBehaviour, ISwordfishWebServiceTimeOut
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnUpdateBoosterInfo;

		private void UpdateRemainingBoosterTime(BoostersContent boostersContent)
		{
			this._remainingFameBoosterTime = this.GetRemainingTimeSpan(boostersContent.StartDateSc, boostersContent.ScHours);
			this._remainingXpBoosterTime = this.GetRemainingTimeSpan(boostersContent.StartDateXp, boostersContent.XpHours);
		}

		private TimeSpan GetRemainingTimeSpan(long startDate, int hours)
		{
			DateTime dateTime = new DateTime(startDate);
			DateTime d = dateTime.AddHours((double)hours);
			return d - DateTime.UtcNow - SingletonMonoBehaviour<RegionController>.Instance.ServerTimeOffset;
		}

		private void OnGroupUpdated()
		{
			if (HMMHub.IsEditorLeavingPlayMode())
			{
				return;
			}
			GroupManager groupManager = ManagerController.Get<GroupManager>();
			GroupStatus selfGroupStatus = groupManager.GetSelfGroupStatus();
			bool isUserInGroupOrPendingInvite = groupManager.IsUserInGroupOrPendingInvite;
			bool isUserInGroupOrIsOwner = groupManager.IsUserInGroupOrIsOwner;
			UnityEngine.Debug.Log(string.Format("isPlayerInGroupOrPendingInvite: {0}; Party Status: {1}", isUserInGroupOrPendingInvite, selfGroupStatus));
			bool flag = !this._currentGroupID.Equals(groupManager.CurrentGroupID) || isUserInGroupOrPendingInvite != this._mainGroupGameObject.activeSelf;
			this._currentGroupID = groupManager.CurrentGroupID;
			if (flag)
			{
				this._mainGroupGameObject.SetActive(isUserInGroupOrIsOwner);
				this._leaveGroupButtonGameObject.SetActive(isUserInGroupOrIsOwner);
				this._portraitGameObject.SetActive(!isUserInGroupOrIsOwner);
				if (!isUserInGroupOrPendingInvite)
				{
					this._selfGroupLeader2DSprite.gameObject.SetActive(false);
				}
			}
			this.RefreshGroupCustomizations();
			this.UpdateTeamsInfo();
			this.RefreshGroupInviteButtonsStatus();
		}

		public void onButtonClick_OpenGroupInviteContextMenu()
		{
			this._groupInviteMenu.onButtonClick_OpenGroupInviteContextMenu();
		}

		public void onButtonClick_LeaveGroup()
		{
			ManagerController.Get<GroupManager>().LeaveGroup(false);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action<int, float, int> OnClientSetPlayerLevel;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event TopMenuController.OnCashButtonClicked CashButtonClicked;

		private void Awake()
		{
			if (SpectatorController.IsSpectating)
			{
				this._portraitGroupGameObject.SetActive(false);
				return;
			}
			this.InitBoosterInfo();
			TopMenuController.OnClientSetPlayerLevel += this.UpdateUserLevel;
			this.ResetTeamTimer();
		}

		private void OnDestroy()
		{
			TopMenuController.OnClientSetPlayerLevel -= this.UpdateUserLevel;
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.State.ListenToStateChanged += this.ListenToStateChanged;
			if (SpectatorController.IsSpectating)
			{
				this._intializedEvents = true;
				return;
			}
			ManagerController.Get<FriendManager>().EvtFriendListUpdated += this.OnFriendListUpdated;
			ManagerController.Get<GroupManager>().OnGroupUpdate += this.OnGroupUpdated;
			MatchManager matchManager = ManagerController.Get<MatchManager>();
			matchManager.EvtLobbyJoined += this.OnLobbyJoined;
			matchManager.EvtLobbyFinished += this.OnLobbyFinished;
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientConnectedEvent += this.RefreshGroupInviteButtonsStatus;
			GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientDisconnectedEvent += this.RefreshGroupInviteButtonsStatus;
			GameHubBehaviour.Hub.ClientApi.matchmakingClient.Disconnection += this.OnMatchmakingDisconnected;
			GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchMade += this.OnMatchmakingMade;
			GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchStarted += this.OnMatchmakingStarted;
			GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchConfirmed += this.OnMatchConfirmed;
			GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchCanceled += this.OnMatchmakingCanceled;
			GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchAccepted += this.OnMatchmakingAccepted;
			GameHubBehaviour.Hub.Store.OnBalanceReloaded += this.UpdateCurrencyLabels;
			GameHubBehaviour.Hub.User.Inventory.OnInvetoryReload += this.UpdateCurrencyLabels;
			this._intializedEvents = true;
		}

		public void PreventGroupMemberRemoval()
		{
			this._groupMemberRemovalLocker.StartPreventMemberRemoval();
		}

		private void OnMatchmakingDisconnected(object sender, MatchmakingEventArgs args)
		{
			this._groupMemberRemovalLocker.InterruptPreventMemberRemoval();
		}

		private void OnDisable()
		{
			if (!this._intializedEvents)
			{
				return;
			}
			if (SingletonMonoBehaviour<ManagerController>.DoesInstanceExist())
			{
				if (ManagerController.Get<GroupManager>() != null)
				{
					ManagerController.Get<GroupManager>().OnGroupUpdate -= this.OnGroupUpdated;
				}
				ManagerController.Get<FriendManager>().EvtFriendListUpdated -= this.OnFriendListUpdated;
				MatchManager matchManager = ManagerController.Get<MatchManager>();
				matchManager.EvtLobbyJoined -= this.OnLobbyJoined;
				matchManager.EvtLobbyFinished -= this.OnLobbyFinished;
			}
			if (GameHubBehaviour.Hub == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking != null)
			{
				GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientConnectedEvent -= this.RefreshGroupInviteButtonsStatus;
				GameHubBehaviour.Hub.Swordfish.Msg.Matchmaking.OnClientDisconnectedEvent -= this.RefreshGroupInviteButtonsStatus;
			}
			if (GameHubBehaviour.Hub.ClientApi != null)
			{
				GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchMade -= this.OnMatchmakingMade;
				GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchStarted -= this.OnMatchmakingStarted;
				GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchConfirmed -= this.OnMatchConfirmed;
				GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchCanceled -= this.OnMatchmakingCanceled;
				GameHubBehaviour.Hub.ClientApi.matchmakingClient.MatchAccepted -= this.OnMatchmakingAccepted;
			}
			GameHubBehaviour.Hub.State.ListenToStateChanged -= this.ListenToStateChanged;
			GameHubBehaviour.Hub.Store.OnBalanceReloaded -= this.UpdateCurrencyLabels;
			GameHubBehaviour.Hub.User.Inventory.OnInvetoryReload -= this.UpdateCurrencyLabels;
		}

		private void Start()
		{
			this._teamGuiComponents.NoTeamGameObject.SetActive(true);
			this._teamGuiComponents.TeamGameObject.SetActive(false);
			this._teamGuiComponents.GlowFullGameObject.SetActive(false);
			this._teamGuiComponents.GlowIconGameObject.SetActive(false);
			this._teamGuiComponents.NameLabel.gameObject.SetActive(false);
			this._groupMemberRemovalLocker = new GroupMemberRemovalLocker(this._leaveGroupButtonGameObject, this._sortedGroupMemberGuiItems, this);
			this._groupMemberRemovalLocker.SetLockingInterval(3);
		}

		private void Update()
		{
			if (!this.TopPlayTween.gameObject.activeSelf)
			{
				return;
			}
			this.BoosterUpdate();
			this.TeamsUpdate();
		}

		private void BoosterUpdate()
		{
			this._boosterLastUpdate -= Time.deltaTime;
			if (this._boosterLastUpdate > 0f)
			{
				return;
			}
			this._boosterLastUpdate = this.BossterUpdateTimeSec;
			this.UpdateBoosterInfo();
		}

		public bool TryToGetBoosterFameInfo(out string boosterInfo)
		{
			return this.TryToGetBoosterInfo(out boosterInfo, "BOOSTERS_FAME_GENERIC_TITLE", this._remainingFameBoosterTime);
		}

		public bool TryToGetBoosterXpInfo(out string boosterInfo)
		{
			return this.TryToGetBoosterInfo(out boosterInfo, "BOOSTERS_XP_GENERIC_TITLE", this._remainingXpBoosterTime);
		}

		private bool TryToGetBoosterInfo(out string boosterInfo, string titleDraft, TimeSpan remainingBoosterTimeSpan)
		{
			boosterInfo = string.Empty;
			if (remainingBoosterTimeSpan.TotalSeconds <= 0.0)
			{
				return false;
			}
			boosterInfo = string.Format("{0}. {1}.", Language.Get(titleDraft, TranslationSheets.MainMenuGui), this.GetTimeRemainingText(remainingBoosterTimeSpan));
			return true;
		}

		private void OnFriendListUpdated(FriendManager friendmanager)
		{
			this.RefreshGroupInviteButtonsStatus();
		}

		private void OnMatchmakingMade(object sender, MatchmakingEventArgs e)
		{
			this._groupMemberRemovalLocker.InterruptPreventMemberRemoval();
			this.RefreshGroupInviteButtonsStatus();
		}

		private void OnMatchmakingAccepted(object sender, MatchAcceptedArgs e)
		{
			this.RefreshGroupInviteButtonsStatus();
		}

		private void OnMatchmakingCanceled(object sender, MatchCancelledArgs e)
		{
			this._groupMemberRemovalLocker.InterruptPreventMemberRemoval();
			this.RefreshGroupInviteButtonsStatus();
		}

		private void OnMatchConfirmed(object sender, MatchmakingEventArgs e)
		{
			this.RefreshGroupInviteButtonsStatus();
		}

		private void OnMatchmakingStarted(object sender, MatchStartedEventArgs e)
		{
			this.RefreshGroupInviteButtonsStatus();
		}

		private void OnLobbyFinished(Lobby lobby, LobbyMatchmakingMessage.LobbyMessageErrorType lobbyErrorType)
		{
			this.RefreshGroupInviteButtonsStatus();
		}

		private void OnLobbyJoined(Lobby lobby)
		{
			this.RefreshGroupInviteButtonsStatus();
		}

		private void RefreshGroupInviteButtonsStatus()
		{
			bool flag = ManagerController.Get<FriendManager>().AvailableFriendsToInvite.Count > 0;
			bool isUserPendingInvite = ManagerController.Get<GroupManager>().IsUserPendingInvite;
			bool isUserInLobby = ManagerController.Get<MatchManager>().IsUserInLobby;
			bool flag2 = GameHubBehaviour.Hub.IsWaitingInQueue();
			bool flag3 = flag && !isUserPendingInvite && !isUserInLobby && !flag2;
			for (int i = 0; i < this._inviteToGroupButtons.Length; i++)
			{
				this._inviteToGroupButtons[i].SetActive(flag3);
			}
			for (int j = 0; j < this._inviteToGroupDisabledButton.Length; j++)
			{
				this._inviteToGroupDisabledButton[j].SetActive(!flag3);
			}
			if (!flag3)
			{
				CantInviteToGroupFeedbackContent cantInviteToGroupFeedbackContent = CantInviteToGroupFeedback.GetCantInviteToGroupFeedbackContent();
				for (int k = 0; k < this._inviteDisabledButtonTooltip.Length; k++)
				{
					this._inviteDisabledButtonTooltip[k].TooltipText = cantInviteToGroupFeedbackContent.I18Key;
					this._inviteDisabledButtonTooltip[k].Sheet = cantInviteToGroupFeedbackContent.I18Sheet;
				}
			}
			this._hardCoinsButton.GetComponent<Collider>().enabled = !isUserInLobby;
			this._hardCoinsButton.SetState((!isUserInLobby) ? UIButtonColor.State.Normal : UIButtonColor.State.Disabled, true);
			this._hardCoinsButtonDisabledGameObject.SetActive(isUserInLobby);
		}

		private void ListenToStateChanged(GameState pChangedstate)
		{
			this.currentChangedstate = pChangedstate;
			this.RefreshGroupInviteButtonsStatus();
			if (this.currentChangedstate is Game || this.currentChangedstate is PickModeSetup)
			{
				this.TopPlayTween.gameObject.SetActive(false);
				return;
			}
			this.ResetTeamTimer();
			this.UpdateTeamsInfo();
		}

		public void OnCashClicked()
		{
			if (this.CashButtonClicked != null)
			{
				this.CashButtonClicked();
			}
		}

		public void SetPlayerName(string playerName)
		{
			this.PlayerName.text = NGUIText.EscapeSymbols(playerName);
		}

		public void UpdateUserLevel(int level, float progress, int totalLevel)
		{
			this.UserLevelLabel.text = (totalLevel + 1).ToString("0");
			this.UserLevelProgressBar.value = progress;
			if (GameHubBehaviour.Hub.ClientApi.friend != null)
			{
				FriendBag friendBag = (FriendBag)((JsonSerializeable<T>)GameHubBehaviour.Hub.ClientApi.friend.GetMyBag());
				if (friendBag != null)
				{
					friendBag.Level = totalLevel;
				}
				ManagerController.Get<FriendBagManager>().SaveMyFriendBagOnNextFrame();
			}
		}

		public void UpdateCurrencyLabels()
		{
			GameHubBehaviour.Hub.GuiScripts.TopMenu.UpdateCurrencyLabels(GameHubBehaviour.Hub.Store.SoftCurrency.ToString(), GameHubBehaviour.Hub.Store.HardCurrency.ToString());
			bool flag;
			if (GameHubBehaviour.Hub.User.Bag.LeaverStatus > 0)
			{
				flag = true;
				this.AFK_Group.SetActive(true);
			}
			else
			{
				flag = false;
				this.AFK_Group.SetActive(false);
			}
			bool flag2 = this.UpdateBoosterInfo();
			this.BuffBarPivot.SetActive(flag || flag2);
		}

		private void InitBoosterInfo()
		{
			this.BuffBoosterXpTooltipTrigger.Config.SimpleTooltipInfo.ExtraDescriptionText = string.Empty;
			this.BuffBoosterFameTooltipTrigger.Config.SimpleTooltipInfo.ExtraDescriptionText = string.Empty;
			this.BuffFounderPackTooltipTrigger.Config.SimpleTooltipInfo.ExtraDescriptionText = string.Empty;
			this.SetEnabledXpBooster(false);
			this.SetEnabledFameBooster(false);
			this.SetEnabledFounderBooster(false);
			this._remainingXpBoosterTime = default(TimeSpan);
			this._remainingFameBoosterTime = default(TimeSpan);
		}

		private void SetEnabledXpBooster(bool enable)
		{
			this.BuffBoosterXpTooltipTrigger.enabled = enable;
			this._boosterXpDisabledTooltip.enabled = !enable;
			this._boosterXpImage.color = ((!enable) ? this._disabledBoosterColor : this._enabledeBoosterColor);
		}

		private void SetEnabledFameBooster(bool enable)
		{
			this.BuffBoosterFameTooltipTrigger.enabled = enable;
			this._boosterFameDisabledTooltip.enabled = !enable;
			this._boosterFameImage.color = ((!enable) ? this._disabledBoosterColor : this._enabledeBoosterColor);
		}

		private void SetEnabledFounderBooster(bool enable)
		{
			this.BuffFounderPackTooltipTrigger.enabled = enable;
			this._boosterFounderPackDisabledTooltip.enabled = !enable;
			this._boosterFounderPackImage.color = ((!enable) ? this._disabledBoosterColor : this._enabledeBoosterColor);
		}

		private bool UpdateBoosterInfo()
		{
			this.SetEnabledXpBooster(false);
			this.SetEnabledFameBooster(false);
			this.SetEnabledFounderBooster(false);
			if (SpectatorController.IsSpectating)
			{
				return false;
			}
			bool flag = this.SetupBoosterInfo();
			flag |= this.SetupFounderInfo();
			if (this.OnUpdateBoosterInfo != null)
			{
				this.OnUpdateBoosterInfo();
			}
			return flag;
		}

		private bool SetupFounderInfo()
		{
			PlayerBag playerBag = (PlayerBag)GameHubBehaviour.Hub.User.PlayerSF.Bag;
			if (playerBag == null)
			{
				return false;
			}
			FounderLevel founderPackLevel = (FounderLevel)playerBag.FounderPackLevel;
			if (!FoundersBoosterGui.HasFounderBooster(founderPackLevel))
			{
				return false;
			}
			int bonusPercentage = FoundersBoosterGui.GetBonusPercentage(founderPackLevel, GameHubBehaviour.Hub);
			if (bonusPercentage <= 0)
			{
				return false;
			}
			this.SetEnabledFounderBooster(true);
			this.UpdateSimpleTooltipData(this.BuffFounderPackTooltipTrigger, SimpleTooltipType.FounderPack, default(TimeSpan));
			this._boosterFounderPackImage.SpriteName = FoundersBoosterGui.GetSpriteName(founderPackLevel, FoundersBoosterGui.SpriteType.MainMenuBooster);
			return true;
		}

		private bool SetupBoosterInfo()
		{
			InventoryAdapter inventory = GameHubBehaviour.Hub.User.Inventory.GetInventory((InventoryAdapter i) => ((InventoryBag)((JsonSerializeable<T>)i.Inventory.Bag)).Kind == InventoryBag.InventoryKind.Boosters);
			if (inventory == null)
			{
				return false;
			}
			InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<T>)inventory.Inventory.Bag);
			BoostersContent boostersContent = (BoostersContent)((JsonSerializeable<T>)inventoryBag.Content);
			if (boostersContent == null)
			{
				return false;
			}
			bool result = false;
			this.UpdateRemainingBoosterTime(boostersContent);
			if (this._remainingXpBoosterTime.TotalSeconds > 0.0)
			{
				this.SetEnabledXpBooster(true);
				this.UpdateSimpleTooltipData(this.BuffBoosterXpTooltipTrigger, SimpleTooltipType.Xp, this._remainingXpBoosterTime);
				result = true;
			}
			if (this._remainingFameBoosterTime.TotalSeconds > 0.0)
			{
				this.SetEnabledFameBooster(true);
				this.UpdateSimpleTooltipData(this.BuffBoosterFameTooltipTrigger, SimpleTooltipType.Fame, this._remainingFameBoosterTime);
				result = true;
			}
			return result;
		}

		public bool IsBoosterActive()
		{
			return this._remainingXpBoosterTime.TotalSeconds > 0.0;
		}

		private void UpdateSimpleTooltipData(PenaltyTooltipTrigger penaltyTooltipTrigger, SimpleTooltipType type, TimeSpan boosterTimeSpan = default(TimeSpan))
		{
			PenaltyTooltipController instance = SingletonMonoBehaviour<PenaltyTooltipController>.Instance;
			SimpleTooltipInfoData simpleTooltipInfo = default(SimpleTooltipInfoData);
			BoosterConfig boosterConfigs = GameHubBehaviour.Hub.SharedConfigs.BoosterConfigs;
			string key = string.Empty;
			string key2 = string.Empty;
			string extraDescriptionText = string.Empty;
			int num = 0;
			Color titleTextColor = Color.white;
			switch (type)
			{
			case SimpleTooltipType.Fame:
				key = "BOOSTERS_FAME_GENERIC_TITLE";
				key2 = "BOOSTERS_FAME_GENERIC_DESCRIPTION";
				num = boosterConfigs.ScrapBounsPercentage;
				titleTextColor = instance.SimpleTitleFameColor;
				extraDescriptionText = this.GetTimeRemainingText(boosterTimeSpan);
				break;
			case SimpleTooltipType.Xp:
				key = "BOOSTERS_XP_GENERIC_TITLE";
				key2 = "BOOSTERS_XP_GENERIC_DESCRIPTION";
				num = boosterConfigs.XpBounsPercentage;
				titleTextColor = instance.SimpleTitleXpColor;
				extraDescriptionText = this.GetTimeRemainingText(boosterTimeSpan);
				break;
			case SimpleTooltipType.FounderPack:
			{
				key = "FOUNDERPACK_GENERIC_TITLE";
				key2 = "BOOSTERS_XP_GENERIC_DESCRIPTION";
				PlayerBag playerBag = (PlayerBag)GameHubBehaviour.Hub.User.PlayerSF.Bag;
				FounderLevel founderPackLevel = (FounderLevel)playerBag.FounderPackLevel;
				num = FoundersBoosterGui.GetBonusPercentage(founderPackLevel, GameHubBehaviour.Hub);
				titleTextColor = instance.SimpleTitleFounderPackColor;
				extraDescriptionText = FoundersBoosterGui.GetDescription(founderPackLevel);
				break;
			}
			default:
				HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("Undefined TopMenu SimpleTooltipType:[{0}]", type), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
				break;
			}
			simpleTooltipInfo.TooltipType = type;
			simpleTooltipInfo.TitleText = Language.Get(key, TranslationSheets.MainMenuGui);
			simpleTooltipInfo.TitleTextColor = titleTextColor;
			simpleTooltipInfo.DescriptionText = string.Format("+" + Language.Get(key2, TranslationSheets.MainMenuGui), num);
			simpleTooltipInfo.ExtraDescriptionText = extraDescriptionText;
			penaltyTooltipTrigger.Config.SimpleTooltipInfo = simpleTooltipInfo;
			if (instance.IsSimpleTooltipVisible(penaltyTooltipTrigger) && instance.CurrentSimpleTooltipType == type)
			{
				penaltyTooltipTrigger.ShowTooltip(penaltyTooltipTrigger.Config);
			}
		}

		private string GetTimeRemainingText(TimeSpan remainingBoosterTimeSpan)
		{
			int days = remainingBoosterTimeSpan.Days;
			int hours = remainingBoosterTimeSpan.Hours;
			int minutes = remainingBoosterTimeSpan.Minutes;
			string text = Language.Get("BOOSTERS_TIME_REMAINING", TranslationSheets.MainMenuGui);
			if (days == 0 && hours == 0 && minutes == 0)
			{
				text += string.Format(" {0} 1 {1}", Language.Get("BOOSTERS_TIME_LESSTHAN", TranslationSheets.MainMenuGui), Language.Get("ACTIVE_BOOSTER_DURATIONTYPE_MATCHTIME", TranslationSheets.MainMenuGui));
			}
			else
			{
				bool flag = false;
				if (days > 0)
				{
					flag = true;
					text += string.Format(" {0} {1}", days, Language.Get("ACTIVE_BOOSTER_DAY", TranslationSheets.MainMenuGui));
				}
				bool flag2 = false;
				if (hours > 0)
				{
					flag2 = true;
					text += string.Format("{0}{1} {2}", (!flag) ? " " : ", ", hours, Language.Get("ACTIVE_BOOSTER_DURATIONTYPE_HOURS", TranslationSheets.MainMenuGui));
				}
				if (minutes > 0)
				{
					text += string.Format("{0}{1} {2}", (!flag && !flag2) ? " " : ", ", minutes, Language.Get("ACTIVE_BOOSTER_DURATIONTYPE_MATCHTIME", TranslationSheets.MainMenuGui));
				}
			}
			return text;
		}

		public void UpdateCurrencyLabels(string soft, string hard)
		{
			this.softCoins.text = soft;
			this.softCoins.TryUpdateText();
			NGUITools.UpdateWidgetCollider(this.softCoins);
			this.hardCoins.text = hard;
			this.HardCoinsSprite.UpdateAnchors();
			this.hardCoins.UpdateAnchors();
		}

		public void UpdatePlayerIcon()
		{
			this.PortraitControl.UpdatePlayerIcon(GameHubBehaviour.Hub.User.UniversalId);
			PlayerBag playerBag = (PlayerBag)GameHubBehaviour.Hub.User.PlayerSF.Bag;
			FounderLevel founderPackLevel = (FounderLevel)playerBag.FounderPackLevel;
		}

		public void RefreshPlayerCustomizations()
		{
			Inventory inventoryByKind = GameHubBehaviour.Hub.User.Inventory.GetInventoryByKind(InventoryBag.InventoryKind.Customization);
			InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<T>)inventoryByKind.Bag);
			GameHubBehaviour.Hub.User.Inventory.Customizations = (CustomizationContent)((JsonSerializeable<T>)inventoryBag.Content);
			this.UpdatePortrait();
		}

		public void UpdatePortrait()
		{
			PortraitDecoratorGui.UpdatePortraitSprite(GameHubBehaviour.Hub.User.Inventory.Customizations, this.PortraitFounderSprite, PortraitDecoratorGui.PortraitSpriteType.Corner);
		}

		public void RefreshGroupCustomizations()
		{
			GroupManager groupManager = ManagerController.Get<GroupManager>();
			int num = 0;
			for (int i = 0; i < groupManager.GroupMembersSortedList.Count; i++)
			{
				GroupMember groupMember = groupManager.GroupMembersSortedList[i];
				string universalID = groupMember.UniversalID;
				if (string.Equals(universalID, GameHubBehaviour.Hub.User.UniversalId, StringComparison.InvariantCultureIgnoreCase))
				{
					this._selfGroupLeader2DSprite.gameObject.SetActive(groupMember.IsOwner);
				}
				else
				{
					this._sortedGroupMemberGuiItems[num].SetProperties(groupMember);
					num++;
				}
			}
			for (int j = num; j < this._sortedGroupMemberGuiItems.Length; j++)
			{
				this._sortedGroupMemberGuiItems[j].SetProperties(null);
			}
		}

		public void AnimateEnterMainMenu()
		{
			this.TopPlayTween.tweenGroup = 1;
			this.TopPlayTween.Play();
		}

		public void RefreshUserXp()
		{
			this.RefreshUserXp(this.XpTooltip);
		}

		private void RefreshUserXp(HMMTooltipTrigger xpTooltip)
		{
			BattlepassConfig battlepass = GameHubBehaviour.Hub.SharedConfigs.Battlepass;
			BattlepassProgress progress = this._battlepassProgressScriptableObject.Progress;
			int totalPlayerLevel = GameHubBehaviour.Hub.User.GetTotalPlayerLevel();
			int currentXp = progress.CurrentXp;
			int levelForXp = battlepass.GetLevelForXp(currentXp);
			int num = currentXp - battlepass.Levels[levelForXp].XP;
			float arg;
			if (levelForXp + 1 >= battlepass.Levels.Length)
			{
				arg = 1f;
				xpTooltip.TooltipText = string.Format("{0} / {1}", currentXp, battlepass.Levels[levelForXp].XP);
			}
			else
			{
				int num2 = battlepass.Levels[levelForXp + 1].XP - battlepass.Levels[levelForXp].XP;
				arg = (float)num / (float)num2;
				xpTooltip.TooltipText = string.Format("{0} / {1}", num, num2);
			}
			if (TopMenuController.OnClientSetPlayerLevel != null)
			{
				TopMenuController.OnClientSetPlayerLevel(levelForXp, arg, totalPlayerLevel);
			}
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

		public void onButtonClick_OpenSelfContextVoiceChat()
		{
			SelfContextVoiceChatPanel selfContextVoiceChatPanel;
			SingletonMonoBehaviour<PanelController>.Instance.ShowModalWindow<SelfContextVoiceChatPanel>(out selfContextVoiceChatPanel);
			selfContextVoiceChatPanel.ParentGUI = this._parentUI;
		}

		private void UpdateTeamsInfo()
		{
			if (SpectatorController.IsSpectating)
			{
				this._teamGuiComponents.MainGroupGameObject.SetActive(false);
				this._teamGuiComponents.NoTeamGameObject.SetActive(false);
				this._teamGuiComponents.TeamGameObject.SetActive(false);
				this._teamGuiComponents.GlowFullGameObject.SetActive(false);
				this._teamGuiComponents.GlowIconGameObject.SetActive(false);
				this._teamGuiComponents.NameLabel.gameObject.SetActive(false);
				return;
			}
			this._teamGuiComponents.MainGroupGameObject.SetActive(true);
			this.ResetTeamTimer();
			if (string.IsNullOrEmpty(GameHubBehaviour.Hub.User.UniversalId))
			{
				this._teamGuiComponents.NoTeamGameObject.SetActive(true);
				this._teamGuiComponents.TeamGameObject.SetActive(false);
				this._teamGuiComponents.GlowFullGameObject.SetActive(false);
				this._teamGuiComponents.GlowIconGameObject.SetActive(false);
				this._teamGuiComponents.NameLabel.gameObject.SetActive(false);
				return;
			}
			if (!GameHubBehaviour.Hub.ClientApi.IsLogged)
			{
				TopMenuController.Log.ErrorFormat("someone is trying to GetTeamByUniversalId but is not logged. stack{0}", new object[]
				{
					StackTraceUtility.ExtractStackTrace()
				});
				return;
			}
			GameHubBehaviour.Hub.ClientApi.team.GetTeamByUniversalId(this, GameHubBehaviour.Hub.User.UniversalId, delegate(object state, Team team)
			{
				this.TryToChangeConnectionFeedback(false);
				if (team == null)
				{
					this._teamGuiComponents.NoTeamGameObject.SetActive(true);
					this._teamGuiComponents.TeamGameObject.SetActive(false);
					this._teamGuiComponents.GlowFullGameObject.SetActive(false);
					this._teamGuiComponents.NameLabel.gameObject.SetActive(false);
					GameHubBehaviour.Hub.ClientApi.team.GetInvitationsForUniversalId(null, GameHubBehaviour.Hub.User.UniversalId, delegate(object s, TeamInviteModelView[] teams)
					{
						this._teamGuiComponents.GlowIconGameObject.SetActive(teams != null && teams.Length > 0);
					}, delegate(object s, Exception exception)
					{
						string format = "Error on GetInvitationsForUniversalId. UniversalId:[{0}]. Exception:{1}";
						if (exception is ConnectionException)
						{
							TopMenuController.Log.WarnFormat(format, new object[]
							{
								GameHubBehaviour.Hub.User.UniversalId,
								exception
							});
						}
						else
						{
							TopMenuController.Log.ErrorFormat(format, new object[]
							{
								GameHubBehaviour.Hub.User.UniversalId,
								exception
							});
						}
						this._teamGuiComponents.GlowIconGameObject.SetActive(false);
					});
					return;
				}
				this._teamGuiComponents.NoTeamGameObject.SetActive(false);
				this._teamGuiComponents.TeamGameObject.SetActive(true);
				this._teamGuiComponents.NameLabel.text = TeamUtils.GetTagColoredEncoded(team.Tag, GameHubBehaviour.Hub.GuiScripts.GUIColors.TeamTagColor);
				this._teamGuiComponents.NameLabel.gameObject.SetActive(true);
				this._teamGuiComponents.IconSprite.SpriteName = team.ImageUrl;
				this._glowTeamId = Guid.Empty;
				GameHubBehaviour.Hub.ClientApi.team.TeamHasChangesForUser(this, team.Id, GameHubBehaviour.Hub.User.UniversalId, delegate(object s, bool teamHasChanges)
				{
					this._glowTeamId = ((!teamHasChanges) ? Guid.Empty : team.Id);
					this._teamGuiComponents.GlowIconGameObject.SetActive(teamHasChanges);
				}, delegate(object s, Exception exception)
				{
					TopMenuController.Log.Error(string.Format("Error on TeamHasChangesForUser. TeamId:[{0}], UniversalId:[{1}]. Exception:{2}", team.Id, GameHubBehaviour.Hub.User.UniversalId, exception));
					this._teamGuiComponents.GlowIconGameObject.SetActive(false);
				});
				this._teamGuiComponents.GlowFullGameObject.SetActive(false);
				if (ManagerController.Get<GroupManager>().IsUserInGroupOrIsOwner && ManagerController.Get<GroupManager>().GroupMembersSortedList.Count == 4)
				{
					List<string> list = new List<string>(4);
					for (int i = 0; i < ManagerController.Get<GroupManager>().GroupMembersSortedList.Count; i++)
					{
						list.Add(ManagerController.Get<GroupManager>().GroupMembersSortedList[i].UniversalID);
					}
					TeamUtils.GetGroupTeamAsync(GameHubBehaviour.Hub, list, delegate(Team groupTeam)
					{
						this._teamGuiComponents.GlowFullGameObject.SetActive(null != groupTeam);
					}, delegate(Exception exception)
					{
						TopMenuController.Log.Error(string.Format("Error on GetGroupTeamAsync. Exception:{0}", exception));
						this._teamGuiComponents.GlowFullGameObject.SetActive(false);
					});
				}
			}, delegate(object state, Exception exception)
			{
				string format = "Error on GetTeamByUniversalId [{0}]. Exception:{1}";
				if (exception is ConnectionException)
				{
					TopMenuController.Log.WarnFormat(format, new object[]
					{
						GameHubBehaviour.Hub.User.UniversalId,
						exception
					});
				}
				else
				{
					TopMenuController.Log.ErrorFormat(format, new object[]
					{
						GameHubBehaviour.Hub.User.UniversalId,
						exception
					});
				}
				this._teamGuiComponents.NoTeamGameObject.SetActive(true);
				this._teamGuiComponents.TeamGameObject.SetActive(false);
				this._teamGuiComponents.GlowFullGameObject.SetActive(false);
				this._teamGuiComponents.GlowIconGameObject.SetActive(false);
				this._teamGuiComponents.NameLabel.gameObject.SetActive(false);
			});
		}

		private void TryToChangeConnectionFeedback(bool isVisible)
		{
			if (GameHubBehaviour.Hub.State.Current is MainMenu)
			{
				if (GameHubBehaviour.Hub.State.IsLoadingState)
				{
					TopMenuController.Log.WarnFormat("Trying to update connection feedback before MainMenu finished loading.\n{0}", new object[]
					{
						StackTraceUtility.ExtractStackTrace()
					});
				}
				else
				{
					GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>().ConnectionFeedback.ChangeVisibilityConnectionFeedback(isVisible);
				}
				return;
			}
		}

		public void TeamsButtonOnClick()
		{
			OpenUrlUtils.OpenTeamsUrl(GameHubBehaviour.Hub, OpenUrlUtils.HardcodedWidth, (int)((float)Screen.height * 0.9f));
			this._teamGuiComponents.GlowIconGameObject.SetActive(false);
			this.ResetTeamTimer();
			if (this._glowTeamId != Guid.Empty)
			{
				GameHubBehaviour.Hub.ClientApi.team.ChangesVisualizedByUser(null, this._glowTeamId, GameHubBehaviour.Hub.User.UniversalId, delegate(object s)
				{
					this._glowTeamId = Guid.Empty;
				}, delegate(object s, Exception exception)
				{
					TopMenuController.Log.Error(string.Format("Error on ChangesVisualizedByUser. TeamId:[{0}], UniversalId:[{1}]. Exception:{2}", this._glowTeamId, GameHubBehaviour.Hub.User.UniversalId, exception));
					this._glowTeamId = Guid.Empty;
				});
			}
		}

		private void TeamsUpdate()
		{
			this._teamsLastUpdate -= Time.deltaTime;
			if (this._teamsLastUpdate > 0f)
			{
				return;
			}
			this.ResetTeamTimer();
			this.UpdateTeamsInfo();
		}

		private void ResetTeamTimer()
		{
			this._teamsLastUpdate = (float)GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.TeamsMainMenuTimeoutInSeconds);
		}

		public bool CloseGame()
		{
			return false;
		}

		public string TimeOutMessage()
		{
			return Language.Get("FAILED_TO_CONNECT", "GUI");
		}

		public void TimeOut()
		{
			TopMenuController.Log.Warn("Team Time Out");
			this.TryToChangeConnectionFeedback(true);
		}

		public void PlayHardCoinsUpdateAnimation()
		{
			this.HardCoinsUpdateAnimation.Play();
		}

		protected static readonly BitLogger Log = new BitLogger(typeof(TopMenuController));

		public HMMUIPlayTween TopPlayTween;

		[Header("PORTRAIT")]
		[SerializeField]
		private GameObject _portraitGroupGameObject;

		[SerializeField]
		private GameObject _portraitGameObject;

		public SteamIconLoader PortraitControl;

		public HMMUI2DDynamicSprite PortraitFounderSprite;

		public UILabel PlayerName;

		public GUIEventListener PortraitEventListener;

		[Header("CURRENCY")]
		public UILabel softCoins;

		public UILabel hardCoins;

		public HMMTooltipTrigger softCoinsTooltip;

		public HMMTooltipTrigger hardCoinsTooltip;

		public UI2DSprite HardCoinsSprite;

		public Animation HardCoinsUpdateAnimation;

		[SerializeField]
		private UIButton _hardCoinsButton;

		[SerializeField]
		private GameObject _hardCoinsButtonDisabledGameObject;

		[Header("PROGRESSION")]
		public UILabel UserLevelLabel;

		public UIProgressBar UserLevelProgressBar;

		public HMMTooltipTrigger XpTooltip;

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		[Header("Buff bar ( afk penalty, boosters )")]
		[SerializeField]
		private Color _disabledBoosterColor;

		[SerializeField]
		private Color _enabledeBoosterColor;

		public GameObject BuffBarPivot;

		public GameObject AFK_Group;

		[SerializeField]
		private HMMUI2DDynamicSprite _boosterFounderPackImage;

		[SerializeField]
		private UI2DSprite _boosterFameImage;

		[SerializeField]
		private UI2DSprite _boosterXpImage;

		[SerializeField]
		private HMMTooltipTrigger _boosterFounderPackDisabledTooltip;

		[SerializeField]
		private HMMTooltipTrigger _boosterFameDisabledTooltip;

		[SerializeField]
		private HMMTooltipTrigger _boosterXpDisabledTooltip;

		[SerializeField]
		private PenaltyTooltipTrigger BuffBoosterXpTooltipTrigger;

		[SerializeField]
		private PenaltyTooltipTrigger BuffBoosterFameTooltipTrigger;

		[SerializeField]
		private PenaltyTooltipTrigger BuffFounderPackTooltipTrigger;

		[Header("GROUP")]
		[SerializeField]
		private UI2DSprite _selfGroupLeader2DSprite;

		[SerializeField]
		private GameObject _mainGroupGameObject;

		[SerializeField]
		private GameObject _leaveGroupButtonGameObject;

		[SerializeField]
		private GroupMemberGuiItem[] _sortedGroupMemberGuiItems;

		private Guid _currentGroupID = Guid.Empty;

		[SerializeField]
		private GroupInviteContextMenu _groupInviteMenu;

		[SerializeField]
		private GameObject[] _inviteToGroupButtons;

		[SerializeField]
		private GameObject[] _inviteToGroupDisabledButton;

		[SerializeField]
		private HMMTooltipTrigger[] _inviteDisabledButtonTooltip;

		private const int _leaveButtonLockIntervalInSec = 3;

		private IGroupMemberRemovalLocker _groupMemberRemovalLocker;

		public float BossterUpdateTimeSec = 60f;

		private float _boosterLastUpdate;

		private TimeSpan _remainingFameBoosterTime;

		private TimeSpan _remainingXpBoosterTime;

		private float _teamsLastUpdate;

		[Header("[Team]")]
		[SerializeField]
		private TopMenuController.TeamGuiComponents _teamGuiComponents;

		private Guid _glowTeamId;

		private bool _intializedEvents;

		private GameState currentChangedstate;

		[SerializeField]
		private SocialModalGUI _parentUI;

		[Serializable]
		private struct TeamGuiComponents
		{
			public GameObject MainGroupGameObject;

			public GameObject GlowFullGameObject;

			public GameObject GlowIconGameObject;

			public GameObject TeamGameObject;

			public UILabel NameLabel;

			public HMMUI2DDynamicSprite IconSprite;

			public GameObject NoTeamGameObject;
		}

		public delegate void OnCashButtonClicked();
	}
}
