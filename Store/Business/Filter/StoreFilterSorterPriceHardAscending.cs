using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Store.Business.GetStoreItem;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterSorterPriceHardAscending : IStoreFilterSorter
	{
		public StoreFilterSorterPriceHardAscending(IGetStoreItem getStoreItem, IStoreFilterSorter tiebreakerSorter)
		{
			this._getStoreItem = getStoreItem;
			this._tiebreakerSorter = tiebreakerSorter;
		}

		public void Sort(List<IItemType> itemTypes)
		{
			itemTypes.Sort(new Comparison<IItemType>(this.SortElements));
		}

		public int SortElements(IItemType x, IItemType y)
		{
			long num = long.MaxValue;
			long value = long.MaxValue;
			StoreItem storeItem = this._getStoreItem.Get(x.Id);
			if (storeItem.IsHardPurchasable)
			{
				num = storeItem.HardPrice;
			}
			StoreItem storeItem2 = this._getStoreItem.Get(y.Id);
			if (storeItem2.IsHardPurchasable)
			{
				value = this._getStoreItem.Get(y.Id).HardPrice;
			}
			int num2 = num.CompareTo(value);
			return (num2 != 0) ? num2 : this._tiebreakerSorter.SortElements(x, y);
		}

		private readonly IGetStoreItem _getStoreItem;

		private readonly IStoreFilterSorter _tiebreakerSorter;
	}
}
