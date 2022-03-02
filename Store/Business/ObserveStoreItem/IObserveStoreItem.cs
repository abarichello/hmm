using System;
using UniRx;

namespace HeavyMetalMachines.Store.Business.ObserveStoreItem
{
	public interface IObserveStoreItem
	{
		IObservable<StoreItem> CreateObservable(Guid storeItemId);
	}
}
