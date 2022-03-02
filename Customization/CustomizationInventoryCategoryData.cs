using System;
using System.Collections.Generic;
using HeavyMetalMachines.Customization.Business;
using HeavyMetalMachines.DataTransferObjects.Battlepass;

namespace HeavyMetalMachines.Customization
{
	[Serializable]
	public class CustomizationInventoryCategoryData
	{
		public CustomizationInventoryCategoryData(Guid categoryId, string categoryName, List<PlayerCustomizationSlot> customizationSlots, IGetCustomizationSlot customizationSlotSelector)
		{
			this.Items = new List<CustomizationInventoryCellItemData>(10);
			this.ItemsDictionary = new Dictionary<Guid, CustomizationInventoryCellItemData>(10);
			this.SortedCharacterIds = new List<Guid>(10);
			this.CharacterIdToItemsDictionary = new Dictionary<Guid, List<CustomizationInventoryCellItemData>>(10);
			this.CategoryId = categoryId;
			this.CategoryName = categoryName;
			this.NewItemsCount = 0;
			this.CustomizationSlots = customizationSlots;
			this._customizationSlotSelector = customizationSlotSelector;
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
		}

		public void AddCharacterSkinDataItems(Guid characterId, List<CustomizationInventoryCellItemData> items)
		{
			this.SortedCharacterIds.Add(characterId);
			this.CharacterIdToItemsDictionary.Add(characterId, items);
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

		public PlayerCustomizationSlot GetEquippingSlot()
		{
			return this._customizationSlotSelector.GetEquippingSlot();
		}

		public PlayerCustomizationSlot GetUnequippingSlot(Guid itemTypeId)
		{
			return this._customizationSlotSelector.GetUnequippingSlot(itemTypeId);
		}

		public Guid CategoryId;

		public List<PlayerCustomizationSlot> CustomizationSlots;

		public bool IsLore;

		public string CategoryName;

		public int NewItemsCount;

		public List<CustomizationInventoryCellItemData> Items;

		public Dictionary<Guid, CustomizationInventoryCellItemData> ItemsDictionary;

		public List<Guid> SortedCharacterIds;

		public Dictionary<Guid, List<CustomizationInventoryCellItemData>> CharacterIdToItemsDictionary;

		private bool _isCurrentSortAscending;

		private IGetCustomizationSlot _customizationSlotSelector;

		public enum SortKind
		{
			None,
			AcquisitionDate
		}
	}
}
