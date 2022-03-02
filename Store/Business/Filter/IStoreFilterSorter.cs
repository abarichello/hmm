using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public interface IStoreFilterSorter
	{
		void Sort(List<IItemType> itemTypes);

		int SortElements(IItemType x, IItemType y);
	}
}
