using System;

namespace HeavyMetalMachines.Store.Business.PurchaseStoreItem
{
	public class SoftCurrencyPurchase
	{
		public Guid StoreItemId { get; set; }

		public long SeenUnitPrice { get; set; }

		public long InventoryId { get; set; }
	}
}
