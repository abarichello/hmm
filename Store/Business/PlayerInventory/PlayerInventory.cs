using System;
using ClientAPI.Objects;

namespace HeavyMetalMachines.Store.Business.PlayerInventory
{
	public class PlayerInventory : IPlayerInventory
	{
		public PlayerInventory(PlayerInventory playerInventory)
		{
			if (playerInventory == null)
			{
				throw new ArgumentNullException("playerInventory");
			}
			this._playerInventory = playerInventory;
		}

		public void AddItem(Item item)
		{
			this._playerInventory.AddItem(new Item
			{
				Id = item.Id,
				Bag = item.Bag,
				Quantity = item.Quantity,
				BagVersion = item.BagVersion,
				InventoryId = item.InventoryId,
				ItemTypeId = item.ItemTypeId
			});
		}

		private readonly PlayerInventory _playerInventory;
	}
}
