using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions;

namespace HeavyMetalMachines.Store.Business.StoreItemStorage
{
	public class StoreItemStorage : IStoreItemStorage
	{
		public StoreItemStorage()
		{
			this._storeItems = new Dictionary<Guid, StoreItem>();
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<StoreItem> StoreItemChanged;

		public void RefreshAllItems(IEnumerable<StoreItem> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException("items");
			}
			List<StoreItem> list = items.ToList<StoreItem>();
			for (int i = 0; i < list.Count; i++)
			{
				StoreItem storeItem = list[i];
				this._storeItems[storeItem.Id] = storeItem;
			}
			if (this.StoreItemChanged == null)
			{
				return;
			}
			for (int j = 0; j < list.Count; j++)
			{
				this.StoreItemChanged(list[j]);
			}
		}

		public StoreItem GetStoreItem(Guid storeItemId)
		{
			if (storeItemId == Guid.Empty)
			{
				throw new ArgumentException("Store item ID cannot be empty.", "storeItemId");
			}
			if (!this._storeItems.ContainsKey(storeItemId))
			{
				throw new StoreItemNotFoundException(storeItemId);
			}
			return this._storeItems[storeItemId];
		}

		private readonly Dictionary<Guid, StoreItem> _storeItems;
	}
}
