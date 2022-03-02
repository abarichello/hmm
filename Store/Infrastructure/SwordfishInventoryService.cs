using System;
using ClientAPI.Objects;
using UniRx;

namespace HeavyMetalMachines.Store.Infrastructure
{
	public class SwordfishInventoryService : ISwordfishInventoryService
	{
		public SwordfishInventoryService(PlayerInventory playerInventory)
		{
			if (playerInventory == null)
			{
				throw new ArgumentNullException("playerInventory");
			}
			this._playerInventory = playerInventory;
		}

		public IObservable<SwordfishItem> GetItem(long instanceItemId)
		{
			return Observable.Create<SwordfishItem>(delegate(IObserver<SwordfishItem> observer)
			{
				this._playerInventory.FetchItem(instanceItemId, delegate(Item item)
				{
					observer.OnNext(SwordfishInventoryService.ConvertItem(item));
					observer.OnCompleted();
				}, new Action<Exception>(observer.OnError));
				return Disposable.Empty;
			});
		}

		private static SwordfishItem ConvertItem(Item item)
		{
			return new SwordfishItem
			{
				Id = item.Id,
				Bag = item.Bag,
				Quantity = item.Quantity,
				ItemType = item.ItemType,
				BagVersion = item.BagVersion,
				InventoryId = item.InventoryId,
				ItemTypeId = item.ItemTypeId
			};
		}

		public Inventory GetInventoryByKind(InventoryBag.InventoryKind inventoryKind)
		{
			return this._playerInventory.GetInventoryByKind(inventoryKind);
		}

		private readonly PlayerInventory _playerInventory;
	}
}
