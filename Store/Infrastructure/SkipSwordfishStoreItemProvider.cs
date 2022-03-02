using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Store.Business;
using UniRx;

namespace HeavyMetalMachines.Store.Infrastructure
{
	public class SkipSwordfishStoreItemProvider : ISwordfishStoreItemProvider
	{
		public SkipSwordfishStoreItemProvider(CollectionScriptableObject collection)
		{
			this._collection = collection;
		}

		public IObservable<IEnumerable<StoreItem>> GetAllStoreItems()
		{
			List<StoreItem> list = new List<StoreItem>();
			foreach (KeyValuePair<Guid, ItemTypeScriptableObject> keyValuePair in this._collection.AllItemTypes)
			{
				list.Add(new StoreItem
				{
					Id = keyValuePair.Key,
					HardPrice = 99L,
					SoftPrice = 999L,
					IsPurchasable = true,
					IsHardPurchasable = true,
					IsSoftPurchasable = true
				});
			}
			return Observable.Return<IEnumerable<StoreItem>>(list);
		}

		private readonly CollectionScriptableObject _collection;
	}
}
