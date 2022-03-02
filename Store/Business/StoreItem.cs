using System;

namespace HeavyMetalMachines.Store.Business
{
	public class StoreItem
	{
		public Guid Id { get; set; }

		public long SoftPrice { get; set; }

		public long HardPrice { get; set; }

		public bool IsPurchasable { get; set; }

		public bool IsHardPurchasable { get; set; }

		public bool IsSoftPurchasable { get; set; }
	}
}
