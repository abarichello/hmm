using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Items.DataTransferObjects;
using Hoplon.Serialization;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class PlayerInventory : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnInvetoryReload;

		public bool HasItemOfType(Guid itemTypeId)
		{
			return this._ownedItensQuantity.ContainsKey(itemTypeId);
		}

		public InventoryAdapter GetInventory(PlayerInventory.Check check)
		{
			if (this.Inventories == null || this.Inventories.Length < 1)
			{
				PlayerInventory.Log.Warn("Player Inventory is null or empty");
				return null;
			}
			for (int i = 0; i < this.Inventories.Length; i++)
			{
				InventoryAdapter inventoryAdapter = this.Inventories[i];
				if (check(inventoryAdapter))
				{
					return inventoryAdapter;
				}
			}
			PlayerInventory.Log.Warn("Player Inventory is null or empty");
			return null;
		}

		public InventoryAdapter GetInventoryAdapterByKind(InventoryBag.InventoryKind inventoryKind)
		{
			for (int i = 0; i < this.Inventories.Length; i++)
			{
				InventoryAdapter inventoryAdapter = this.Inventories[i];
				InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<!0>)inventoryAdapter.Inventory.Bag);
				if (inventoryBag != null && inventoryBag.Kind == inventoryKind)
				{
					return inventoryAdapter;
				}
			}
			return null;
		}

		public Inventory GetInventoryByKind(InventoryBag.InventoryKind inventoryKind)
		{
			InventoryAdapter inventoryAdapterByKind = this.GetInventoryAdapterByKind(inventoryKind);
			return (inventoryAdapterByKind != null) ? inventoryAdapterByKind.Inventory : null;
		}

		public Item GetItemById(long itemId)
		{
			for (int i = 0; i < this.Inventories.Length; i++)
			{
				Item[] items = this.Inventories[i].Items;
				if (items != null)
				{
					foreach (Item item in items)
					{
						if (item.Id == itemId)
						{
							return item;
						}
					}
				}
			}
			return null;
		}

		private InventoryAdapter[] AdpatedInventoryies(Inventory[] inventories, Item[] allItems)
		{
			Dictionary<long, List<Item>> dictionary = new Dictionary<long, List<Item>>();
			foreach (Inventory inventory in inventories)
			{
				dictionary[inventory.Id] = new List<Item>();
			}
			foreach (Item item in allItems)
			{
				dictionary[item.InventoryId].Add(item);
				this.PopulateOwnedItensDictionary(item.ItemTypeId);
			}
			InventoryAdapter[] array = new InventoryAdapter[inventories.Length];
			for (int k = 0; k < inventories.Length; k++)
			{
				array[k] = new InventoryAdapter(inventories[k], dictionary[inventories[k].Id].ToArray());
			}
			return array;
		}

		private void PopulateOwnedItensDictionary(Guid itemTypeId)
		{
			if (this._ownedItensQuantity.ContainsKey(itemTypeId))
			{
				Dictionary<Guid, int> ownedItensQuantity;
				(ownedItensQuantity = this._ownedItensQuantity)[itemTypeId] = ownedItensQuantity[itemTypeId] + 1;
				return;
			}
			this._ownedItensQuantity[itemTypeId] = 1;
		}

		public void SetAllReloadedItems(Inventory[] inventories, Item[] allItems)
		{
			if (allItems == null)
			{
				allItems = new Item[0];
			}
			this._ownedItensQuantity.Clear();
			this.Inventories = this.AdpatedInventoryies(inventories, allItems);
			PlayerInventory.Log.Debug("SetAllReloadedItems - Inventories: " + this.Inventories.Length);
		}

		public void RefreshPlayerCustomizations()
		{
			Inventory inventoryByKind = this.GetInventoryByKind(3);
			InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<!0>)inventoryByKind.Bag);
			CustomizationContent customizationContent = new CustomizationContent();
			customizationContent.DeserializeAndUpdate(inventoryBag.Content);
			customizationContent.SyncDictionary();
			this.Customizations = customizationContent;
			PlayerInventory.Log.Debug("RefreshPlayerCustomizations: " + inventoryBag.Content);
		}

		public void ReloadInventoryByID(long inventoryId)
		{
			GameHubBehaviour.Hub.ClientApi.inventory.GetInventoryById(null, inventoryId, new SwordfishClientApi.ParameterizedCallback<Inventory>(this.OnGetInventorySucess), new SwordfishClientApi.ErrorCallback(this.OnGetInventoryError));
		}

		private void OnGetInventoryError(object state, Exception exception)
		{
			PlayerInventory.Log.ErrorFormat("Error when reloading inventory, Error {0}", new object[]
			{
				exception
			});
		}

		private void OnGetInventorySucess(object state, Inventory inventory)
		{
			InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<!0>)inventory.Bag);
			if (inventoryBag == null)
			{
				return;
			}
			this.SetInventoryAdapterByKind(inventoryBag.Kind, inventory);
		}

		private void SetInventoryAdapterByKind(InventoryBag.InventoryKind inventoryKind, Inventory inventory)
		{
			int inventoryIndexByKind = this.GetInventoryIndexByKind(inventoryKind);
			if (inventoryIndexByKind < 0)
			{
				PlayerInventory.Log.ErrorFormat("Inventory not update {0}", new object[]
				{
					inventoryKind
				});
				return;
			}
			InventoryAdapter inventoryAdapter = this.Inventories[inventoryIndexByKind];
			InventoryAdapter inventoryAdapter2 = new InventoryAdapter(inventory, inventoryAdapter.Items);
			this.Inventories[inventoryIndexByKind] = inventoryAdapter2;
			if (this.OnInvetoryReload != null)
			{
				this.OnInvetoryReload();
			}
		}

		private int GetInventoryIndexByKind(InventoryBag.InventoryKind inventoryKind)
		{
			for (int i = 0; i < this.Inventories.Length; i++)
			{
				InventoryAdapter inventoryAdapter = this.Inventories[i];
				InventoryBag inventoryBag = (InventoryBag)((JsonSerializeable<!0>)inventoryAdapter.Inventory.Bag);
				if (inventoryBag != null && inventoryBag.Kind == inventoryKind)
				{
					return i;
				}
			}
			return -1;
		}

		public void FetchItem(long itemId, Action<Item> successCallback = null, Action<Exception> errorCallback = null)
		{
			GameHubBehaviour.Hub.ClientApi.inventory.GetItemById(null, itemId, delegate(object state, Item item)
			{
				if (successCallback != null)
				{
					successCallback(item);
				}
			}, delegate(object state, Exception exception)
			{
				PlayerInventory.Log.ErrorFormat("Error when fetching item. Exception={0}", new object[]
				{
					exception
				});
				if (errorCallback != null)
				{
					errorCallback(exception);
				}
			});
		}

		public void AddItem(Item item)
		{
			InventoryAdapter inventory = this.GetInventory((InventoryAdapter i) => i.Inventory.Id == item.InventoryId);
			inventory.ConcatItem(item);
			this.PopulateOwnedItensDictionary(item.ItemTypeId);
		}

		public void ReloadItemByItemTypeId(Guid itemTypeId, long inventoryId, Action<long> whenDone = null)
		{
			InventoryAdapter inventoryAdapter = this.GetInventory((InventoryAdapter i) => i.Inventory.Id == inventoryId);
			GameHubBehaviour.Hub.ClientApi.inventory.GetItemsByItemTypeId(null, itemTypeId, inventoryId, delegate(object state, Item[] items)
			{
				int num = 0;
				if (num >= items.Length)
				{
					return;
				}
				Item item = items[num];
				long itemId = inventoryAdapter.GetItemId(item.ItemTypeId);
				if (itemId < 0L)
				{
					inventoryAdapter.ConcatItem(item);
					this.PopulateOwnedItensDictionary(item.ItemTypeId);
				}
				if (whenDone != null)
				{
					whenDone(item.Id);
				}
			}, delegate(object state, Exception exception)
			{
				PlayerInventory.Log.ErrorFormat("Error when reloading item! Exception={0}", new object[]
				{
					exception
				});
				if (whenDone != null)
				{
					whenDone(-1L);
				}
			});
		}

		public void AddSingleItem(ItemAddResult itemAdded)
		{
			InventoryAdapter inventory = this.GetInventory((InventoryAdapter i) => i.Inventory.Id == itemAdded.InventoryId);
			Item item = new Item
			{
				ItemTypeId = itemAdded.ItemTypeId,
				Id = itemAdded.Id,
				Bag = itemAdded.Bag,
				ItemType = null,
				Quantity = itemAdded.Quantity,
				BagVersion = itemAdded.BagVersion,
				InventoryId = itemAdded.InventoryId
			};
			inventory.Items = inventory.Items.Concat(new Item[]
			{
				item
			}).ToArray<Item>();
		}

		public void ReloadAllItems(Action whenDone = null)
		{
			new PlayerInventory.InventoryLoadingState(whenDone, this);
		}

		private void Awake()
		{
			this.User = base.GetComponent<UserInfo>();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerInventory));

		public UserInfo User;

		public InventoryAdapter[] Inventories;

		private int _inventoryVersion;

		public CustomizationContent Customizations;

		public bool HasNewItems;

		private Dictionary<Guid, int> _ownedItensQuantity = new Dictionary<Guid, int>();

		public delegate bool Check(InventoryAdapter i);

		private class InventoryLoadingState
		{
			public InventoryLoadingState(Action whenDone, PlayerInventory playerInventory)
			{
				this._thisCallVersion = PlayerInventory.InventoryLoadingState._callVersion++;
				this._playerPlayerInventory = playerInventory;
				this._whenDone = whenDone;
				GameHubBehaviour.Hub.ClientApi.inventory.GetPlayerInventories(this, this._playerPlayerInventory.User.PlayerSF.Id, new SwordfishClientApi.ParameterizedCallback<Inventory[]>(this.OnInventoriesLoaded), new SwordfishClientApi.ErrorCallback(this.OnLoadError));
			}

			private void OnInventoriesLoaded(object state, Inventory[] obj)
			{
				this._inventories = obj;
				GameHubBehaviour.Hub.ClientApi.inventory.GetAllItems(null, this._playerPlayerInventory.User.UserSF.Id, new SwordfishClientApi.ParameterizedCallback<Item[]>(this.OnItemsLoaded), new SwordfishClientApi.ErrorCallback(this.OnLoadError));
			}

			private void OnItemsLoaded(object state, Item[] allItems)
			{
				this._allItems = allItems;
				this.Finalized();
			}

			private void Finalized()
			{
				if (this._playerPlayerInventory._inventoryVersion <= this._thisCallVersion)
				{
					this._playerPlayerInventory._inventoryVersion = this._thisCallVersion;
					this._playerPlayerInventory.SetAllReloadedItems(this._inventories, this._allItems);
				}
				if (this._whenDone != null)
				{
					this._whenDone();
				}
				this._whenDone = null;
				this._inventories = null;
				this._allItems = null;
				this._playerPlayerInventory = null;
			}

			private void OnLoadError(object state, Exception exception)
			{
				PlayerInventory.Log.Fatal("Failed to load inventories.", exception);
				if (this._whenDone != null)
				{
					this._whenDone();
				}
			}

			private static int _callVersion;

			private Action _whenDone;

			private PlayerInventory _playerPlayerInventory;

			private readonly int _thisCallVersion;

			private Inventory[] _inventories;

			private Item[] _allItems;
		}
	}
}
