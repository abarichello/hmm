using System;
using System.Collections.Generic;
using System.Linq;
using ClientAPI.Objects;
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
			foreach (Item item in items)
			{
				if (this.ItemTypeGuidToItem.ContainsKey(item.ItemTypeId))
				{
					InventoryAdapter.Log.WarnFormat("Item already added[{0}], Id:[{1}]", new object[]
					{
						item.ItemTypeId,
						this.ItemTypeGuidToItem[item.ItemTypeId].Id
					});
				}
				else
				{
					this.ItemTypeGuidToItem.Add(item.ItemTypeId, item);
				}
			}
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
	}
}
