using System;
using System.Collections.Generic;
using HeavyMetalMachines.DataTransferObjects.Store;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Store.Infrastructure
{
	public class SwordfishStoreItemProvider : ISwordfishStoreItemProvider
	{
		public IObservable<IEnumerable<StoreItem>> GetAllStoreItems()
		{
			return Observable.Create<IEnumerable<StoreItem>>(delegate(IObserver<IEnumerable<StoreItem>> observer)
			{
				StoreCustomWS.GetAllStoreItems(SwordfishStore.GetStoreId(), delegate(object _, string result)
				{
					observer.OnNext(SwordfishStoreItemProvider.ConvertResultToStoreItems(result));
					observer.OnCompleted();
				}, delegate(object _, Exception exception)
				{
					observer.OnError(exception);
				});
				return Disposable.Empty;
			});
		}

		private static IEnumerable<StoreItem> ConvertResultToStoreItems(string result)
		{
			StoreItemCollection storeItemCollection = (StoreItemCollection)((JsonSerializeable<!0>)result);
			StoreItem[] array = new StoreItem[storeItemCollection.StoreItems.Length];
			for (int i = 0; i < storeItemCollection.StoreItems.Length; i++)
			{
				StoreItem storeItem = storeItemCollection.StoreItems[i];
				array[i] = new StoreItem
				{
					Id = storeItem.Id,
					HardPrice = storeItem.HardPrice,
					SoftPrice = storeItem.SoftPrice,
					IsPurchasable = storeItem.IsPurchasable,
					IsHardPurchasable = storeItem.IsHardPurchasable,
					IsSoftPurchasable = storeItem.IsSoftPurchasable
				};
			}
			return array;
		}
	}
}
