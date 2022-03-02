using System;

namespace HeavyMetalMachines.Store.Business.Filter
{
	public enum StoreFilterType
	{
		ShopTitleAscending,
		ShopTitleDescending,
		PriceHardDescendingAndShopTitle,
		PriceSoftDescendingAndShopTitle,
		PriceHardAscendingAndShopTitle,
		PriceSoftAscendingAndShopTitle,
		CategoryNameAscending,
		CategoryNameDescending,
		SkinIdol,
		SkinRockstar,
		SkinMetalLegend,
		SkinHeavyMetal,
		InventoryTitleAscending,
		InventoryTitleDescending,
		PriceHardDescendingAndInventoryTitle,
		PriceSoftDescendingAndInventoryTitle,
		SkinRoleSupport,
		SkinRoleTransporter,
		SkinRoleInterceptor,
		VfxKill,
		VfxRespawn,
		VfxScore,
		VfxTakeOff,
		CharacterRoleSupport,
		CharacterRoleTransporter,
		CharacterRoleInterceptor,
		PriceHardAscendingAndInventoryTitle,
		PriceSoftAscendingAndInventoryTitle
	}
}
