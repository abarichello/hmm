using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.Standard_Assets.Scripts.HMM.Battlepass;
using ClientAPI;
using Commons.Swordfish.Battlepass;
using Commons.Swordfish.Progression;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Battlepass
{
	public class BattlepassRewardComponent : GameHubScriptableObject, IBattlepassRewardComponent
	{
		private void Awake()
		{
			this._exchangeableItemChecker = new ExchangeableItemChecker(GameHubScriptableObject.Hub.User.Inventory, GameHubScriptableObject.Hub.SharedConfigs.Battlepass.SoftCoinAlreadyClaimPercentage, GameHubScriptableObject.Hub.InventoryColletion, this._hardCoinItemTypeScriptableObject.Id, this._softCoinItemTypeScriptableObject.Id);
		}

		public UnityUIBattlepassRewardView.DataReward RegisterRewardView(IBattlepassRewardView view)
		{
			this._battlepassRewardView = view;
			UnityUIBattlepassRewardView.DataReward result = default(UnityUIBattlepassRewardView.DataReward);
			if (GameHubScriptableObject.Hub == null)
			{
				return result;
			}
			BattlepassProgress progress = this._battlepassProgressScriptableObject.Progress;
			List<UnityUIBattlepassRewardView.DataPreview> list = new List<UnityUIBattlepassRewardView.DataPreview>();
			this.PopulateDataRewards(progress, list);
			result.Itens = list;
			return result;
		}

		private void PopulateDataRewards(BattlepassProgress progress, List<UnityUIBattlepassRewardView.DataPreview> dataRewardItens)
		{
			BattlepassConfig battlepass = GameHubScriptableObject.Hub.SharedConfigs.Battlepass;
			int levelForXp = battlepass.GetLevelForXp(progress.CurrentXp);
			bool flag = progress.HasPremium();
			for (int i = 0; i <= levelForXp; i++)
			{
				if (!progress.FreeLevelsClaimed[i])
				{
					this.AddBattlepassRewardItens(i, battlepass.Levels[i], (ProgressionLevel level) => level.FreeRewards.Kind, (ProgressionLevel level) => level.FreeRewards.Argument, dataRewardItens, false);
				}
				if (flag && !progress.PremiumLevelsClaimed[i])
				{
					this.AddBattlepassRewardItens(i, battlepass.Levels[i], (ProgressionLevel level) => level.PremiumRewards.Kind, (ProgressionLevel level) => level.PremiumRewards.Argument, dataRewardItens, true);
				}
			}
		}

		private void AddBattlepassRewardItens(int level, ProgressionLevel battlepassReward, Func<ProgressionLevel, ProgressionInfo.RewardKind> GetKind, Func<ProgressionLevel, string> GetArguments, List<UnityUIBattlepassRewardView.DataPreview> dataRewardItens, bool isPremium)
		{
			ProgressionInfo.RewardKind rewardKind = GetKind(battlepassReward);
			string text = GetArguments(battlepassReward);
			if (rewardKind == ProgressionInfo.RewardKind.None || string.IsNullOrEmpty(text))
			{
				return;
			}
			int amount;
			Guid? itemGuidByRewardKind = this.GetItemGuidByRewardKind(rewardKind, text, out amount);
			if (itemGuidByRewardKind == null)
			{
				return;
			}
			this.AddBattlepassRewardItens(itemGuidByRewardKind.Value, level, amount, dataRewardItens, isPremium, rewardKind);
		}

		private Guid? GetItemGuidByRewardKind(ProgressionInfo.RewardKind kind, string args, out int amount)
		{
			amount = 0;
			switch (kind)
			{
			case ProgressionInfo.RewardKind.SoftCurrency:
				amount = int.Parse(args);
				return new Guid?(this._softCoinItemTypeScriptableObject.Id);
			case ProgressionInfo.RewardKind.ItemType:
				return new Guid?(new Guid(args));
			case ProgressionInfo.RewardKind.HardCurrency:
				amount = int.Parse(args);
				return new Guid?(this._hardCoinItemTypeScriptableObject.Id);
			}
			HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("BattlepassComponent: Invalid PremiumRewards Kind: {0}", kind), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			return null;
		}

		private void AddBattlepassRewardItens(Guid keyGuid, int level, int amount, List<UnityUIBattlepassRewardView.DataPreview> listToAdd, bool isPremium, ProgressionInfo.RewardKind kind)
		{
			ItemTypeScriptableObject itemTypeScriptableObject;
			if (!GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes.TryGetValue(keyGuid, out itemTypeScriptableObject))
			{
				return;
			}
			ItemTypeComponent itemTypeComponent;
			if (!itemTypeScriptableObject.GetComponentByEnum(ItemTypeComponent.Type.Battlepass, out itemTypeComponent))
			{
				return;
			}
			BattlepassItemTypeComponent battlepassItemTypeComponent = (BattlepassItemTypeComponent)itemTypeComponent;
			string loreDescription = string.Empty;
			string loreTitle = string.Empty;
			string loreSubtitle = string.Empty;
			if (itemTypeScriptableObject.GetComponentByEnum(ItemTypeComponent.Type.Lore, out itemTypeComponent))
			{
				LoreItemTypeComponent loreItemTypeComponent = (LoreItemTypeComponent)itemTypeComponent;
				loreDescription = loreItemTypeComponent._loreText;
				loreTitle = loreItemTypeComponent._loreTitle;
				loreSubtitle = loreItemTypeComponent._loreSubTitle;
			}
			bool isExchange = this._exchangeableItemChecker.CheckIfPlayerWillExchangeItemType(itemTypeScriptableObject, ref amount);
			UnityUIBattlepassRewardView.DataPreview item = new UnityUIBattlepassRewardView.DataPreview
			{
				Level = level,
				ArtAssetKind = battlepassItemTypeComponent.PreviewKind,
				ArtAssetName = battlepassItemTypeComponent.ArtAssetName,
				Fame = amount,
				TitleText = battlepassItemTypeComponent.TitleDraft,
				DescriptionText = battlepassItemTypeComponent.DescriptionDraft,
				IsPremium = isPremium,
				LoreTitle = loreTitle,
				LoreSubtitle = loreSubtitle,
				LoreDescription = loreDescription,
				IsExchange = isExchange,
				Kind = kind
			};
			listToAdd.Add(item);
		}

		public bool TryToOpenRewardsToClaim(System.Action onWindowCloseAction)
		{
			if (this._battlepassProgressScriptableObject.Progress.HasRewardToClaim(GameHubScriptableObject.Hub.SharedConfigs.Battlepass))
			{
				this.ShowRewardWindow(onWindowCloseAction);
				return true;
			}
			return false;
		}

		public void ShowRewardWindow(System.Action onWindowCloseAction)
		{
			this._rewardsClaimed = false;
			if (this._battlepassRewardView == null)
			{
				SceneManager.LoadSceneAsync("UI_ADD_BattlepassReward", LoadSceneMode.Additive);
				if (onWindowCloseAction != null)
				{
					this._onRewardWindowCloseAction = (System.Action)Delegate.Combine(this._onRewardWindowCloseAction, onWindowCloseAction);
				}
				return;
			}
			if (!this._battlepassComponent.IsMetalpassWindowVisible())
			{
				SceneManager.UnloadSceneAsync("UI_ADD_BattlepassReward");
				this._battlepassRewardView = null;
				this._onRewardWindowCloseAction = null;
				return;
			}
			if (!this._battlepassRewardView.IsVisible())
			{
				if (onWindowCloseAction != null)
				{
					this._onRewardWindowCloseAction = (System.Action)Delegate.Combine(this._onRewardWindowCloseAction, onWindowCloseAction);
				}
				this._battlepassRewardView.SetVisibility(true);
				GameHubScriptableObject.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.TryCloseWindow();
			}
		}

		public void HideRewardWindow()
		{
			if (this._battlepassRewardView == null || !this._battlepassRewardView.IsVisible())
			{
				return;
			}
			this._battlepassRewardView.SetVisibility(false);
			if (this._onRewardWindowCloseAction != null)
			{
				this._onRewardWindowCloseAction();
				this._onRewardWindowCloseAction = null;
			}
			SceneManager.UnloadSceneAsync("UI_ADD_BattlepassReward");
			this._battlepassRewardView = null;
			if (this._rewardsClaimed)
			{
				this._battlepassComponent.NotifyBattlepassTransactionSuccess();
			}
		}

		public void OnRewardWindowDispose()
		{
			this._battlepassRewardView = null;
			this._onRewardWindowCloseAction = null;
		}

		public void ClaimReward(int levelToClaim, bool premiumClaim)
		{
			if (GameHubScriptableObject.Hub == null)
			{
				return;
			}
			BattlepassCustomWS.ClaimRewards(null, GameHubScriptableObject.Hub.User.PlayerSF.Id, levelToClaim, premiumClaim, new SwordfishClientApi.ParameterizedCallback<string>(this.OnClaimRewardSuccess), new SwordfishClientApi.ErrorCallback(this.OnClaimRewardError));
		}

		private void OnClaimRewardError(object state, Exception exception)
		{
			BattlepassRewardComponent.Log.Error(string.Format("Clain Reward Error {0}", exception));
			this.ShowClaimItemErrorFeedback();
		}

		private void OnClaimRewardSuccess(object state, string s)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<T>)s);
			if (!netResult.Success)
			{
				ClaimRewardFail claimRewardFail = (ClaimRewardFail)((JsonSerializeable<T>)netResult.Msg);
				BattlepassRewardComponent.Log.Error(string.Format("Clain Reward Failed. {0}", netResult.Msg));
				if (netResult.Error == -310)
				{
					BattlepassConfig battlepass = GameHubScriptableObject.Hub.SharedConfigs.Battlepass;
					ProgressionReward progressionReward;
					if (claimRewardFail.IsPremium)
					{
						progressionReward = battlepass.Levels[claimRewardFail.ClaimIndex].PremiumRewards;
					}
					else
					{
						progressionReward = battlepass.Levels[claimRewardFail.ClaimIndex].FreeRewards;
					}
					int num;
					Guid? itemGuidByRewardKind = this.GetItemGuidByRewardKind(progressionReward.Kind, progressionReward.Argument, out num);
					ItemTypeScriptableObject itemTypeScriptableObject;
					if (!GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes.TryGetValue(itemGuidByRewardKind.Value, out itemTypeScriptableObject))
					{
						this.ShowClaimItemErrorFeedback();
						return;
					}
					this.ShowClaimItemErrorFeedback();
				}
				else
				{
					this.ShowClaimItemErrorFeedback();
				}
			}
			else
			{
				ClaimRewardsSuccess claimRewardsSuccess = (ClaimRewardsSuccess)((JsonSerializeable<T>)netResult.Msg);
				for (int i = 0; i < claimRewardsSuccess.ItemAddResults.Count; i++)
				{
					GameHubScriptableObject.Hub.User.Inventory.AddSingleItem(claimRewardsSuccess.ItemAddResults[i]);
				}
				this._rewardsClaimed = true;
			}
		}

		private void ShowClaimItemErrorFeedback()
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("UNKNOWN_CLAIM_ERROR", TranslationSheets.Battlepass),
				EnableItemErrorGameObject = true,
				OkButtonText = Language.Get("Ok", TranslationSheets.GUI),
				OnOk = delegate()
				{
					GameHubScriptableObject.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubScriptableObject.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		public void MetalpassBuyLevelRequest(int quantityFromCurrentLevel, System.Action onBuyLevel, System.Action onBuyWindowClosed)
		{
			this._onMetalpassBuyLevel = onBuyLevel;
			if (GameHubScriptableObject.Hub != null)
			{
				GameHubScriptableObject.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(this._accountXpItemType, new System.Action(this.OnMetalpassBuyLevel), onBuyWindowClosed, new System.Action(this.OnGoToShopCash), quantityFromCurrentLevel, true);
			}
		}

		private void OnGoToShopCash()
		{
			this._battlepassComponent.OnGoToShopCash();
		}

		private void OnMetalpassBuyLevel()
		{
			this._battlepassComponent.NotifyBattlepassTransactionSuccess();
			if (this._onMetalpassBuyLevel != null)
			{
				this._onMetalpassBuyLevel();
			}
			this._onMetalpassBuyLevel = null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BattlepassRewardComponent));

		[SerializeField]
		private BattlepassComponent _battlepassComponent;

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		[SerializeField]
		private ItemTypeScriptableObject _accountXpItemType;

		[SerializeField]
		private ItemTypeScriptableObject _softCoinItemTypeScriptableObject;

		[SerializeField]
		private ItemTypeScriptableObject _hardCoinItemTypeScriptableObject;

		private IBattlepassRewardView _battlepassRewardView;

		private System.Action _onRewardWindowCloseAction;

		private System.Action _onMetalpassBuyLevel;

		private ExchangeableItemChecker _exchangeableItemChecker;

		private bool _rewardsClaimed;
	}
}
