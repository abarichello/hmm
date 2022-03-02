using System;

namespace HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions
{
	public class StoreItemNotFoundException : Exception
	{
		public StoreItemNotFoundException(Guid storeItemId) : base(string.Format("An store item with ID {0} could not be found.", storeItemId))
		{
			this.StoreItemId = storeItemId;
		}

		public StoreItemNotFoundException(Guid storeItemId, Exception innerException) : base(string.Format("An store item with ID {0} could not be found. Check the inner exception for details.", storeItemId), innerException)
		{
			this.StoreItemId = storeItemId;
		}

		public Guid StoreItemId { get; private set; }
	}
}
