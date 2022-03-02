using System;

namespace HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions
{
	public class StoreItemDeactivatedException : Exception
	{
		public StoreItemDeactivatedException(Guid itemId) : base(string.Format("The store item with ID {0} was deactivated.", itemId))
		{
			this.ItemId = itemId;
		}

		public Guid ItemId { get; private set; }
	}
}
