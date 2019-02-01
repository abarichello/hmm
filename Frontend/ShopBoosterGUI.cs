using System;
using Assets.ClientApiObjects;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Obsolete("This class will be migrated to ItemTypeShopScreen")]
	public class ShopBoosterGUI : ShopScreen
	{
		private void Awake()
		{
			UIGrid boostersGrid = this.BoostersGrid;
			boostersGrid.onCustomSort = (Comparison<Transform>)Delegate.Combine(boostersGrid.onCustomSort, new Comparison<Transform>(this.OnCustomSort));
		}

		private void OnDestroy()
		{
			UIGrid boostersGrid = this.BoostersGrid;
			boostersGrid.onCustomSort = (Comparison<Transform>)Delegate.Remove(boostersGrid.onCustomSort, new Comparison<Transform>(this.OnCustomSort));
		}

		public override void Setup()
		{
			base.Setup();
			StoreItem boosterStoreItemReference = this.BoosterStoreItemReference;
			int childCount = this.BoostersGrid.transform.childCount;
			int i = 0;
			int num = 0;
			int count = this.Boosters.BoostItems.Count;
			while (num < count && i < this.MaxVisibleBoosterItems)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = this.Boosters.BoostItems[num];
				if (itemTypeScriptableObject.IsActive)
				{
					StoreItem storeItem;
					if (i < childCount)
					{
						Transform child = this.BoostersGrid.transform.GetChild(i);
						child.gameObject.SetActive(true);
						storeItem = child.GetComponent<StoreItem>();
					}
					else
					{
						storeItem = UnityEngine.Object.Instantiate<StoreItem>(boosterStoreItemReference, Vector3.zero, Quaternion.identity);
						storeItem.transform.parent = boosterStoreItemReference.transform.parent;
						storeItem.transform.localScale = boosterStoreItemReference.transform.localScale;
					}
					storeItem.StoreItemType = itemTypeScriptableObject;
					this.SetupBooster(storeItem);
					storeItem.gameObject.GetComponent<BoxCollider>().enabled = true;
					i++;
				}
				num++;
			}
			while (i < childCount)
			{
				this.BoostersGrid.GetChild(i).gameObject.SetActive(false);
				i++;
			}
			this.BoostersGrid.Reposition();
			this.SetActiveBoosterTimeRemaining();
			BoosterConfig boosterConfigs = GameHubBehaviour.Hub.SharedConfigs.BoosterConfigs;
			this.BoosterbaseDescriptionLabel.text = string.Format(Language.Get("BOOSTER_MAIN_DESCRIPTION", TranslationSheets.Store), boosterConfigs.ScrapBounsPercentage, boosterConfigs.XpBounsPercentage);
		}

		private int OnCustomSort(Transform transform1, Transform transform2)
		{
			string name = transform1.GetComponent<StoreItem>().StoreItemType.Name;
			string name2 = transform2.GetComponent<StoreItem>().StoreItemType.Name;
			return string.Compare(name, name2, StringComparison.Ordinal);
		}

		private void SetupBooster(StoreItem storeItem)
		{
			int num;
			int num2;
			GameHubBehaviour.Hub.Store.GetItemPrice(storeItem.StoreItemType.Id, out num, out num2, false);
			bool flag = !storeItem.StoreItemType.Deleted;
			storeItem.softPrice.text = num.ToString("0");
			storeItem.SoftPriceGroup.SetActive(flag && storeItem.StoreItemType.IsSoftPurchasable);
			storeItem.hardPrice.text = num2.ToString("0");
			storeItem.HardPriceGroup.SetActive(flag && storeItem.StoreItemType.IsHardPurchasable);
		}

		public override void Show()
		{
			base.Show();
			this.SetActiveBoosterTimeRemaining();
		}

		[Obsolete]
		public void BuyBooster(GameObject boosterGameObject)
		{
			StoreItem component = boosterGameObject.GetComponent<StoreItem>();
			ItemTypeScriptableObject storeItemType = component.StoreItemType;
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(storeItemType, new Action(this.OnCompleteBoosterBuy), new Action(this.OnBuyWindowClosed), new Action(this.OnGoToShopCash), 1, true);
		}

		private void OnGoToShopCash()
		{
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.CloseWindow();
			this.Hide();
			this.Shop.AnimateReturnToCash();
		}

		private void OnCompleteBoosterBuy()
		{
			GameHubBehaviour.Hub.User.Inventory.ReloadAllItems(new Action(this.OnReloadAllItemsDone));
		}

		private void OnBuyWindowClosed()
		{
		}

		private void OnReloadAllItemsDone()
		{
			GameHubBehaviour.Hub.GuiScripts.TopMenu.UpdateCurrencyLabels();
			this.SetActiveBoosterTimeRemaining();
		}

		private void SetActiveBoosterTimeRemaining()
		{
			this.BoosterActiveInfoPivot.SetActive(true);
			this.BoosterInactiveInfodescription.gameObject.SetActive(false);
			this.BoosterActiveInfodescription.text = string.Empty;
			this.BoosterActiveInfodescription2.text = string.Empty;
			this.BoosterActiveInfodescription.gameObject.SetActive(true);
			this.BoosterActiveInfodescription2.gameObject.SetActive(true);
			string text;
			if (GameHubBehaviour.Hub.GuiScripts.TopMenu.TryToGetBoosterFameInfo(out text))
			{
				this.BoosterActiveInfodescription.text = text;
			}
			if (GameHubBehaviour.Hub.GuiScripts.TopMenu.TryToGetBoosterXpInfo(out text))
			{
				if (this.BoosterActiveInfodescription.text.Length == 0)
				{
					this.BoosterActiveInfodescription2.gameObject.SetActive(false);
					this.BoosterActiveInfodescription.text = text;
				}
				else
				{
					this.BoosterActiveInfodescription2.text = text;
				}
			}
			if (this.BoosterActiveInfodescription.text.Length == 0)
			{
				this.BoosterActiveInfodescription.gameObject.SetActive(false);
				this.BoosterActiveInfodescription2.gameObject.SetActive(false);
				this.BoosterInactiveInfodescription.gameObject.SetActive(true);
			}
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.GuiScripts.TopMenu.OnUpdateBoosterInfo += this.TopMenuOnUpdateBoosterInfo;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.GuiScripts.TopMenu.OnUpdateBoosterInfo -= this.TopMenuOnUpdateBoosterInfo;
		}

		private void TopMenuOnUpdateBoosterInfo()
		{
			if (!this.IsVisible())
			{
				return;
			}
			this.SetActiveBoosterTimeRemaining();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ShopBoosterGUI));

		[Header("EXTERNAL2")]
		public MainMenuGui MainMenugui;

		[Header("BOOSTERS")]
		public BoostersInfo Boosters;

		public StoreItem BoosterStoreItemReference;

		public GameObject BoosterActiveInfoPivot;

		public UILabel BoosterActiveInfodescription;

		public UILabel BoosterActiveInfodescription2;

		public UILabel BoosterInactiveInfodescription;

		public UIGrid BoostersGrid;

		public int MaxVisibleBoosterItems = 4;

		public UILabel BoosterbaseDescriptionLabel;
	}
}
