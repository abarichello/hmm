using System;

namespace HeavyMetalMachines.Store.Business.PurchaseStoreItem.Exceptions
{
	public class ItemNotFoundException : Exception
	{
		public ItemNotFoundException(long itemInstanceId) : base(string.Format("An item with ID {0} could not be found.", itemInstanceId))
		{
			this.ItemInstanceId = itemInstanceId;
		}

		public ItemNotFoundException(long itemInstanceId, Exception innerException) : base(string.Format("An item with ID {0} could not be found. Check the inner exception for details.", itemInstanceId), innerException)
		{
			this.ItemInstanceId = itemInstanceId;
		}

		public long ItemInstanceId { get; private set; }
	}
}
