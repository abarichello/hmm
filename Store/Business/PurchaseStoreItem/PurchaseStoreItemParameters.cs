using System;
using HeavyMetalMachines.Store.Business.PlayerInventory;
using HeavyMetalMachines.Store.Business.RefreshStoreItemStorage;
using HeavyMetalMachines.Store.Infrastructure;

namespace HeavyMetalMachines.Store.Business.PurchaseStoreItem
{
	public class PurchaseStoreItemParameters
	{
		public ISwordfishPurchaseService SwordfishPurchaseService { get; set; }

		public ISwordfishInventoryService SwordfishInventoryService { get; set; }

		public IPlayerInventory PlayerInventory { get; set; }

		public IRefreshStoreItemStorage RefreshStoreItemStorage { get; set; }
	}
}
