using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Store.Business.StoreItemStorage;
using HeavyMetalMachines.Store.Infrastructure;
using UniRx;

namespace HeavyMetalMachines.Store.Business.RefreshStoreItemStorage
{
	public class RefreshStoreItemStorage : IRefreshStoreItemStorage
	{
		public RefreshStoreItemStorage(IStoreItemStorage storeItemStorage, ISwordfishStoreItemProvider swordfishStoreItemProvider)
		{
			if (storeItemStorage == null)
			{
				throw new ArgumentNullException("storeItemStorage");
			}
			if (swordfishStoreItemProvider == null)
			{
				throw new ArgumentNullException("swordfishStoreItemProvider");
			}
			this._storeItemStorage = storeItemStorage;
			this._swordfishStoreItemProvider = swordfishStoreItemProvider;
		}

		public IObservable<Unit> RefreshAllItems()
		{
			IObservable<IEnumerable<StoreItem>> observable = Observable.Catch<IEnumerable<StoreItem>, Exception>(this._swordfishStoreItemProvider.GetAllStoreItems(), delegate(Exception exception)
			{
				throw new UnavailableProviderException(exception);
			});
			if (RefreshStoreItemStorage.<>f__mg$cache0 == null)
			{
				RefreshStoreItemStorage.<>f__mg$cache0 = new Func<IEnumerable<StoreItem>, IEnumerable<StoreItem>>(RefreshStoreItemStorage.ConvertItems);
			}
			return Observable.Select<IEnumerable<StoreItem>, Unit>(Observable.Do<IEnumerable<StoreItem>>(Observable.Select<IEnumerable<StoreItem>, IEnumerable<StoreItem>>(observable, RefreshStoreItemStorage.<>f__mg$cache0), delegate(IEnumerable<StoreItem> storeItems)
			{
				this._storeItemStorage.RefreshAllItems(storeItems);
			}), (IEnumerable<StoreItem> result) => Unit.Default);
		}

		private static IEnumerable<StoreItem> ConvertItems(IEnumerable<StoreItem> swordfishStoreItems)
		{
			List<StoreItem> list = new List<StoreItem>();
			if (swordfishStoreItems == null)
			{
				return list;
			}
			List<StoreItem> list2 = swordfishStoreItems.ToList<StoreItem>();
			if (!list2.Any<StoreItem>())
			{
				return list;
			}
			for (int i = 0; i < list2.Count; i++)
			{
				StoreItem storeItem = list2[i];
				list.Add(new StoreItem
				{
					Id = storeItem.Id,
					HardPrice = storeItem.HardPrice,
					SoftPrice = storeItem.SoftPrice,
					IsPurchasable = storeItem.IsPurchasable,
					IsHardPurchasable = storeItem.IsHardPurchasable,
					IsSoftPurchasable = storeItem.IsSoftPurchasable
				});
			}
			return list;
		}

		private readonly IStoreItemStorage _storeItemStorage;

		private readonly ISwordfishStoreItemProvider _swordfishStoreItemProvider;

		[CompilerGenerated]
		private static Func<IEnumerable<StoreItem>, IEnumerable<StoreItem>> <>f__mg$cache0;
	}
}
