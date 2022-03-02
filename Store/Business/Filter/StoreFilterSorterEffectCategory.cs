using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Battlepass;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterSorterEffectCategory : IStoreFilterSorter
	{
		public StoreFilterSorterEffectCategory(PlayerCustomizationSlot customizationSlot, IStoreFilterSorter tiebreakerSorter, ICollectionScriptableObject collectionScriptableObject)
		{
			this._customizationSlot = customizationSlot;
			this._tiebreakerSorter = tiebreakerSorter;
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public void Sort(List<IItemType> itemTypes)
		{
			List<IItemType> list = new List<IItemType>(itemTypes.Count);
			foreach (IItemType itemType in itemTypes)
			{
				if (!this.IsValidCustomizationSlot(itemType.ItemCategoryId))
				{
					list.Add(itemType);
				}
			}
			foreach (IItemType item in list)
			{
				itemTypes.Remove(item);
			}
			itemTypes.Sort(new Comparison<IItemType>(this.SortElements));
		}

		private bool IsValidCustomizationSlot(Guid itemCategoryId)
		{
			ItemCategoryScriptableObject categoryById = this._collectionScriptableObject.GetCategoryById(itemCategoryId);
			return categoryById.CustomizationSlots.Contains(this._customizationSlot);
		}

		public int SortElements(IItemType x, IItemType y)
		{
			return this._tiebreakerSorter.SortElements(x, y);
		}

		private readonly PlayerCustomizationSlot _customizationSlot;

		private readonly IStoreFilterSorter _tiebreakerSorter;

		private readonly ICollectionScriptableObject _collectionScriptableObject;
	}
}
