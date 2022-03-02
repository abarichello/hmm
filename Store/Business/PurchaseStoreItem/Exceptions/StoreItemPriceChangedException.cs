using System;

namespace HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions
{
	public class StoreItemPriceChangedException : Exception
	{
		public StoreItemPriceChangedException(Guid itemId) : base(string.Format("The price of the store item with ID {0} was changed.", itemId))
		{
			this.ItemId = itemId;
		}

		public Guid ItemId { get; private set; }
	}
}
