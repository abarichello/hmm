using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Localization;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterSorterShopTitleDescending : IStoreFilterSorter
	{
		public void Sort(List<IItemType> itemTypes)
		{
			itemTypes.Sort(new Comparison<IItemType>(this.SortElements));
		}

		public int SortElements(IItemType x, IItemType y)
		{
			ShopItemTypeComponent component = x.GetComponent<ShopItemTypeComponent>();
			ShopItemTypeComponent component2 = y.GetComponent<ShopItemTypeComponent>();
			string strB = Language.Get(component.TitleDraft, TranslationContext.Items);
			string strA = Language.Get(component2.TitleDraft, TranslationContext.Items);
			return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
		}
	}
}
