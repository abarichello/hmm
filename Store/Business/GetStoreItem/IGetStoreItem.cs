using System;

namespace HeavyMetalMachines.Store.Business.GetStoreItem
{
	public interface IGetStoreItem
	{
		StoreItem Get(Guid storeItemId);
	}
}
