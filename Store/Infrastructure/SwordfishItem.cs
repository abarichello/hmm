using System;
using ClientAPI.Objects;

namespace HeavyMetalMachines.Store.Infrastructure
{
	public class SwordfishItem
	{
		public long Id { get; set; }

		public Guid ItemTypeId { get; set; }

		public int Quantity { get; set; }

		public long InventoryId { get; set; }

		public string Bag { get; set; }

		public long BagVersion { get; set; }

		public ItemType ItemType { get; set; }
	}
}
