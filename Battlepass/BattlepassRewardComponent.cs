using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.Standard_Assets.Scripts.HMM.Battlepass;
using ClientAPI;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Utils;
using Hoplon.Serialization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	public class BattlepassRewardComponent : GameHubScriptableObject, IBattlepassRewardComponent
	{
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

		public void SetStoreBusinessFactory(IStoreBusinessFactory storeBusinessFactory)
		{
			this._storeBusinessFactory = storeBusinessFactory;
			this._exchangeableItemChecker = new ExchangeableItemChecker(GameHubScriptableObject.Hub.User.Inventory, GameHubScriptableObject.Hub.SharedConfigs.Battlepass.SoftCoinAlreadyClaimPercentage, GameHubScriptableObject.Hub.InventoryColletion, this._hardCoinItemTypeScriptableObject.Id, this._softCoinItemTypeScriptableObject.Id, storeBusinessFactory);
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
			if (rewardKind == null || string.IsNullOrEmpty(text))
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
			case 1:
				amount = int.Parse(args);
				return new Guid?(this._softCoinItemTypeScriptableObject.Id);
			case 2:
				return new Guid?(new Guid(args));
			case 4:
				amount = int.Parse(args);
				return new Guid?(this._hardCoinItemTypeScriptableObject.Id);
			}
			Debug.Assert(false, string.Format("BattlepassComponent: Invalid PremiumRewards Kind: {0}", kind), Debug.TargetTeam.All);
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

		public bool HasRewardToClaim()
		{
			return this._battlepassProgressScriptableObject.Progress.HasRewardToClaim(GameHubScriptableObject.Hub.SharedConfigs.Battlepass);
		}

		public void HideRewardWindow()
		{
			if (this._battlepassRewardView == null || !this._battlepassRewardView.IsVisible())
			{
				return;
			}
			this._battlepassRewardView.SetVisibility(false);
			this._battlepassRewardView = null;
			BattlepassRewardComponent.Log.Debug(string.Format("HideRewardWindow: _rewardsClaimed: {0}", this._rewardsClaimed));
			if (this._rewardsClaimed)
			{
				this._rewardsClaimed = false;
				this._battlepassComponent.NotifyBattlepassTransactionSuccess();
			}
		}

		public void OnRewardWindowDispose()
		{
			this._battlepassRewardView = null;
		}

		public void ClaimReward(int levelToClaim, bool isPremiumClaim)
		{
			if (GameHubScriptableObject.Hub == null)
			{
				return;
			}
			BattlepassCustomWS.ClaimRewards(null, GameHubScriptableObject.Hub.User.PlayerSF.Id, levelToClaim, isPremiumClaim, new SwordfishClientApi.ParameterizedCallback<string>(this.OnClaimRewardSuccess), new SwordfishClientApi.ErrorCallback(this.OnClaimRewardError));
		}

		public void ClaimAllRewards(List<ClaimRewardInfo> rewardsInfo, Action onClaimAllRewardsFinished)
		{
			if (GameHubScriptableObject.Hub == null)
			{
				return;
			}
			BattlepassCustomWS.ClaimAllRewards(onClaimAllRewardsFinished, GameHubScriptableObject.Hub.User.PlayerSF.Id, rewardsInfo, new SwordfishClientApi.ParameterizedCallback<string>(this.OnClaimAllRewardsSuccess), new SwordfishClientApi.ErrorCallback(this.OnClaimRewardError));
		}

		private void OnClaimRewardError(object state, Exception exception)
		{
			BattlepassRewardComponent.Log.Error(string.Format("Clain Reward Error {0}", exception));
			this.ShowClaimItemErrorFeedback();
			if (state == null)
			{
				return;
			}
			Action action = (Action)state;
			action();
		}

		private void OnClaimRewardSuccess(object state, string s)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)s);
			if (!netResult.Success)
			{
				this.ShowClaimItemErrorFeedback();
				BattlepassRewardComponent.Log.Debug("Claim Reward Failed");
				return;
			}
			ClaimRewardsSuccess claimRewardsSuccess = (ClaimRewardsSuccess)((JsonSerializeable<!0>)netResult.Msg);
			for (int i = 0; i < claimRewardsSuccess.ItemAddResults.Count; i++)
			{
				GameHubScriptableObject.Hub.User.Inventory.AddSingleItem(claimRewardsSuccess.ItemAddResults[i]);
			}
			this._rewardsClaimed = true;
			BattlepassRewardComponent.Log.Debug("Claim Reward Sucess");
		}

		private void ShowClaimItemErrorFeedback()
		{
			Guid confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = confirmWindowGuid,
				QuestionText = Language.Get("UNKNOWN_CLAIM_ERROR", TranslationContext.Battlepass),
				EnableItemErrorGameObject = true,
				OkButtonText = Language.Get("Ok", TranslationContext.GUI),
				OnOk = delegate()
				{
					GameHubScriptableObject.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(confirmWindowGuid);
				}
			};
			GameHubScriptableObject.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void OnClaimAllRewardsSuccess(object state, string s)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)s);
			Action action = (Action)state;
			ClaimRewardsSuccess claimRewardsSuccess = (ClaimRewardsSuccess)((JsonSerializeable<!0>)netResult.Msg);
			for (int i = 0; i < claimRewardsSuccess.ItemAddResults.Count; i++)
			{
				GameHubScriptableObject.Hub.User.Inventory.AddSingleItem(claimRewardsSuccess.ItemAddResults[i]);
			}
			this._rewardsClaimed = true;
			action();
			if (!netResult.Success)
			{
				this.ShowClaimItemErrorFeedback();
				BattlepassRewardComponent.Log.Debug("Claim Reward Failed");
				return;
			}
			BattlepassRewardComponent.Log.Debug("Claim Reward Sucess");
		}

		public void MetalpassBuyLevelRequest(int quantityFromCurrentLevel, Action onBuyLevel, Action onBuyWindowClosed)
		{
			this._onMetalpassBuyLevel = onBuyLevel;
			if (GameHubScriptableObject.Hub != null)
			{
				GameHubScriptableObject.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(this._accountXpItemType, new Action(this.OnMetalpassBuyLevel), onBuyWindowClosed, new Action(this.OnGoToShopCash), quantityFromCurrentLevel, true);
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

		private Action _onMetalpassBuyLevel;

		private IStoreBusinessFactory _storeBusinessFactory;

		private ExchangeableItemChecker _exchangeableItemChecker;

		private bool _rewardsClaimed;
	}
}
