using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.ScriptableObjects;
using Hoplon.Serialization;
using Pocketverse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Battlepass
{
	public class BattlepassPremiumShopComponent : GameHubScriptableObject, IBattlepassPremiumShopComponent
	{
		private void OnMetalpassUnlockPremium(int levelTarget)
		{
			this._battlepassComponent.NotifyBattlepassTransactionSuccess();
			if (this._onMetalpassUnlockPremium != null)
			{
				this._onMetalpassUnlockPremium(levelTarget);
			}
			this._onMetalpassUnlockPremium = null;
		}

		public void ShowPremiumShopWindow(Action<int> onUnlockPremium, Action onBuyWindowClosed)
		{
			if (this.IsPremiumShopSceneLoad())
			{
				BattlepassPremiumShopComponent.Log.Error("Trying to load a premium shop already loading.");
				return;
			}
			this._onMetalpassUnlockPremium = onUnlockPremium;
			this._onMetalpassShopClosed = onBuyWindowClosed;
			SceneManager.LoadSceneAsync("UI_ADD_BattlepassShop", 1);
			this._premiumShopSceneLoad = true;
		}

		public bool IsPremiumShopSceneLoad()
		{
			return this._premiumShopSceneLoad;
		}

		public void OnHidePremiumShopWindowAnimationEnded()
		{
			this._onMetalpassUnlockPremium = null;
			this._onMetalpassShopClosed = null;
			SceneManager.UnloadSceneAsync("UI_ADD_BattlepassShop");
			this._premiumShopSceneLoad = false;
		}

		public void RegisterPremiumShopWindow(IBattlepassPremiumShopView premiumShopView, BattlepassSeason battlepassSeason)
		{
			this._battlepassPremiumShopView = premiumShopView;
			int level = this._battlepassProgressScriptableObject.Progress.Level;
			ItemTypeScriptableObject[] packages;
			if (level >= GameHubScriptableObject.Hub.SharedConfigs.Battlepass.Levels.Length - 1)
			{
				packages = new ItemTypeScriptableObject[]
				{
					this._unlockPremiumBattlepassItemTypes[0]
				};
			}
			else
			{
				packages = new ItemTypeScriptableObject[]
				{
					this._unlockPremiumBattlepassItemTypes[0],
					this._unlockPremiumBattlepassItemTypes[this._battlepassComponent.GetPackageIndexRelevantToBattlepassLevel(level, GameHubScriptableObject.Hub.SharedConfigs.Battlepass.Levels.Length - 1, this._unlockPremiumBattlepassItemTypes.Length)]
				};
			}
			DateTime endSeasonDateTime = battlepassSeason.EndSeasonDateTime;
			TimeSpan t = new TimeSpan(this._battlepassComponent.GetSfClockOffset());
			TimeSpan remainingTime = endSeasonDateTime - (DateTime.UtcNow + t);
			this._battlepassPremiumShopView.Setup(packages, remainingTime);
			this._battlepassPremiumShopView.SetVisibility(true);
		}

		public void HidePremiumShopWindow()
		{
			if (this._battlepassPremiumShopView == null)
			{
				return;
			}
			this._battlepassPremiumShopView.SetVisibility(false);
			if (this._onMetalpassShopClosed != null)
			{
				this._onMetalpassShopClosed();
				this._onMetalpassShopClosed = null;
			}
		}

		public void OnBuyPremiumRequested(int packageIndex)
		{
			this._levelBuyTarget = 0;
			int level = this._battlepassProgressScriptableObject.Progress.Level;
			ItemTypeScriptableObject itemTypeScriptableObject = (packageIndex != 0) ? this._unlockPremiumBattlepassItemTypes[this._battlepassComponent.GetPackageIndexRelevantToBattlepassLevel(level, GameHubScriptableObject.Hub.SharedConfigs.Battlepass.Levels.Length - 1, this._unlockPremiumBattlepassItemTypes.Length)] : this._unlockPremiumBattlepassItemTypes[0];
			this._battlepassPremiumShopView.SetInteractability(false);
			if (itemTypeScriptableObject is PackageItemTypeScriptableObject)
			{
				PackageItemTypeScriptableObject packageItemTypeScriptableObject = (PackageItemTypeScriptableObject)itemTypeScriptableObject;
				PackageItemTypeBag packageItemTypeBag = (PackageItemTypeBag)((JsonSerializeable<!0>)packageItemTypeScriptableObject.Bag);
				for (int i = 0; i < packageItemTypeBag.itens.Length; i++)
				{
					PackageItem packageItem = packageItemTypeBag.itens[i];
					if (this._accountXpItemType.Id == packageItem.Id)
					{
						BattlepassProgress progress = this._battlepassProgressScriptableObject.Progress;
						BattlepassConfig battlepass = GameHubScriptableObject.Hub.SharedConfigs.Battlepass;
						int levelForXp = battlepass.GetLevelForXp(progress.CurrentXp);
						this._levelBuyTarget = packageItem.Amount + levelForXp;
					}
				}
			}
			this._buyPremiumComfirmed = false;
			GameHubScriptableObject.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(itemTypeScriptableObject, new Action(this.OnBuyPremiumConfirmed), new Action(this.OnBuyPremiumClosed), new Action(this.GoToShopCashFromPremiumShop), 1, this._isBuyImagePortrait);
		}

		private void OnBuyPremiumConfirmed()
		{
			this._buyPremiumComfirmed = true;
			this.OnMetalpassUnlockPremium(this._levelBuyTarget);
		}

		private void OnBuyPremiumClosed()
		{
			if (this._buyPremiumComfirmed)
			{
				this._buyPremiumComfirmed = false;
				this.HidePremiumShopWindow();
			}
			else if (this._battlepassPremiumShopView.IsVisible())
			{
				this._battlepassPremiumShopView.SetInteractability(true);
			}
		}

		private void GoToShopCashFromPremiumShop()
		{
			ClientShopBILogger.LegacyLog(GameHubScriptableObject.Hub, 5, 6);
			this.HidePremiumShopWindow();
			this._battlepassComponent.OnGoToShopCash();
		}

		[SerializeField]
		private bool _isBuyImagePortrait = true;

		private static readonly BitLogger Log = new BitLogger(typeof(BattlepassPremiumShopComponent));

		private const string BATTLEPASS_SHOP_SCENE_NAME = "UI_ADD_BattlepassShop";

		[SerializeField]
		private BattlepassComponent _battlepassComponent;

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		[SerializeField]
		private ItemTypeScriptableObject _accountXpItemType;

		[SerializeField]
		private ItemTypeScriptableObject[] _unlockPremiumBattlepassItemTypes;

		private IBattlepassPremiumShopView _battlepassPremiumShopView;

		private Action<int> _onMetalpassUnlockPremium;

		private Action _onMetalpassShopClosed;

		private int _levelBuyTarget;

		private bool _buyPremiumComfirmed;

		private bool _premiumShopSceneLoad;
	}
}
