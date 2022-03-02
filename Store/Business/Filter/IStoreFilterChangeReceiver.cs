using System;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public interface IStoreFilterChangeReceiver
	{
		StoreFilterType GetCurrentFilter();

		void OnFilterChange(StoreFilterType filterType);
	}
}
