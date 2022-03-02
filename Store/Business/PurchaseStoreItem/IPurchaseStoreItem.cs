using System;
using UniRx;

namespace HeavyMetalMachines.Store.Business.PurchaseStoreItem
{
	public interface IPurchaseStoreItem
	{
		IObservable<PurchaseResult> PurchaseWithSoftCurrency(SoftCurrencyPurchase purchase);

		IObservable<PurchaseResult> PurchaseWithHardCurrency(HardCurrencyPurchase purchase);
	}
}
