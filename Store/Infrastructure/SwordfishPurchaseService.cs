using System;
using ClientAPI.Objects;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions;
using Swordfish.Common.exceptions;
using UniRx;

namespace HeavyMetalMachines.Store.Infrastructure
{
	public class SwordfishPurchaseService : ISwordfishPurchaseService
	{
		public SwordfishPurchaseService(IStore swordfishStore, Player player, Wallet wallet)
		{
			if (swordfishStore == null)
			{
				throw new ArgumentNullException("swordfishStore");
			}
			if (player == null)
			{
				throw new ArgumentNullException("player");
			}
			if (wallet == null)
			{
				throw new ArgumentNullException("wallet");
			}
			this._swordfishStore = swordfishStore;
			this._player = player;
			this._wallet = wallet;
		}

		public IObservable<SwordfishPurchaseResult> PurchaseStoreItemWithSoftCurrency(SoftCurrencyPurchase purchase)
		{
			SwordfishPurchaseService.<PurchaseStoreItemWithSoftCurrency>c__AnonStorey0 <PurchaseStoreItemWithSoftCurrency>c__AnonStorey = new SwordfishPurchaseService.<PurchaseStoreItemWithSoftCurrency>c__AnonStorey0();
			<PurchaseStoreItemWithSoftCurrency>c__AnonStorey.purchase = purchase;
			<PurchaseStoreItemWithSoftCurrency>c__AnonStorey.$this = this;
			return Observable.Create<SwordfishPurchaseResult>(delegate(IObserver<SwordfishPurchaseResult> observer)
			{
				<PurchaseStoreItemWithSoftCurrency>c__AnonStorey.$this._swordfishStore.BuyItemUsingPlayerSoftCoinRealTime(<PurchaseStoreItemWithSoftCurrency>c__AnonStorey.purchase.StoreItemId, <PurchaseStoreItemWithSoftCurrency>c__AnonStorey.$this._player.Id, SwordfishStore.GetStoreId(), <PurchaseStoreItemWithSoftCurrency>c__AnonStorey.purchase.StoreItemId, <PurchaseStoreItemWithSoftCurrency>c__AnonStorey.purchase.InventoryId, <PurchaseStoreItemWithSoftCurrency>c__AnonStorey.$this._wallet.Id, <PurchaseStoreItemWithSoftCurrency>c__AnonStorey.purchase.SeenUnitPrice, delegate(object state, long id)
				{
					observer.OnNext(new SwordfishPurchaseResult
					{
						ItemInstanceId = id
					});
					observer.OnCompleted();
				}, delegate(object state, Exception exception)
				{
					observer.OnError(<PurchaseStoreItemWithSoftCurrency>c__AnonStorey.ConvertException(exception, <PurchaseStoreItemWithSoftCurrency>c__AnonStorey.purchase.StoreItemId));
				});
				return Disposable.Empty;
			});
		}

		public IObservable<SwordfishPurchaseResult> PurchaseStoreItemWithHardCurrency(HardCurrencyPurchase purchase)
		{
			SwordfishPurchaseService.<PurchaseStoreItemWithHardCurrency>c__AnonStorey2 <PurchaseStoreItemWithHardCurrency>c__AnonStorey = new SwordfishPurchaseService.<PurchaseStoreItemWithHardCurrency>c__AnonStorey2();
			<PurchaseStoreItemWithHardCurrency>c__AnonStorey.purchase = purchase;
			<PurchaseStoreItemWithHardCurrency>c__AnonStorey.$this = this;
			return Observable.Create<SwordfishPurchaseResult>(delegate(IObserver<SwordfishPurchaseResult> observer)
			{
				<PurchaseStoreItemWithHardCurrency>c__AnonStorey.$this._swordfishStore.BuyItemTypeUsingHardCurrencyRealTime(<PurchaseStoreItemWithHardCurrency>c__AnonStorey.purchase.StoreItemId, SwordfishStore.GetStoreId(), <PurchaseStoreItemWithHardCurrency>c__AnonStorey.purchase.StoreItemId, <PurchaseStoreItemWithHardCurrency>c__AnonStorey.purchase.InventoryId, (int)<PurchaseStoreItemWithHardCurrency>c__AnonStorey.purchase.Quantity, <PurchaseStoreItemWithHardCurrency>c__AnonStorey.purchase.SeenUnitPrice, delegate(object state, long id)
				{
					observer.OnNext(new SwordfishPurchaseResult
					{
						ItemInstanceId = id
					});
					observer.OnCompleted();
				}, delegate(object state, Exception exception)
				{
					observer.OnError(<PurchaseStoreItemWithHardCurrency>c__AnonStorey.ConvertException(exception, <PurchaseStoreItemWithHardCurrency>c__AnonStorey.purchase.StoreItemId));
				});
				return Disposable.Empty;
			});
		}

		private Exception ConvertException(Exception exception, Guid storeItemId)
		{
			if (exception is ItemTypePriceMismatchException)
			{
				return new StoreItemPriceChangedException(storeItemId);
			}
			if (exception is ItemTypeNotPurchasableException)
			{
				return new StoreItemDeactivatedException(storeItemId);
			}
			return exception;
		}

		private readonly IStore _swordfishStore;

		private readonly Player _player;

		private readonly Wallet _wallet;
	}
}
