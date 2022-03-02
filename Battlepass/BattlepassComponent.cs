using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.Standard_Assets.Scripts.HMM.Battlepass;
using ClientAPI;
using HeavyMetalMachines.Battlepass.Business;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Boosters.Business;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Utils;
using Hoplon.Serialization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	public class BattlepassComponent : GameHubScriptableObject, IBattlepassComponent
	{
		public void SetStoreBusinessFactory(IStoreBusinessFactory storeBusinessFactory)
		{
			this._storeBusinessFactory = storeBusinessFactory;
			this._exchangeableItemChecker = new ExchangeableItemChecker(GameHubScriptableObject.Hub.User.Inventory, GameHubScriptableObject.Hub.SharedConfigs.Battlepass.SoftCoinAlreadyClaimPercentage, GameHubScriptableObject.Hub.InventoryColletion, this._hardCoinItemTypeScriptableObject.Id, this._softCoinItemTypeScriptableObject.Id, this._storeBusinessFactory);
		}

		public void SetIGetLocalPlayerXpBooster(IGetLocalPlayerXpBooster playerXpBooster)
		{
			this._getLocalPlayerXpBooster = playerXpBooster;
		}

		public void SetIGetBattlepassSeason(IGetBattlepassSeason getBattlepassSeason)
		{
			this._getBattlepassSeason = getBattlepassSeason;
		}

		public void RegisterView(ILegacyBattlepassView view)
		{
			this._battlepassView = view;
		}

		public void UnregisterView()
		{
			this._battlepassView = null;
		}

		public BattlepassViewData GetBattlepassViewData()
		{
			BattlepassViewData battlepassViewData = new BattlepassViewData();
			battlepassViewData.DataTime = new BattlepassViewData.BattlepassViewDataTime
			{
				OffsetToSf = new TimeSpan(this._sfClockOffset)
			};
			battlepassViewData.DataLevels = default(BattlepassViewData.BattlepassViewDataLevels);
			battlepassViewData.DataSeason = default(BattlepassViewData.BattlepassViewDataSeason);
			if (GameHubScriptableObject.Hub != null)
			{
				BattlepassConfig battlepass = GameHubScriptableObject.Hub.SharedConfigs.Battlepass;
				BattlepassSeason battlepassSeason = this._getBattlepassSeason.Get();
				battlepassViewData.DataTime.StartDateUtc = battlepassSeason.StartSeasonDateTime;
				battlepassViewData.DataTime.EndDateUtc = battlepassSeason.EndSeasonDateTime;
				BattlepassProgress progress = this._battlepassProgressScriptableObject.Progress;
				int currentXp = progress.CurrentXp;
				int levelForXp = battlepass.GetLevelForXp(currentXp);
				ProgressionLevel[] levels = battlepass.Levels;
				int num = levels.Length;
				battlepassViewData.DataLevels.CurrentLevel = levelForXp;
				battlepassViewData.DataLevels.CurrentXp = currentXp - levels[levelForXp].XP;
				battlepassViewData.DataLevels.MaxLevels = num;
				battlepassViewData.DataLevels.MaxXpPerLevel = new int[levels.Length];
				battlepassViewData.DataLevels.HasXpBooster = this._getLocalPlayerXpBooster.IsActive();
				battlepassViewData.DataSeason.UserHasPremium = progress.HasPremium();
				battlepassViewData.DataSeason.ItemTypeId = this._accountXpItemType.Id;
				battlepassViewData.DataSeason.FreeSeasonItems = new List<BattlepassViewData.BattlepassViewDataSlotItem>(num);
				battlepassViewData.DataSeason.PremiumSeasonItems = new List<BattlepassViewData.BattlepassViewDataSlotItem>(num);
				int num2 = 0;
				int i = 0;
				while (i < levels.Length)
				{
					ProgressionLevel progressionLevel = levels[i];
					battlepassViewData.DataLevels.MaxXpPerLevel[i] = progressionLevel.XP - num2;
					num2 = progressionLevel.XP;
					if (progressionLevel.FreeRewards.Argument != null && progressionLevel.FreeRewards.Kind != null)
					{
						int currencyAmount = 0;
						Guid id;
						switch (progressionLevel.FreeRewards.Kind)
						{
						case 1:
							id = this._softCoinItemTypeScriptableObject.Id;
							currencyAmount = ((!string.IsNullOrEmpty(progressionLevel.FreeRewards.Argument)) ? int.Parse(progressionLevel.FreeRewards.Argument) : 0);
							goto IL_2C1;
						case 2:
							id = new Guid(progressionLevel.FreeRewards.Argument);
							goto IL_2C1;
						case 4:
							id = this._hardCoinItemTypeScriptableObject.Id;
							currencyAmount = ((!string.IsNullOrEmpty(progressionLevel.FreeRewards.Argument)) ? int.Parse(progressionLevel.FreeRewards.Argument) : 0);
							goto IL_2C1;
						}
						Debug.Assert(false, "BattlepassComponent: Invalid FreeRewards Kind: " + progressionLevel.FreeRewards.Kind, Debug.TargetTeam.All);
						goto IL_41B;
						IL_2C1:
						this.AddBattlepassViewDataSlotItem(progressionLevel.FreeRewards.Kind, false, id, i, currencyAmount, battlepassViewData.DataSeason.FreeSeasonItems);
						goto IL_2E5;
					}
					goto IL_2E5;
					IL_41B:
					i++;
					continue;
					IL_2E5:
					if (progressionLevel.PremiumRewards.Argument != null && progressionLevel.PremiumRewards.Kind != null)
					{
						int currencyAmount2 = 0;
						Guid id2;
						switch (progressionLevel.PremiumRewards.Kind)
						{
						case 1:
							id2 = this._softCoinItemTypeScriptableObject.Id;
							currencyAmount2 = ((!string.IsNullOrEmpty(progressionLevel.PremiumRewards.Argument)) ? int.Parse(progressionLevel.PremiumRewards.Argument) : 0);
							goto IL_3F7;
						case 2:
							id2 = new Guid(progressionLevel.PremiumRewards.Argument);
							goto IL_3F7;
						case 4:
							id2 = this._hardCoinItemTypeScriptableObject.Id;
							currencyAmount2 = ((!string.IsNullOrEmpty(progressionLevel.PremiumRewards.Argument)) ? int.Parse(progressionLevel.PremiumRewards.Argument) : 0);
							goto IL_3F7;
						}
						Debug.Assert(false, "BattlepassComponent: Invalid PremiumRewards Kind: " + progressionLevel.FreeRewards.Kind, Debug.TargetTeam.All);
						goto IL_41B;
						IL_3F7:
						this.AddBattlepassViewDataSlotItem(progressionLevel.PremiumRewards.Kind, true, id2, i, currencyAmount2, battlepassViewData.DataSeason.PremiumSeasonItems);
						goto IL_41B;
					}
					goto IL_41B;
				}
				battlepassViewData.BattlepassConfig = GameHubScriptableObject.Hub.SharedConfigs.Battlepass;
				battlepassViewData.BattlepassProgress = progress;
			}
			return battlepassViewData;
		}

		private void AddBattlepassViewDataSlotItem(ProgressionInfo.RewardKind rewardKind, bool isPremium, Guid keyGuid, int unlockLevel, int currencyAmount, List<BattlepassViewData.BattlepassViewDataSlotItem> listToAdd)
		{
			ItemTypeScriptableObject itemTypeScriptableObject;
			ItemTypeComponent itemTypeComponent;
			if (GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes.TryGetValue(keyGuid, out itemTypeScriptableObject) && itemTypeScriptableObject.GetComponentByEnum(ItemTypeComponent.Type.Battlepass, out itemTypeComponent))
			{
				BattlepassItemTypeComponent battlepassItemTypeComponent = (BattlepassItemTypeComponent)itemTypeComponent;
				string loreTitleDraft = string.Empty;
				string loreSubtitleDraft = string.Empty;
				string loreDescriptionDraft = string.Empty;
				if (itemTypeScriptableObject.GetComponentByEnum(ItemTypeComponent.Type.Lore, out itemTypeComponent))
				{
					LoreItemTypeComponent loreItemTypeComponent = (LoreItemTypeComponent)itemTypeComponent;
					loreTitleDraft = loreItemTypeComponent._loreTitle;
					loreSubtitleDraft = loreItemTypeComponent._loreSubTitle;
					loreDescriptionDraft = loreItemTypeComponent._loreText;
				}
				int num = 0;
				bool isRepeated = this._exchangeableItemChecker.CheckIfPlayerWillExchangeItemType(itemTypeScriptableObject, ref num);
				string iconAssetName;
				if (isPremium && !string.IsNullOrEmpty(battlepassItemTypeComponent.IconPremiumAssetName))
				{
					iconAssetName = battlepassItemTypeComponent.IconPremiumAssetName;
				}
				else
				{
					iconAssetName = battlepassItemTypeComponent.IconAssetName;
				}
				BattlepassViewData.BattlepassViewDataSlotItem item = new BattlepassViewData.BattlepassViewDataSlotItem
				{
					RewardKind = rewardKind,
					UnlockLevel = unlockLevel,
					ArtAssetName = battlepassItemTypeComponent.ArtAssetName,
					DescriptionDraft = battlepassItemTypeComponent.DescriptionDraft,
					CurrencyAmount = currencyAmount,
					IconAssetName = iconAssetName,
					PreviewKind = battlepassItemTypeComponent.PreviewKind,
					TitleDraft = battlepassItemTypeComponent.TitleDraft,
					LoreTitleDraft = loreTitleDraft,
					LoreSubtitleDraft = loreSubtitleDraft,
					LoreDescriptionDraft = loreDescriptionDraft,
					IsRepeated = isRepeated,
					ArtPreviewBackGroundAssetName = battlepassItemTypeComponent.ArtPreviewBackGroundAssetName
				};
				if (itemTypeScriptableObject.GetComponentByEnum(ItemTypeComponent.Type.SkinPrefab, out itemTypeComponent))
				{
					item.SkinPrefabComponent = (SkinPrefabItemTypeComponent)itemTypeComponent;
				}
				listToAdd.Add(item);
			}
		}

		public void ShowMetalpassPremiumShopWindow()
		{
			bool flag = this._battlepassProgressScriptableObject.Progress.HasPremium();
			if (!this._battlepassView.IsVisible())
			{
				this._battlepassView.Setup(this.GetBattlepassViewData());
			}
			if (!flag)
			{
				this._battlepassView.TryToOpenPremiumShop();
			}
		}

		private void OnRewardWindowClose()
		{
			this._battlepassView.RewardWindowClosed();
		}

		private void HideMetalpassWindow(bool imediate)
		{
			if (this._battlepassView.IsVisible())
			{
				this._battlepassRewardComponent.HideRewardWindow();
				this._battlepassPremiumShopComponent.HidePremiumShopWindow();
				if (this._onWindowCloseAction != null)
				{
					this._onWindowCloseAction();
					this._onWindowCloseAction = null;
				}
				this._battlepassView.SetVisibility(false, false, imediate);
				if (GameHubScriptableObject.Hub != null)
				{
					MainMenuGui stateGuiController = GameHubScriptableObject.Hub.State.Current.GetStateGuiController<MainMenuGui>();
					if (stateGuiController)
					{
						stateGuiController.AnimateReturnToLobby(false, false);
					}
				}
			}
			else if (imediate)
			{
				this._battlepassView.SetVisibility(false, false, true);
			}
		}

		public void NotifyBattlepassTransactionSuccess()
		{
			if (this.OnBattlepassTransactionSuccess != null)
			{
				this.OnBattlepassTransactionSuccess();
			}
		}

		public void MetalpassBuyLevelRequest(int quantityFromCurrentLevel, Action onBuyLevel, Action onBuyWindowClosed)
		{
			this._onMetalpassBuyLevel = onBuyLevel;
			if (GameHubScriptableObject.Hub != null)
			{
				GameHubScriptableObject.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(this._accountXpItemType, new Action(this.OnMetalpassBuyLevel), onBuyWindowClosed, new Action(this.OnGoToShopCashFromBuyLevelRequest), quantityFromCurrentLevel, true);
			}
		}

		private void OnMetalpassBuyLevel()
		{
			this.NotifyBattlepassTransactionSuccess();
			if (this._onMetalpassBuyLevel != null)
			{
				this._onMetalpassBuyLevel();
			}
			this._onMetalpassBuyLevel = null;
		}

		private void OnGoToShopCashFromBuyLevelRequest()
		{
			ClientShopBILogger.LegacyLog(GameHubScriptableObject.Hub, 4, 6);
			this.OnGoToShopCash();
		}

		public void OnGoToShopCash()
		{
			GameHubScriptableObject.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.CloseWindow();
			this.HideMetalpassWindow(false);
			GameHubScriptableObject.Hub.State.Current.GetStateGuiController<MainMenuGui>().OnCashTopButtonClick();
		}

		public void MarkMissionsAsSeen()
		{
			BattlepassCustomWS.MarkMissionsAsSeen(GameHubScriptableObject.Hub.State.Current.StateKind, GameHubScriptableObject.Hub.User.PlayerSF.Id.ToString(), new SwordfishClientApi.ParameterizedCallback<string>(this.OnMarkMissionsSuccess), new SwordfishClientApi.ErrorCallback(this.OnMarkMissionsError));
		}

		public void SetLevelFake(int level)
		{
			ProgressionLevel[] levels = GameHubScriptableObject.Hub.SharedConfigs.Battlepass.Levels;
			int level2 = this._battlepassProgressScriptableObject.Progress.Level;
			int currentXp = this._battlepassProgressScriptableObject.Progress.CurrentXp;
			int num = currentXp - levels[level2].XP;
			int currentXp2 = levels[level].XP + num;
			this._battlepassProgressScriptableObject.Progress.CurrentXp = currentXp2;
		}

		public void GivePremiumFake()
		{
			this._battlepassProgressScriptableObject.Progress.PremiumPurchaseDate = DateTime.UtcNow;
			this._battlepassProgressScriptableObject.Progress.PremiumPurchaseXp = this._battlepassProgressScriptableObject.Progress.CurrentXp;
		}

		private void OnMarkMissionsSuccess(object state, string s)
		{
			GameState.GameStateKind gameStateKind = (GameState.GameStateKind)state;
			if (gameStateKind == GameHubScriptableObject.Hub.State.Current.StateKind)
			{
				NetResult netResult = (NetResult)((JsonSerializeable<!0>)s);
				if (netResult.Success)
				{
					GameHubScriptableObject.Hub.User.SetBattlepassProgress(netResult.Msg);
				}
			}
			else
			{
				BattlepassComponent.Log.DebugFormat("OnMarkMissionsSuccess on another state. Old={0} Current={1}", new object[]
				{
					gameStateKind,
					GameHubScriptableObject.Hub.State.Current.StateKind
				});
			}
		}

		private void OnMarkMissionsError(object state, Exception exception)
		{
			BattlepassComponent.Log.ErrorFormat("OnMarkMissionsError on another state. state={0} Exception={1}", new object[]
			{
				GameHubScriptableObject.Hub.State.Current.StateKind,
				exception
			});
		}

		public int GetPackageIndexRelevantToBattlepassLevel(int level, int maxLevel, int numItemTypes)
		{
			int num = maxLevel - numItemTypes + 1;
			int num2 = Mathf.Max(level, num);
			return Mathf.Clamp(num2 - maxLevel + numItemTypes, 1, numItemTypes - 1);
		}

		public void RefreshData(MainMenuData mainMenuData)
		{
			long num;
			if (long.TryParse(mainMenuData.UtcNowTicksString, out num))
			{
				this._sfClockOffset = num - DateTime.UtcNow.Ticks;
			}
			if (this._battlepassView != null && this._battlepassView.IsVisible())
			{
				this._battlepassView.RefreshData(this.GetBattlepassViewData());
			}
		}

		public long GetSfClockOffset()
		{
			return this._sfClockOffset;
		}

		public bool IsMetalpassWindowVisible()
		{
			return this._battlepassView != null && this._battlepassView.IsVisible();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BattlepassComponent));

		[SerializeField]
		private BattlepassRewardComponent _battlepassRewardComponent;

		[SerializeField]
		private BattlepassPremiumShopComponent _battlepassPremiumShopComponent;

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		[SerializeField]
		private ItemTypeScriptableObject _accountXpItemType;

		[SerializeField]
		private ItemTypeScriptableObject _softCoinItemTypeScriptableObject;

		[SerializeField]
		private ItemTypeScriptableObject _hardCoinItemTypeScriptableObject;

		private ILegacyBattlepassView _battlepassView;

		private Action _onWindowCloseAction;

		private Action _onMetalpassBuyLevel;

		private IStoreBusinessFactory _storeBusinessFactory;

		private ExchangeableItemChecker _exchangeableItemChecker;

		private IGetLocalPlayerXpBooster _getLocalPlayerXpBooster;

		private IGetBattlepassSeason _getBattlepassSeason;

		public Action OnBattlepassTransactionSuccess;

		private long _sfClockOffset;
	}
}
