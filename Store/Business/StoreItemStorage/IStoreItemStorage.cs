using System;
using System.Collections.Generic;

namespace HeavyMetalMachines.Store.Business.StoreItemStorage
{
	public interface IStoreItemStorage
	{
		event Action<StoreItem> StoreItemChanged;

		void RefreshAllItems(IEnumerable<StoreItem> items);

		StoreItem GetStoreItem(Guid storeItemId);
	}
}
