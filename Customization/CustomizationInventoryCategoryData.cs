using System;
using System.Collections.Generic;
using Commons.Swordfish.Battlepass;

namespace HeavyMetalMachines.Customization
{
	[Serializable]
	public class CustomizationInventoryCategoryData
	{
		public CustomizationInventoryCategoryData(Guid categoryId, string categoryName)
		{
			int capacity = 10;
			this.Items = new List<CustomizationInventoryCellItemData>(capacity);
			this.ItemsDictionary = new Dictionary<Guid, CustomizationInventoryCellItemData>(capacity);
			this.CategoryId = categoryId;
			this.CategoryName = categoryName;
			this.NewItemsCount = 0;
			this.SkinTabDataItems = new List<CustomizationInventoryCellItemSkinTabData>(capacity);
			this.SkinTabToCellCountDictionary = new Dictionary<int, int>(capacity);
			this.SkinTabToItemsDictionary = new Dictionary<int, List<CustomizationInventoryCellItemData>>(capacity);
		}

		public void AddItem(CustomizationInventoryCellItemData item)
		{
			if (!this.ItemsDictionary.ContainsKey(item.ItemTypeId))
			{
				this.ItemsDictionary.Add(item.ItemTypeId, item);
			}
			this.Items.Add(item);
			if (item.IsNew)
			{
				this.NewItemsCount++;
			}
			if (item.IsEquipped)
			{
				this.EquipItem(item.ItemTypeId);
			}
		}

		public void AddSkinTabDataItem(CustomizationInventoryCellItemSkinTabData skinTabData, List<CustomizationInventoryCellItemData> items)
		{
			this.SkinTabDataItems.Add(skinTabData);
			int key = this.SkinTabDataItems.Count - 1;
			int num = items.Count / 3;
			if (items.Count % 3 != 0)
			{
				num++;
			}
			this.SkinTabToCellCountDictionary.Add(key, num);
			this.SkinTabToItemsDictionary.Add(key, items);
		}

		public void EquipItem(Guid itemId)
		{
			if (this._currentlySelectedItemId != Guid.Empty && this.ItemsDictionary.ContainsKey(this._currentlySelectedItemId))
			{
				this.ItemsDictionary[this._currentlySelectedItemId].IsEquipped = false;
			}
			this._currentlySelectedItemId = itemId;
			if (this.ItemsDictionary.ContainsKey(this._currentlySelectedItemId))
			{
				this.ItemsDictionary[this._currentlySelectedItemId].IsEquipped = true;
			}
		}

		public void SortItems(CustomizationInventoryCategoryData.SortKind sortKind, bool asc = true)
		{
			this._isCurrentSortAscending = asc;
			if (sortKind == CustomizationInventoryCategoryData.SortKind.AcquisitionDate)
			{
				this.Items.Sort(new Comparison<CustomizationInventoryCellItemData>(this.SortItemsByAcquisitionDate));
			}
		}

		private int SortItemsByAcquisitionDate(CustomizationInventoryCellItemData a, CustomizationInventoryCellItemData b)
		{
			int num = (!(a.DateAcquired > b.DateAcquired)) ? -1 : 1;
			if (!this._isCurrentSortAscending)
			{
				num = -num;
			}
			return num;
		}

		public bool SkinIndexIsTab(int scrollIndex)
		{
			int num;
			int num2;
			return this.SkinIndexIsTab(scrollIndex, out num, out num2);
		}

		public bool SkinIndexIsTab(int scrollIndex, out int tabIndexFound, out int tabIndexCellOffset)
		{
			tabIndexFound = 0;
			tabIndexCellOffset = 0;
			if (scrollIndex == 0)
			{
				return true;
			}
			int num = 0;
			int i = 0;
			while (i < this.SkinTabDataItems.Count)
			{
				num++;
				tabIndexFound = i;
				if (this.SkinTabDataItems[i].IsExpanded)
				{
					num += this.SkinTabToCellCountDictionary[i];
				}
				if (scrollIndex < num)
				{
					tabIndexCellOffset = this.SkinTabToCellCountDictionary[i] - (num - scrollIndex);
					return false;
				}
				i++;
				if (scrollIndex == num)
				{
					tabIndexFound = i;
					return true;
				}
			}
			return false;
		}

		public int GetSkinScrollerCellCount()
		{
			int num = 0;
			for (int i = 0; i < this.SkinTabDataItems.Count; i++)
			{
				CustomizationInventoryCellItemSkinTabData customizationInventoryCellItemSkinTabData = this.SkinTabDataItems[i];
				num++;
				if (customizationInventoryCellItemSkinTabData.IsExpanded)
				{
					num += this.SkinTabToCellCountDictionary[i];
				}
			}
			return num;
		}

		public Guid CategoryId;

		public PlayerCustomizationSlot CustomizationSlot;

		public bool IsLore;

		public string CategoryName;

		public int NewItemsCount;

		public List<CustomizationInventoryCellItemData> Items;

		public List<CustomizationInventoryCellItemSkinTabData> SkinTabDataItems;

		public Dictionary<int, List<CustomizationInventoryCellItemData>> SkinTabToItemsDictionary;

		public Dictionary<int, int> SkinTabToCellCountDictionary;

		public Dictionary<Guid, CustomizationInventoryCellItemData> ItemsDictionary;

		private Guid _currentlySelectedItemId = Guid.Empty;

		private bool _isCurrentSortAscending;

		public enum SortKind
		{
			None,
			AcquisitionDate
		}
	}
}
