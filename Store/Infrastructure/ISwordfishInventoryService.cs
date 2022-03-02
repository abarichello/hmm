using System;
using ClientAPI.Objects;
using UniRx;

namespace HeavyMetalMachines.Store.Infrastructure
{
	public interface ISwordfishInventoryService
	{
		IObservable<SwordfishItem> GetItem(long instanceItemId);

		Inventory GetInventoryByKind(InventoryBag.InventoryKind inventoryKind);
	}
}
