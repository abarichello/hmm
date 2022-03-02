using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Inventory.Business;
using HeavyMetalMachines.Localization.Business;
using HeavyMetalMachines.Store.Business.GetStoreItem;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public class StoreFilterSorterMapper : IStoreFilterSorterMapper
	{
		public StoreFilterSorterMapper(IGetStoreItem getStoreItem, IGetLocalizedCategoryName getLocalizedCategoryName, IGetCharacterItemTypeFromSkinItemTypeId getCharacterItemTypeFromSkinItemTypeId, ICollectionScriptableObject collectionScriptableObject)
		{
			this._sorters = new Dictionary<StoreFilterType, IStoreFilterSorter>();
			StoreFilterSorterShopTitleAscending storeFilterSorterShopTitleAscending = new StoreFilterSorterShopTitleAscending();
			this._sorters.Add(StoreFilterType.ShopTitleAscending, storeFilterSorterShopTitleAscending);
			this._sorters.Add(StoreFilterType.ShopTitleDescending, new StoreFilterSorterShopTitleDescending());
			this._sorters.Add(StoreFilterType.PriceHardDescendingAndShopTitle, new StoreFilterSorterPriceHardDescending(getStoreItem, storeFilterSorterShopTitleAscending));
			this._sorters.Add(StoreFilterType.PriceSoftDescendingAndShopTitle, new StoreFilterSorterPriceSoftDescending(getStoreItem, storeFilterSorterShopTitleAscending));
			this._sorters.Add(StoreFilterType.PriceHardAscendingAndShopTitle, new StoreFilterSorterPriceHardAscending(getStoreItem, storeFilterSorterShopTitleAscending));
			this._sorters.Add(StoreFilterType.PriceSoftAscendingAndShopTitle, new StoreFilterSorterPriceSoftAscending(getStoreItem, storeFilterSorterShopTitleAscending));
			StoreFilterSorterCategoryNameAscending storeFilterSorterCategoryNameAscending = new StoreFilterSorterCategoryNameAscending(getLocalizedCategoryName, storeFilterSorterShopTitleAscending);
			this._sorters.Add(StoreFilterType.CategoryNameAscending, storeFilterSorterCategoryNameAscending);
			this._sorters.Add(StoreFilterType.CategoryNameDescending, new StoreFilterSorterCategoryNameDescending(getLocalizedCategoryName, storeFilterSorterShopTitleAscending));
			StoreFilterSorterInventoryTitleAscending storeFilterSorterInventoryTitleAscending = new StoreFilterSorterInventoryTitleAscending();
			this._sorters.Add(StoreFilterType.InventoryTitleAscending, storeFilterSorterInventoryTitleAscending);
			this._sorters.Add(StoreFilterType.InventoryTitleDescending, new StoreFilterSorterInventoryTitleDescending());
			this._sorters.Add(StoreFilterType.PriceHardAscendingAndInventoryTitle, new StoreFilterSorterPriceHardAscending(getStoreItem, storeFilterSorterInventoryTitleAscending));
			this._sorters.Add(StoreFilterType.PriceHardDescendingAndInventoryTitle, new StoreFilterSorterPriceHardDescending(getStoreItem, storeFilterSorterInventoryTitleAscending));
			this._sorters.Add(StoreFilterType.PriceSoftAscendingAndInventoryTitle, new StoreFilterSorterPriceSoftAscending(getStoreItem, storeFilterSorterInventoryTitleAscending));
			this._sorters.Add(StoreFilterType.PriceSoftDescendingAndInventoryTitle, new StoreFilterSorterPriceSoftDescending(getStoreItem, storeFilterSorterInventoryTitleAscending));
			this.AddCharacterSorters(storeFilterSorterShopTitleAscending);
			this.AddSkinSorters(storeFilterSorterInventoryTitleAscending, getCharacterItemTypeFromSkinItemTypeId);
			this.AddVfxSorters(storeFilterSorterCategoryNameAscending, collectionScriptableObject);
		}

		private void AddCharacterSorters(StoreFilterSorterShopTitleAscending filterSorterShopTitleAscending)
		{
			this._sorters.Add(StoreFilterType.CharacterRoleSupport, new StoreFilterSorterCharacterRoleKind(0, filterSorterShopTitleAscending));
			this._sorters.Add(StoreFilterType.CharacterRoleInterceptor, new StoreFilterSorterCharacterRoleKind(2, filterSorterShopTitleAscending));
			this._sorters.Add(StoreFilterType.CharacterRoleTransporter, new StoreFilterSorterCharacterRoleKind(1, filterSorterShopTitleAscending));
		}

		private void AddSkinSorters(StoreFilterSorterInventoryTitleAscending filterSorterInventoryTitleAscending, IGetCharacterItemTypeFromSkinItemTypeId getCharacterItemTypeFromSkinItemTypeId)
		{
			this._sorters.Add(StoreFilterType.SkinIdol, new StoreFilterSorterSkinTierKind(2, filterSorterInventoryTitleAscending));
			this._sorters.Add(StoreFilterType.SkinRockstar, new StoreFilterSorterSkinTierKind(3, filterSorterInventoryTitleAscending));
			this._sorters.Add(StoreFilterType.SkinMetalLegend, new StoreFilterSorterSkinTierKind(4, filterSorterInventoryTitleAscending));
			this._sorters.Add(StoreFilterType.SkinHeavyMetal, new StoreFilterSorterSkinTierKind(5, filterSorterInventoryTitleAscending));
			this._sorters.Add(StoreFilterType.SkinRoleSupport, new StoreFilterSorterSkinRoleKind(0, filterSorterInventoryTitleAscending, getCharacterItemTypeFromSkinItemTypeId));
			this._sorters.Add(StoreFilterType.SkinRoleInterceptor, new StoreFilterSorterSkinRoleKind(2, filterSorterInventoryTitleAscending, getCharacterItemTypeFromSkinItemTypeId));
			this._sorters.Add(StoreFilterType.SkinRoleTransporter, new StoreFilterSorterSkinRoleKind(1, filterSorterInventoryTitleAscending, getCharacterItemTypeFromSkinItemTypeId));
		}

		private void AddVfxSorters(StoreFilterSorterCategoryNameAscending storeFilterSorterCategoryNameAscending, ICollectionScriptableObject collectionScriptableObject)
		{
			this._sorters.Add(StoreFilterType.VfxKill, new StoreFilterSorterEffectCategory(4, storeFilterSorterCategoryNameAscending, collectionScriptableObject));
			this._sorters.Add(StoreFilterType.VfxRespawn, new StoreFilterSorterEffectCategory(5, storeFilterSorterCategoryNameAscending, collectionScriptableObject));
			this._sorters.Add(StoreFilterType.VfxScore, new StoreFilterSorterEffectCategory(3, storeFilterSorterCategoryNameAscending, collectionScriptableObject));
			this._sorters.Add(StoreFilterType.VfxTakeOff, new StoreFilterSorterEffectCategory(2, storeFilterSorterCategoryNameAscending, collectionScriptableObject));
		}

		public IStoreFilterSorter Resolve(StoreFilterType storeFilterType)
		{
			return this._sorters[storeFilterType];
		}

		private readonly Dictionary<StoreFilterType, IStoreFilterSorter> _sorters;
	}
}
