using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Store.Infrastructure;
using UniRx;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishInventoryService : ISwordfishInventoryService
	{
		public IObservable<SwordfishItem> GetItem(long instanceItemId)
		{
			throw new NotImplementedException();
		}

		public Inventory GetInventoryByKind(InventoryBag.InventoryKind inventoryKind)
		{
			return new Inventory();
		}
	}
}
