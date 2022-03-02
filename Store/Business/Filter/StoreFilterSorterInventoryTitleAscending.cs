using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Localization;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterSorterInventoryTitleAscending : IStoreFilterSorter
	{
		public void Sort(List<IItemType> itemTypes)
		{
			itemTypes.Sort(new Comparison<IItemType>(this.SortElements));
		}

		public int SortElements(IItemType x, IItemType y)
		{
			InventoryItemTypeComponent component = x.GetComponent<InventoryItemTypeComponent>();
			InventoryItemTypeComponent component2 = y.GetComponent<InventoryItemTypeComponent>();
			if (component == null || component2 == null)
			{
				return 0;
			}
			string strA = Language.Get(component.TitleDraft, TranslationContext.Items);
			string strB = Language.Get(component2.TitleDraft, TranslationContext.Items);
			return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
		}
	}
}
