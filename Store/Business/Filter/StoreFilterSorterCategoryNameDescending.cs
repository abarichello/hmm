using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Localization.Business;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterSorterCategoryNameDescending : IStoreFilterSorter
	{
		public StoreFilterSorterCategoryNameDescending(IGetLocalizedCategoryName getLocalizedCategoryName, IStoreFilterSorter tiebreakerSorter)
		{
			this._getLocalizedCategoryName = getLocalizedCategoryName;
			this._tiebreakerSorter = tiebreakerSorter;
		}

		public void Sort(List<IItemType> itemTypes)
		{
			itemTypes.Sort(new Comparison<IItemType>(this.SortElements));
		}

		public int SortElements(IItemType x, IItemType y)
		{
			string strB = this._getLocalizedCategoryName.Get(x.ItemCategoryId);
			string strA = this._getLocalizedCategoryName.Get(y.ItemCategoryId);
			int num = string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
			return (num != 0) ? num : this._tiebreakerSorter.SortElements(x, y);
		}

		private readonly IGetLocalizedCategoryName _getLocalizedCategoryName;

		private readonly IStoreFilterSorter _tiebreakerSorter;
	}
}
