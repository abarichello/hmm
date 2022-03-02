using System;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Store.Business.PlayerInventory;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions;
using HeavyMetalMachines.Store.Business.RefreshStoreItemStorage;
using HeavyMetalMachines.Store.Infrastructure;
using UniRx;

namespace HeavyMetalMachines.Store.Business.PurchaseStoreItem
{
	public class PurchaseStoreItem : IPurchaseStoreItem
	{
		public PurchaseStoreItem(PurchaseStoreItemParameters parameters)
		{
			if (parameters.SwordfishPurchaseService == null)
			{
				throw new ArgumentNullException("SwordfishPurchaseService");
			}
			if (parameters.SwordfishInventoryService == null)
			{
				throw new ArgumentNullException("SwordfishInventoryService");
			}
			if (parameters.PlayerInventory == null)
			{
				throw new ArgumentNullException("PlayerInventory");
			}
			if (parameters.RefreshStoreItemStorage == null)
			{
				throw new ArgumentNullException("RefreshStoreItemStorage");
			}
			this._swordfishPurchaseService = parameters.SwordfishPurchaseService;
			this._swordfishInventoryService = parameters.SwordfishInventoryService;
			this._playerInventory = parameters.PlayerInventory;
			this._refreshStoreItemStorage = parameters.RefreshStoreItemStorage;
		}

		public IObservable<PurchaseResult> PurchaseWithSoftCurrency(SoftCurrencyPurchase purchase)
		{
			PurchaseStoreItem.AssertNotEmptyStoreItemId(purchase.StoreItemId);
			PurchaseStoreItem.AssertValidPurchaseValue(purchase.SeenUnitPrice);
			return Observable.Select<SwordfishItem, PurchaseResult>(Observable.ContinueWith<SwordfishPurchaseResult, SwordfishItem>(Observable.Catch<SwordfishPurchaseResult, Exception>(this._swordfishPurchaseService.PurchaseStoreItemWithSoftCurrency(purchase), new Func<Exception, IObservable<SwordfishPurchaseResult>>(this.OnSwordfishPurchaseError)), new Func<SwordfishPurchaseResult, IObservable<SwordfishItem>>(this.FetchPurchasedItemOnSwordfish)), new Func<SwordfishItem, PurchaseResult>(this.CreatePurchaseResult));
		}

		public IObservable<PurchaseResult> PurchaseWithHardCurrency(HardCurrencyPurchase purchase)
		{
			PurchaseStoreItem.AssertNotEmptyStoreItemId(purchase.StoreItemId);
			PurchaseStoreItem.AssertValidPurchaseValue(purchase.SeenUnitPrice);
			PurchaseStoreItem.AssertValidPurchaseQuantity(purchase.Quantity);
			return Observable.Select<SwordfishItem, PurchaseResult>(Observable.ContinueWith<SwordfishPurchaseResult, SwordfishItem>(Observable.Catch<SwordfishPurchaseResult, Exception>(this._swordfishPurchaseService.PurchaseStoreItemWithHardCurrency(purchase), new Func<Exception, IObservable<SwordfishPurchaseResult>>(this.OnSwordfishPurchaseError)), new Func<SwordfishPurchaseResult, IObservable<SwordfishItem>>(this.FetchPurchasedItemOnSwordfish)), new Func<SwordfishItem, PurchaseResult>(this.CreatePurchaseResult));
		}

		private static void AssertNotEmptyStoreItemId(Guid storeItemId)
		{
			if (storeItemId == Guid.Empty)
			{
				throw new ArgumentException("Store item ID cannot be empty.", "storeItemId");
			}
		}

		private static void AssertValidPurchaseValue(long unitPrice)
		{
			if (unitPrice <= 0L)
			{
				throw new ArgumentException("Unit price cannot be negative or zero.", "unitPrice");
			}
		}

		private static void AssertValidPurchaseQuantity(long quantity)
		{
			if (quantity <= 0L)
			{
				throw new ArgumentException("Quantity cannot be negative or zero.", "quantity");
			}
		}

		private IObservable<SwordfishPurchaseResult> OnSwordfishPurchaseError(Exception exception)
		{
			if (exception is StoreItemPriceChangedException || exception is StoreItemDeactivatedException)
			{
				return Observable.ContinueWith<Unit, SwordfishPurchaseResult>(this._refreshStoreItemStorage.RefreshAllItems(), Observable.Throw<SwordfishPurchaseResult>(exception));
			}
			return Observable.Throw<SwordfishPurchaseResult>(new UndeterminedPurchaseResultException(exception));
		}

		private IObservable<SwordfishItem> FetchPurchasedItemOnSwordfish(SwordfishPurchaseResult swordfishPurchaseResult)
		{
			if (swordfishPurchaseResult == null)
			{
				throw new UndeterminedPurchaseResultException();
			}
			IObservable<SwordfishItem> item = this._swordfishInventoryService.GetItem(swordfishPurchaseResult.ItemInstanceId);
			if (PurchaseStoreItem.<>f__mg$cache0 == null)
			{
				PurchaseStoreItem.<>f__mg$cache0 = new Action<Exception>(PurchaseStoreItem.OnFetchSwordfishItemError);
			}
			return Observable.DoOnError<SwordfishItem>(item, PurchaseStoreItem.<>f__mg$cache0);
		}

		private PurchaseResult CreatePurchaseResult(SwordfishItem swordfishItem)
		{
			Item item = PurchaseStoreItem.CreatePurchasedItem(swordfishItem);
			PurchaseResult result = new PurchaseResult
			{
				PurchasedItem = item
			};
			this._playerInventory.AddItem(item);
			return result;
		}

		private static void OnFetchSwordfishItemError(Exception exception)
		{
			if (exception is ItemNotFoundException)
			{
				throw exception;
			}
			throw new UndeterminedPurchaseResultException(exception);
		}

		private static Item CreatePurchasedItem(SwordfishItem swordfishItem)
		{
			return new Item
			{
				Id = swordfishItem.Id,
				Bag = swordfishItem.Bag,
				Quantity = swordfishItem.Quantity,
				ItemTypeId = swordfishItem.ItemTypeId,
				BagVersion = swordfishItem.BagVersion,
				InventoryId = swordfishItem.InventoryId
			};
		}

		private readonly ISwordfishPurchaseService _swordfishPurchaseService;

		private readonly ISwordfishInventoryService _swordfishInventoryService;

		private readonly IPlayerInventory _playerInventory;

		private readonly IRefreshStoreItemStorage _refreshStoreItemStorage;

		[CompilerGenerated]
		private static Action<Exception> <>f__mg$cache0;
	}
}
