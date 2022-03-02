using System;
using System.Collections.Generic;
using HeavyMetalMachines.Store.Business;
using UniRx;

namespace HeavyMetalMachines.Store.Infrastructure
{
	public interface ISwordfishStoreItemProvider
	{
		IObservable<IEnumerable<StoreItem>> GetAllStoreItems();
	}
}
