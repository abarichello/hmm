using System;

namespace HeavyMetalMachines.Store.Business
{
	public class Item
	{
		public long Id { get; set; }

		public Guid ItemTypeId { get; set; }

		public int Quantity { get; set; }

		public long InventoryId { get; set; }

		public string Bag { get; set; }

		public long BagVersion { get; set; }
	}
}
