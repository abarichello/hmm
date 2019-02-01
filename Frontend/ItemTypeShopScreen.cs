using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ItemTypeShopScreen : ShopScreen
	{
		public override void Setup()
		{
			base.Setup();
			if (this._storeItems == null)
			{
				List<ItemTypeScriptableObject> list = new List<ItemTypeScriptableObject>();
				List<ItemTypeScriptableObject> itemTypeList = this._customizationAssets.ItemTypeList;
				for (int i = 0; i < itemTypeList.Count; i++)
				{
					ItemTypeScriptableObject itemTypeScriptableObject = itemTypeList[i];
					for (int j = 0; j < this._itemCategories.Length; j++)
					{
						if (itemTypeScriptableObject.IsItemEnableInShop() && itemTypeScriptableObject.ItemCategoryId == this._itemCategories[j].Id)
						{
							list.Add(itemTypeScriptableObject);
							break;
						}
					}
				}
				if (list.Count > 0)
				{
					ObjectPoolUtils.CreateObjectPool<StoreItem>(this._referenceItem, out this._storeItems, list.Count);
					for (int k = 0; k < this._storeItems.Length; k++)
					{
						this.SetupItem(this._storeItems[k], list[k]);
					}
				}
			}
			else
			{
				for (int l = 0; l < this._storeItems.Length; l++)
				{
					StoreItem storeItem = this._storeItems[l];
					this.UpdateBoughtStatus(storeItem, storeItem.StoreItemType);
				}
			}
			this.ConfigurePages();
		}

		protected virtual void ConfigurePages()
		{
			if (this._storeItems.Length > 0)
			{
				Array.Sort<StoreItem>(this._storeItems, new Comparison<StoreItem>(this.Comparison));
				int pTotalNumberOfPages = (this._itemsPerPage - 1 + this._storeItems.Length) / this._itemsPerPage;
				base.SetupPageToggleControllers(pTotalNumberOfPages);
				this.ShowPage(base.CurrentPage);
				this._selectedItem = -1;
				this._uiGrid.Reposition();
			}
		}

		private int Comparison(StoreItem thisStoreItem, StoreItem otherStoreItem)
		{
			return thisStoreItem.StoreItemType.name.CompareTo(otherStoreItem.StoreItemType.name);
		}

		protected virtual void SetupItem(StoreItem storeItem, ItemTypeScriptableObject itemType)
		{
			storeItem.StoreItemType = itemType;
			ShopItemTypeComponent component = itemType.GetComponent<ShopItemTypeComponent>();
			if (component == null)
			{
				return;
			}
			storeItem.carTexture.SpriteName = component.IconAssetName;
			storeItem.itemName.text = Language.Get(component.TitleDraft, TranslationSheets.Items);
			if (storeItem.categoryLabel != null)
			{
				ItemCategoryScriptableObject categoryById = GameHubBehaviour.Hub.InventoryColletion.GetCategoryById(itemType.ItemCategoryId);
				storeItem.categoryLabel.text = categoryById.LocalizedName;
			}
			if (storeItem.descriptionLabel != null)
			{
				storeItem.descriptionLabel.text = Language.Get(component.DescriptionDraft, TranslationSheets.Items);
			}
			if (storeItem.icon != null)
			{
				storeItem.icon.SpriteName = itemType.GetItemCategory().IconName;
			}
			if (storeItem.characterName != null)
			{
				storeItem.characterName.text = Language.Get(itemType.GetItemCategory().TitleDraft, TranslationSheets.Inventory);
			}
			this.UpdateBoughtStatus(storeItem, itemType);
		}

		protected virtual void UpdateBoughtStatus(StoreItem storeItem, ItemTypeScriptableObject itemType)
		{
			if (storeItem.boughtGO == null || storeItem.unboughtGO == null)
			{
				ItemTypeShopScreen.SetItemPrices(storeItem, itemType);
				return;
			}
			bool flag = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(itemType.Id);
			storeItem.boughtGO.SetActive(flag);
			storeItem.unboughtGO.SetActive(!flag);
			if (flag)
			{
				return;
			}
			ItemTypeShopScreen.SetItemPrices(storeItem, itemType);
		}

		protected static void SetItemPrices(StoreItem storeItem, ItemTypeScriptableObject itemType)
		{
			int num;
			int num2;
			GameHubBehaviour.Hub.Store.GetItemPrice(itemType.Id, out num, out num2, false);
			storeItem.softPrice.text = num.ToString("0");
			storeItem.SoftPriceGroup.SetActive(itemType.IsActive && itemType.IsSoftPurchasable);
			storeItem.hardPrice.text = num2.ToString("0");
			storeItem.HardPriceGroup.SetActive(itemType.IsActive && itemType.IsHardPurchasable);
		}

		public virtual void OnStoreItemClick(StoreItem item)
		{
			this.Hide();
			this.Shop.ShowItemTypeDetails(item.StoreItemType);
		}

		protected void ShowPage(int page)
		{
			base.CurrentPage = page;
			int num = this._itemsPerPage * base.CurrentPage;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < this._storeItems.Length; i++)
			{
				bool flag = this.IsFiltered(i);
				bool flag2 = false;
				if (flag)
				{
					flag2 = (num3 >= num && num2 < this._itemsPerPage);
					num3++;
				}
				this._storeItems[i].gameObject.SetActive(flag2);
				if (flag2)
				{
					num2++;
				}
			}
			this._uiGrid.Reposition();
			int pTotalNumberOfPages = Mathf.CeilToInt((float)num3 / (float)this._itemsPerPage);
			base.UpdateTotalNumberofPages(pTotalNumberOfPages);
			base.UpdatePageButtonControllers();
		}

		protected virtual bool IsFiltered(int index)
		{
			return true;
		}

		public void GoToPreviousPage()
		{
			if (base.CurrentPage == 0)
			{
				return;
			}
			this.ShowPage(base.CurrentPage - 1);
		}

		public void GoToNextPage()
		{
			if (base.CurrentPage == base.TotalNumberofPages - 1)
			{
				return;
			}
			this.ShowPage(base.CurrentPage + 1);
		}

		[SerializeField]
		private CollectionScriptableObject _customizationAssets;

		[SerializeField]
		private ItemCategoryScriptableObject[] _itemCategories;

		[SerializeField]
		private UIGrid _uiGrid;

		[SerializeField]
		private int _itemsPerPage;

		[SerializeField]
		private StoreItem _referenceItem;

		protected StoreItem[] _storeItems;

		protected int _selectedItem;
	}
}
