using System;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public interface IStoreFilterSorterMapper
	{
		IStoreFilterSorter Resolve(StoreFilterType storeFilterType);
	}
}
