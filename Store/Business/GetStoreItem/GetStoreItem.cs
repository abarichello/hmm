using System;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions;
using HeavyMetalMachines.Store.Business.StoreItemStorage;

namespace HeavyMetalMachines.Store.Business.GetStoreItem
{
	public class GetStoreItem : IGetStoreItem
	{
		public GetStoreItem(IStoreItemStorage storeItemStorage)
		{
			if (storeItemStorage == null)
			{
				throw new ArgumentNullException("storeItemStorage");
			}
			this._storeItemStorage = storeItemStorage;
		}

		public StoreItem Get(Guid storeItemId)
		{
			if (storeItemId == Guid.Empty)
			{
				throw new ArgumentException("Store item ID cannot be empty.", "storeItemId");
			}
			StoreItem storeItem = this._storeItemStorage.GetStoreItem(storeItemId);
			if (storeItem == null)
			{
				throw new StoreItemNotFoundException(storeItemId);
			}
			return storeItem;
		}

		private readonly IStoreItemStorage _storeItemStorage;
	}
}
