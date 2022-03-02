using System;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem;
using UniRx;

namespace HeavyMetalMachines.Store.Infrastructure
{
	public interface ISwordfishPurchaseService
	{
		IObservable<SwordfishPurchaseResult> PurchaseStoreItemWithSoftCurrency(SoftCurrencyPurchase purchase);

		IObservable<SwordfishPurchaseResult> PurchaseStoreItemWithHardCurrency(HardCurrencyPurchase purchase);
	}
}
