using System;
using HeavyMetalMachines.Store.Business.StoreItemStorage;
using UniRx;

namespace HeavyMetalMachines.Store.Business.ObserveStoreItem
{
	public class ObserveStoreItem : IObserveStoreItem
	{
		public ObserveStoreItem(IStoreItemStorage storeItemStorage)
		{
			if (storeItemStorage == null)
			{
				throw new ArgumentNullException("storeItemStorage");
			}
			this._storeItemStorage = storeItemStorage;
		}

		public IObservable<StoreItem> CreateObservable(Guid storeItemId)
		{
			if (storeItemId == Guid.Empty)
			{
				throw new ArgumentException("Item store ID cannot be empty.", "storeItemId");
			}
			return Observable.Where<StoreItem>(Observable.FromEvent<StoreItem>(delegate(Action<StoreItem> handler)
			{
				this._storeItemStorage.StoreItemChanged += handler;
			}, delegate(Action<StoreItem> handler)
			{
				this._storeItemStorage.StoreItemChanged -= handler;
			}), (StoreItem storeItem) => storeItem.Id == storeItemId);
		}

		private readonly IStoreItemStorage _storeItemStorage;
	}
}
