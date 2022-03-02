using System;
using System.Collections.Generic;
using System.Linq;
using ClientAPI.Objects;
using Hoplon.Serialization;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class InventoryAdapter
	{
		public InventoryAdapter(Inventory inventory, Item[] items)
		{
			this.Inventory = inventory;
			this.Items = items;
			this.ItemTypeGuidToItem = new Dictionary<Guid, Item>();
			this.Initialize(items);
		}

		private void Initialize(Item[] items)
		{
			InventoryAdapter.AddedItemsTracker addedItemsTracker = new InventoryAdapter.AddedItemsTracker(this.Inventory);
			foreach (Item item in items)
			{
				addedItemsTracker.Add(item.ItemTypeId);
				if (!this.ItemTypeGuidToItem.ContainsKey(item.ItemTypeId))
				{
					this.ItemTypeGuidToItem.Add(item.ItemTypeId, item);
				}
			}
			addedItemsTracker.Dump();
		}

		public void ConcatItem(Item item)
		{
			this.Items = this.Items.Concat(new Item[]
			{
				item
			}).ToArray<Item>();
			this.ItemTypeGuidToItem[item.ItemTypeId] = item;
		}

		public long GetItemId(Guid itemTypeId)
		{
			Item item;
			if (this.ItemTypeGuidToItem.TryGetValue(itemTypeId, out item))
			{
				return item.Id;
			}
			return -1L;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(InventoryAdapter));

		public readonly Inventory Inventory;

		public Item[] Items;

		public readonly Dictionary<Guid, Item> ItemTypeGuidToItem;

		private class AddedItemsTracker
		{
			public AddedItemsTracker(Inventory inventory)
			{
				InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<!0>)inventory.Bag);
				this._inventoryKind = ((inventoryBag == null) ? 0 : inventoryBag.Kind);
				this._alreadyAdded = new Dictionary<Guid, int>();
			}

			public void Add(Guid id)
			{
				if (!this._alreadyAdded.ContainsKey(id))
				{
					this._alreadyAdded.Add(id, 0);
				}
				Dictionary<Guid, int> alreadyAdded;
				(alreadyAdded = this._alreadyAdded)[id] = alreadyAdded[id] + 1;
			}

			public void Dump()
			{
				InventoryAdapter.Log.DebugFormat("Dumping duplicated items from inventory {0}:", new object[]
				{
					this._inventoryKind
				});
				foreach (KeyValuePair<Guid, int> keyValuePair in this._alreadyAdded)
				{
					string message = string.Format("Item [{0}] added {1} times.", keyValuePair.Key, keyValuePair.Value);
					if (keyValuePair.Value > 1)
					{
						InventoryAdapter.Log.Warn(message);
					}
				}
			}

			private readonly Dictionary<Guid, int> _alreadyAdded;

			private readonly InventoryBag.InventoryKind _inventoryKind;
		}
	}
}
