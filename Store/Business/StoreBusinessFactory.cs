using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Battlepass.Business;
using HeavyMetalMachines.Inventory.Business;
using HeavyMetalMachines.Localization.Business;
using HeavyMetalMachines.Store.Business.Filter;
using HeavyMetalMachines.Store.Business.GetStoreItem;
using HeavyMetalMachines.Store.Business.ObserveStoreItem;
using HeavyMetalMachines.Store.Business.PlayerInventory;
using HeavyMetalMachines.Store.Business.PurchaseStoreItem;
using HeavyMetalMachines.Store.Business.RefreshStoreItemStorage;
using HeavyMetalMachines.Store.Business.StoreItemStorage;
using HeavyMetalMachines.Store.Infrastructure;
using HeavyMetalMachines.Store.Presenter;
using HeavyMetalMachines.Store.View;

namespace HeavyMetalMachines.Store.Business
{
	public class StoreBusinessFactory : IStoreBusinessFactory
	{
		public StoreBusinessFactory(IStoreItemStorage storeItemStorage, IPlayerInventory playerInventory, ISwordfishPurchaseService swordfishPurchaseService, ISwordfishInventoryService swordfishInventoryService, ISwordfishStoreItemProvider swordfishStoreItemProvider, IGetLocalizedCategoryName getLocalizedCategoryName, IGetCategoryIconName getCategoryIconName, IGetCharacterItemTypeFromSkinItemTypeId getCharacterItemTypeFromSkinItemTypeId, ICollectionScriptableObject collectionScriptableObject)
		{
			this._storeItemStorage = storeItemStorage;
			this._playerInventory = playerInventory;
			this._swordfishPurchaseService = swordfishPurchaseService;
			this._swordfishInventoryService = swordfishInventoryService;
			this._swordfishStoreItemProvider = swordfishStoreItemProvider;
			this._getCategoryIconName = getCategoryIconName;
			this._getCharacterItemTypeFromSkinItemTypeId = getCharacterItemTypeFromSkinItemTypeId;
			this._collectionScriptableObject = collectionScriptableObject;
			this._getLocalizedCategoryName = getLocalizedCategoryName;
		}

		public IGetStoreItem CreateGetStoreItem()
		{
			return new GetStoreItem(this._storeItemStorage);
		}

		public IStoreItemPresenter CreateGetStoreItemPresenter(IStoreItemView view)
		{
			return new StoreItemPresenter(this, view, this._getLocalizedCategoryName, this._getCategoryIconName, this._getCharacterItemTypeFromSkinItemTypeId);
		}

		public IObserveStoreItem CreateObserveStoreItem()
		{
			return new ObserveStoreItem(this._storeItemStorage);
		}

		public IPurchaseStoreItem CreatePurchaseStoreItem()
		{
			PurchaseStoreItemParameters parameters = new PurchaseStoreItemParameters
			{
				SwordfishPurchaseService = this._swordfishPurchaseService,
				SwordfishInventoryService = this._swordfishInventoryService,
				PlayerInventory = this._playerInventory,
				RefreshStoreItemStorage = this.CreateRefreshStoreItemStorage()
			};
			return new PurchaseStoreItem(parameters);
		}

		public IGetLocalUserHasItem CreateGetLocalUserHasItem()
		{
			return new GetLocalUserHasItemWrapper();
		}

		public IStoreFilterSorterMapper GetStoreFilterMapper(IGetStoreItem getStoreItem)
		{
			return new StoreFilterSorterMapper(getStoreItem, this._getLocalizedCategoryName, this._getCharacterItemTypeFromSkinItemTypeId, this._collectionScriptableObject);
		}

		public IRefreshStoreItemStorage CreateRefreshStoreItemStorage()
		{
			return new RefreshStoreItemStorage(this._storeItemStorage, this._swordfishStoreItemProvider);
		}

		private readonly IStoreItemStorage _storeItemStorage;

		private readonly IPlayerInventory _playerInventory;

		private readonly ISwordfishPurchaseService _swordfishPurchaseService;

		private readonly ISwordfishInventoryService _swordfishInventoryService;

		private readonly ISwordfishStoreItemProvider _swordfishStoreItemProvider;

		private readonly IGetLocalizedCategoryName _getLocalizedCategoryName;

		private readonly IGetCategoryIconName _getCategoryIconName;

		private readonly IGetCharacterItemTypeFromSkinItemTypeId _getCharacterItemTypeFromSkinItemTypeId;

		private readonly ICollectionScriptableObject _collectionScriptableObject;
	}
}
