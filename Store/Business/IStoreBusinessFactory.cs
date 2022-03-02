using System;
using HeavyMetalMachines.Store.Business.Filter;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Store.Business.ObserveStoreItem;
using HeavyMetalMachines.Store.Business.PlayerInventory;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem;
using HeavyMetalMachines.Store.Business.RefreshStoreItemStorage;
using HeavyMetalMachines.Store.Presenter;
using HeavyMetalMachines.Store.View;

namespace HeavyMetalMachines.Store.Business
{
	public interface IStoreBusinessFactory
	{
		IRefreshStoreItemStorage CreateRefreshStoreItemStorage();

		IGetStoreItem CreateGetStoreItem();

		IStoreItemPresenter CreateGetStoreItemPresenter(IStoreItemView view);

		IObserveStoreItem CreateObserveStoreItem();

		IPurchaseStoreItem CreatePurchaseStoreItem();

		IGetLocalUserHasItem CreateGetLocalUserHasItem();

		IStoreFilterSorterMapper GetStoreFilterMapper(IGetStoreItem getStoreItem);
	}
}
