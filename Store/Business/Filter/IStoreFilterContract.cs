using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public interface IStoreFilterContract
	{
		bool IsOverflow { get; }

		List<StoreFilterType> FilterTypes { get; }

		int GetStoreFilterTypeIndex(StoreFilterType filterType);
	}
}
