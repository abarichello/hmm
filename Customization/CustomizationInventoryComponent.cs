using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI;
using ClientAPI.Objects;
using Commons.Swordfish.Battlepass;
using Commons.Swordfish.Inventory;
using Commons.Swordfish.Util;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Customization
{
	[CreateAssetMenu(menuName = "UnityUI/CustomizationInventoryComponent")]
	public class CustomizationInventoryComponent : GameHubScriptableObject
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<PlayerCustomizationSlot> OnItemEquiped;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> OnHasNewItemsStateChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event System.Action OnSkinTabHasNewItemsStateChanged;

		public bool HasNewItems
		{
			get
			{
				return this._hasNewItems;
			}
		}

		private void OnEnable()
		{
			MainMenu.PlayerReloadedEvent += this.OnPlayerDataReloaded;
		}

		private void OnDisable()
		{
			MainMenu.PlayerReloadedEvent -= this.OnPlayerDataReloaded;
		}

		public void LoadInventoryScene(System.Action onCloseInventoryCallback)
		{
			this.OnCloseInventoryCallback = onCloseInventoryCallback;
			SceneManager.LoadSceneAsync(this._inventorySceneName, LoadSceneMode.Additive);
		}

		public void RegisterView(ICustomizationInventoryView view)
		{
			this._customizationInventoryView = view;
			this.ShowCustomizationInventoryWindow();
		}

		public void ShowCustomizationInventoryWindow()
		{
			if (!this._customizationInventoryView.IsVisible())
			{
				this._customizationInventoryView.SetVisibility(true, false);
			}
		}

		public void HideCustomizationInventoryWindow(bool imediate = false)
		{
			if (!this._customizationInventoryView.IsVisible())
			{
				return;
			}
			this._customizationInventoryView.SetVisibility(false, imediate);
			SceneManager.UnloadSceneAsync(this._inventorySceneName);
			this._categoryInventory.Clear();
			this._customizationInventoryView = null;
			if (this.OnCloseInventoryCallback != null)
			{
				this.OnCloseInventoryCallback();
			}
			this.OnCloseInventoryCallback = null;
		}

		public CustomizationInventoryCellItemData GetItem(Guid categoryId, Guid itemTypeId)
		{
			CustomizationInventoryCategoryData customizationInventoryCategoryData;
			CustomizationInventoryCellItemData result;
			if (this._categoryInventory.TryGetValue(categoryId, out customizationInventoryCategoryData) && customizationInventoryCategoryData.ItemsDictionary.TryGetValue(itemTypeId, out result))
			{
				return result;
			}
			return null;
		}

		private CustomizationInventoryCategoryData GetCategoryData(Guid categoryId)
		{
			CustomizationInventoryCategoryData result;
			if (!this._categoryInventory.TryGetValue(categoryId, out result))
			{
				result = this.PopulateCategoryItems(categoryId);
			}
			return result;
		}

		public void GetCategoryItems(Guid categoryId)
		{
			CustomizationInventoryCategoryData categoryData = this.GetCategoryData(categoryId);
			this._customizationInventoryView.Setup(categoryData);
		}

		public int GetCategoryNewItemsCount(Guid categoryId)
		{
			CustomizationInventoryCategoryData categoryData = this.GetCategoryData(categoryId);
			return categoryData.NewItemsCount;
		}

		private CustomizationInventoryCategoryData PopulateCategoryItems(Guid categoryId)
		{
			this._categoriesIds.Add(categoryId);
			ItemCategoryScriptableObject itemCategory = this.GetItemCategory(categoryId);
			string categoryName = (!itemCategory) ? "Unnamed category" : Language.Get(itemCategory.TitleDraft, TranslationSheets.Inventory);
			CustomizationInventoryCategoryData customizationInventoryCategoryData = new CustomizationInventoryCategoryData(categoryId, categoryName);
			customizationInventoryCategoryData.CustomizationSlot = ((!itemCategory) ? PlayerCustomizationSlot.None : itemCategory.CustomizationSlot);
			customizationInventoryCategoryData.IsLore = (itemCategory.Name == "Lore");
			this._categoryInventory[categoryId] = customizationInventoryCategoryData;
			if (customizationInventoryCategoryData.CustomizationSlot != PlayerCustomizationSlot.Skin)
			{
				ItemTypeScriptableObject defaultCategoryItem = GameHubScriptableObject.Hub.CustomizationAssets.GetDefaultCategoryItem(categoryId);
				CustomizationInventoryCellItemData customizationInventoryCellItemData = this.ConvertItemType(defaultCategoryItem, null);
				Guid defaultItemTypeId = Guid.Empty;
				if (customizationInventoryCellItemData != null)
				{
					defaultItemTypeId = customizationInventoryCellItemData.ItemTypeId;
					customizationInventoryCellItemData.IsDefault = true;
					customizationInventoryCellItemData.DateAcquired = DateTime.MinValue;
					customizationInventoryCategoryData.AddItem(customizationInventoryCellItemData);
				}
				PlayerCustomizationSlot customizationSlotByCategoryId = this.GetCustomizationSlotByCategoryId(categoryId);
				Guid guidBySlot = GameHubScriptableObject.Hub.User.Inventory.Customizations.GetGuidBySlot(customizationSlotByCategoryId);
				if (customizationInventoryCellItemData != null && (guidBySlot == Guid.Empty || guidBySlot == customizationInventoryCellItemData.ItemTypeId))
				{
					customizationInventoryCategoryData.EquipItem(customizationInventoryCellItemData.ItemTypeId);
				}
				this.TryToAddItemData(InventoryBag.InventoryKind.Customization, categoryId, defaultItemTypeId, guidBySlot, customizationInventoryCategoryData);
				this.TryToAddItemData(InventoryBag.InventoryKind.Collectables, categoryId, defaultItemTypeId, guidBySlot, customizationInventoryCategoryData);
				customizationInventoryCategoryData.SortItems(CustomizationInventoryCategoryData.SortKind.AcquisitionDate, true);
			}
			else
			{
				this.TryToAddSkinItemData(customizationInventoryCategoryData);
			}
			return customizationInventoryCategoryData;
		}

		private void TryToAddSkinItemData(CustomizationInventoryCategoryData categoryData)
		{
			InventoryAdapter inventoryAdapterByKind = GameHubScriptableObject.Hub.User.Inventory.GetInventoryAdapterByKind(InventoryBag.InventoryKind.Customization);
			if (inventoryAdapterByKind == null)
			{
				return;
			}
			List<ItemTypeScriptableObject> list = GameHubScriptableObject.Hub.InventoryColletion.CategoriesIdToItemTypes[InventoryMapper.CharactersCategoryGuid];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = list[i];
				bool isExpanded;
				if (!this._expandStateSkins.TryGetValue(itemTypeScriptableObject.Id, out isExpanded))
				{
					this._expandStateSkins.Add(itemTypeScriptableObject.Id, true);
					isExpanded = true;
				}
				bool flag = GameHubScriptableObject.Hub.User.Inventory.HasItemOfType(itemTypeScriptableObject.Id);
				CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
				List<Guid> list2 = GameHubScriptableObject.Hub.InventoryColletion.CharacterToSkinGuids[itemTypeScriptableObject.Id];
				List<CustomizationInventoryCellItemData> list3 = new List<CustomizationInventoryCellItemData>(list2.Count + 1);
				CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<T>)itemTypeScriptableObject.Bag);
				ItemTypeScriptableObject itemTypeScriptableObject2 = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[characterItemTypeBag.DefaultSkinGuid];
				if (flag)
				{
					CustomizationInventoryCellItemData customizationInventoryCellItemData = this.ConvertItemType(itemTypeScriptableObject2, null);
					if (customizationInventoryCellItemData != null)
					{
						categoryData.AddItem(customizationInventoryCellItemData);
						list3.Add(customizationInventoryCellItemData);
					}
					else
					{
						CustomizationInventoryComponent.Log.WarnFormat("Missing Inventory Data for ItemType: {0}", new object[]
						{
							itemTypeScriptableObject2
						});
					}
				}
				for (int j = 0; j < list2.Count; j++)
				{
					ItemTypeScriptableObject itemTypeScriptableObject3 = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[list2[j]];
					Item item;
					if (inventoryAdapterByKind.ItemTypeGuidToItem.TryGetValue(itemTypeScriptableObject3.Id, out item))
					{
						CustomizationInventoryCellItemData customizationInventoryCellItemData2 = this.ConvertItemType(itemTypeScriptableObject3, item);
						if (customizationInventoryCellItemData2 == null)
						{
							CustomizationInventoryComponent.Log.WarnFormat("Missing Inventory Data for ItemType: {0}", new object[]
							{
								itemTypeScriptableObject3
							});
						}
						else if (!(itemTypeScriptableObject2 != null) || !(itemTypeScriptableObject2.Id == itemTypeScriptableObject3.Id))
						{
							categoryData.AddItem(customizationInventoryCellItemData2);
							list3.Add(customizationInventoryCellItemData2);
						}
					}
				}
				if (list3.Count > 0)
				{
					CustomizationInventoryCellItemSkinTabData skinTabData = new CustomizationInventoryCellItemSkinTabData
					{
						CharacterId = itemTypeScriptableObject.Id,
						CharacterName = component.MainAttributes.LocalizedName,
						IconAssetName = component.CharacterIcon64Name,
						IsNew = this.CharacterSkinHasNewItems(inventoryAdapterByKind, itemTypeScriptableObject.Id),
						IsExpanded = isExpanded,
						HasCharacter = flag
					};
					categoryData.AddSkinTabDataItem(skinTabData, list3);
				}
			}
		}

		public bool UpdateSkinTabDataNewItems(CustomizationInventoryCategoryData categoryData)
		{
			InventoryAdapter inventoryAdapterByKind = GameHubScriptableObject.Hub.User.Inventory.GetInventoryAdapterByKind(InventoryBag.InventoryKind.Customization);
			if (inventoryAdapterByKind == null)
			{
				return false;
			}
			List<ItemTypeScriptableObject> list = GameHubScriptableObject.Hub.InventoryColletion.CategoriesIdToItemTypes[InventoryMapper.CharactersCategoryGuid];
			bool flag = false;
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = list[i];
				if (GameHubScriptableObject.Hub.User.Inventory.HasItemOfType(itemTypeScriptableObject.Id))
				{
					for (int j = 0; j < categoryData.SkinTabDataItems.Count; j++)
					{
						CustomizationInventoryCellItemSkinTabData customizationInventoryCellItemSkinTabData = categoryData.SkinTabDataItems[j];
						if (customizationInventoryCellItemSkinTabData.CharacterId == itemTypeScriptableObject.Id)
						{
							bool flag2 = this.CharacterSkinHasNewItems(inventoryAdapterByKind, itemTypeScriptableObject.Id);
							flag |= (customizationInventoryCellItemSkinTabData.IsNew != flag2);
							customizationInventoryCellItemSkinTabData.IsNew = flag2;
							break;
						}
					}
				}
			}
			return flag;
		}

		private bool CharacterSkinHasNewItems(InventoryAdapter inventoryAdapter, Guid characterId)
		{
			List<Guid> list = GameHubScriptableObject.Hub.InventoryColletion.CharacterToSkinGuids[characterId];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[list[i]];
				Item item;
				if (inventoryAdapter.ItemTypeGuidToItem.TryGetValue(itemTypeScriptableObject.Id, out item))
				{
					CustomizationInventoryCellItemData customizationInventoryCellItemData = this.ConvertItemType(itemTypeScriptableObject, item);
					if (customizationInventoryCellItemData != null && customizationInventoryCellItemData.IsNew)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void TryToAddItemData(InventoryBag.InventoryKind inventoryKind, Guid categoryId, Guid defaultItemTypeId, Guid equippedItemTypeId, CustomizationInventoryCategoryData categoryData)
		{
			InventoryAdapter inventoryAdapterByKind = GameHubScriptableObject.Hub.User.Inventory.GetInventoryAdapterByKind(inventoryKind);
			if (inventoryAdapterByKind == null)
			{
				return;
			}
			for (int i = 0; i < inventoryAdapterByKind.Items.Length; i++)
			{
				Item item = inventoryAdapterByKind.Items[i];
				if (!(item.ItemTypeId == defaultItemTypeId))
				{
					ItemTypeScriptableObject itemTypeScriptableObject;
					if (GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes.TryGetValue(item.ItemTypeId, out itemTypeScriptableObject) && itemTypeScriptableObject.ItemCategoryId == categoryId)
					{
						CustomizationInventoryCellItemData customizationInventoryCellItemData = this.ConvertItemType(itemTypeScriptableObject, item);
						if (customizationInventoryCellItemData == null)
						{
							CustomizationInventoryComponent.Log.WarnFormat("Missing Inventory Data for ItemType: {0}", new object[]
							{
								itemTypeScriptableObject
							});
						}
						else
						{
							if (customizationInventoryCellItemData.ItemTypeId == equippedItemTypeId)
							{
								customizationInventoryCellItemData.IsEquipped = true;
							}
							categoryData.AddItem(customizationInventoryCellItemData);
						}
					}
				}
			}
		}

		private PlayerCustomizationSlot GetCustomizationSlotByCategoryId(Guid categoryId)
		{
			ItemCategoryScriptableObject itemCategory = this.GetItemCategory(categoryId);
			return (!itemCategory) ? PlayerCustomizationSlot.None : itemCategory.CustomizationSlot;
		}

		private void AddItemToCategory(CustomizationInventoryCellItemData itemData)
		{
			Guid itemCategoryId = itemData.ItemCategoryId;
			ItemCategoryScriptableObject itemCategory = this.GetItemCategory(itemCategoryId);
			CustomizationInventoryCategoryData customizationInventoryCategoryData;
			if (!this._categoryInventory.TryGetValue(itemCategoryId, out customizationInventoryCategoryData))
			{
				string categoryName = (!(itemCategory == null)) ? Language.Get(itemCategory.TitleDraft, TranslationSheets.Inventory) : string.Empty;
				customizationInventoryCategoryData = new CustomizationInventoryCategoryData(itemCategoryId, categoryName);
				this._categoryInventory[itemCategoryId] = customizationInventoryCategoryData;
			}
			customizationInventoryCategoryData.AddItem(itemData);
		}

		private CustomizationInventoryCellItemData ConvertItemType(ItemTypeScriptableObject itemType, Item item)
		{
			if (itemType == null)
			{
				return null;
			}
			ItemTypeComponent itemTypeComponent;
			if (!itemType.GetComponentByEnum(ItemTypeComponent.Type.Inventory, out itemTypeComponent))
			{
				return null;
			}
			InventoryItemTypeComponent inventoryItemTypeComponent = itemTypeComponent as InventoryItemTypeComponent;
			if (inventoryItemTypeComponent == null)
			{
				return null;
			}
			bool flag = item != null;
			CustomizationInventoryCellItemData customizationInventoryCellItemData = new CustomizationInventoryCellItemData();
			customizationInventoryCellItemData.ItemId = ((!flag) ? -1L : item.Id);
			customizationInventoryCellItemData.ItemTypeId = itemType.Id;
			customizationInventoryCellItemData.ItemCategoryId = itemType.ItemCategoryId;
			customizationInventoryCellItemData.ItemName = inventoryItemTypeComponent.TitleDraft;
			customizationInventoryCellItemData.ItemDescription = inventoryItemTypeComponent.DescriptionDraft;
			customizationInventoryCellItemData.IconName = inventoryItemTypeComponent.InventoryIconName;
			customizationInventoryCellItemData.PreviewName = inventoryItemTypeComponent.InventoryPreviewName;
			customizationInventoryCellItemData.PreviewKind = inventoryItemTypeComponent.PreviewKind;
			customizationInventoryCellItemData.ArtPreviewBackGroundAssetName = inventoryItemTypeComponent.ArtPreviewBackGroundAssetName;
			customizationInventoryCellItemData.IsEquipped = false;
			customizationInventoryCellItemData.IsSelected = false;
			ItemTypeComponent itemTypeComponent2;
			if (itemType.GetComponentByEnum(ItemTypeComponent.Type.SkinPrefab, out itemTypeComponent2))
			{
				customizationInventoryCellItemData.SkinCustomizations = ((SkinPrefabItemTypeComponent)itemTypeComponent2).SkinCustomization;
			}
			string loreTitleDraft = string.Empty;
			string loreSubtitleDraft = string.Empty;
			string loreDescriptionDraft = string.Empty;
			if (itemType.GetComponentByEnum(ItemTypeComponent.Type.Lore, out itemTypeComponent))
			{
				LoreItemTypeComponent loreItemTypeComponent = (LoreItemTypeComponent)itemTypeComponent;
				loreTitleDraft = loreItemTypeComponent._loreTitle;
				loreSubtitleDraft = loreItemTypeComponent._loreSubTitle;
				loreDescriptionDraft = loreItemTypeComponent._loreText;
			}
			customizationInventoryCellItemData.LoreTitleDraft = loreTitleDraft;
			customizationInventoryCellItemData.LoreSubtitleDraft = loreSubtitleDraft;
			customizationInventoryCellItemData.LoreDescriptionDraft = loreDescriptionDraft;
			if (flag)
			{
				if (string.IsNullOrEmpty(item.Bag))
				{
					customizationInventoryCellItemData.IsNew = true;
				}
				else
				{
					CustomizationItemTypeBag customizationItemTypeBag = (CustomizationItemTypeBag)((JsonSerializeable<T>)item.Bag);
					customizationInventoryCellItemData.IsNew = !customizationItemTypeBag.Seen;
				}
			}
			else
			{
				customizationInventoryCellItemData.IsNew = false;
			}
			customizationInventoryCellItemData.DateAcquired = DateTime.Now;
			return customizationInventoryCellItemData;
		}

		public void EquipItem(Guid categoryId, Guid itemTypeId)
		{
			CustomizationInventoryCellItemData item = this.GetItem(categoryId, itemTypeId);
			if (item.IsEquipped)
			{
				this._customizationInventoryView.OnEquipItemResponse(true);
				return;
			}
			PlayerCustomizationSlot customizationSlotByCategoryId = this.GetCustomizationSlotByCategoryId(categoryId);
			if (customizationSlotByCategoryId == PlayerCustomizationSlot.None)
			{
				return;
			}
			CustomizationInventoryComponent.EquipItemRequestState state = new CustomizationInventoryComponent.EquipItemRequestState
			{
				CategoryId = categoryId,
				ItemTypeId = itemTypeId
			};
			if (item.IsDefault)
			{
				itemTypeId = Guid.Empty;
			}
			BattlepassCustomWS.SaveCustomizationsSelected(new CustomizationBagAdapter
			{
				Slot = customizationSlotByCategoryId,
				TypeId = itemTypeId
			}, state, new SwordfishClientApi.ParameterizedCallback<string>(this.OnEquipItemSuccess), new SwordfishClientApi.ErrorCallback(this.OnEquipItemError));
		}

		private void MarkItemAsEquipped(Guid categoryId, Guid itemTypeId)
		{
			CustomizationInventoryCategoryData customizationInventoryCategoryData;
			if (!this._categoryInventory.TryGetValue(categoryId, out customizationInventoryCategoryData))
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Failed to mark item as equipped: No category data found with ID {0}", new object[]
				{
					categoryId
				});
				return;
			}
			PlayerCustomizationSlot playerCustomizationSlot = PlayerCustomizationSlot.None;
			for (int i = 0; i < GameHubScriptableObject.Hub.InventoryColletion.ItemCategories.Length; i++)
			{
				ItemCategoryScriptableObject itemCategoryScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.ItemCategories[i];
				if (itemCategoryScriptableObject.Id == categoryId)
				{
					playerCustomizationSlot = itemCategoryScriptableObject.CustomizationSlot;
				}
			}
			if (playerCustomizationSlot == PlayerCustomizationSlot.None)
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Failed to mark item as equipped: Unable to find category slot with ID {0}", new object[]
				{
					categoryId
				});
			}
			else
			{
				GameHubScriptableObject.Hub.User.Inventory.Customizations.SetGuidBySlot(playerCustomizationSlot, itemTypeId);
			}
			if (this.OnItemEquiped != null)
			{
				this.OnItemEquiped(playerCustomizationSlot);
			}
			customizationInventoryCategoryData.EquipItem(itemTypeId);
		}

		private void OnEquipItemSuccess(object state, string json)
		{
			CustomizationInventoryComponent.EquipItemRequestState equipItemRequestState = state as CustomizationInventoryComponent.EquipItemRequestState;
			bool flag = null != this._customizationInventoryView;
			if (equipItemRequestState == null)
			{
				CustomizationInventoryComponent.Log.Error("Failed to equip item: No state object received.");
				if (flag)
				{
					this._customizationInventoryView.OnEquipItemResponse(false);
				}
				return;
			}
			NetResult netResult = (NetResult)((JsonSerializeable<T>)json);
			if (netResult.Success)
			{
				this.MarkItemAsEquipped(equipItemRequestState.CategoryId, equipItemRequestState.ItemTypeId);
				if (flag)
				{
					this._customizationInventoryView.OnEquipItemResponse(true);
				}
			}
			else
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Failed to equip item: Attempt unsuccessful. CategoryId={0} ItemTypeId={1}. Full response={2}", new object[]
				{
					equipItemRequestState.CategoryId,
					equipItemRequestState.ItemTypeId,
					json
				});
				if (flag)
				{
					this._customizationInventoryView.OnEquipItemResponse(false);
				}
			}
		}

		private void OnEquipItemError(object state, Exception exception)
		{
			CustomizationInventoryComponent.Log.ErrorFormat("Error trying to equip item: {0}", new object[]
			{
				exception.ToString()
			});
			this._customizationInventoryView.OnEquipItemResponse(false);
		}

		public int MarkItemAsSeen(CustomizationInventoryCellItemData itemData)
		{
			bool isNew = itemData.IsNew;
			int result = 0;
			if (isNew)
			{
				itemData.IsNew = false;
				long[] array = new long[]
				{
					itemData.ItemId
				};
				CustomizationInventoryCustomWS.MarkItemAsSeen(array, array, new SwordfishClientApi.ParameterizedCallback<string>(this.OnMarkItemAsSeenSuccess), new SwordfishClientApi.ErrorCallback(this.OnMarkItemAsSeenError));
			}
			CustomizationInventoryCategoryData customizationInventoryCategoryData;
			if (this._categoryInventory.TryGetValue(itemData.ItemCategoryId, out customizationInventoryCategoryData))
			{
				if (isNew)
				{
					customizationInventoryCategoryData.NewItemsCount--;
				}
				result = customizationInventoryCategoryData.NewItemsCount;
			}
			else
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Failed to mark item as seen. No category data found with ID {0}", new object[]
				{
					itemData.ItemCategoryId
				});
			}
			this.UpdateHasNewItems();
			return result;
		}

		private void UpdateHasNewItems()
		{
			for (int i = 0; i < this._categoriesIds.Count; i++)
			{
				Guid key = this._categoriesIds[i];
				CustomizationInventoryCategoryData customizationInventoryCategoryData;
				if (this._categoryInventory.TryGetValue(key, out customizationInventoryCategoryData) && customizationInventoryCategoryData.NewItemsCount > 0)
				{
					if (!this._hasNewItems)
					{
						this._hasNewItems = true;
						this.TriggerHasNewItemsStateChangedCallback();
					}
					return;
				}
			}
			if (!this._hasNewItems)
			{
				return;
			}
			this._hasNewItems = false;
			this.TriggerHasNewItemsStateChangedCallback();
		}

		public bool MarkAllItemsAsSeen(Guid[] categoriesWithNewIds)
		{
			bool result = true;
			List<long> list = new List<long>();
			foreach (Guid guid in categoriesWithNewIds)
			{
				CustomizationInventoryCategoryData customizationInventoryCategoryData;
				if (!this._categoryInventory.TryGetValue(guid, out customizationInventoryCategoryData))
				{
					CustomizationInventoryComponent.Log.ErrorFormat("Failed to mark all items as seen. No category data found for category {0}", new object[]
					{
						guid
					});
					result = false;
				}
				if (customizationInventoryCategoryData.NewItemsCount != 0)
				{
					list.Capacity += customizationInventoryCategoryData.NewItemsCount;
					for (int j = 0; j < customizationInventoryCategoryData.Items.Count; j++)
					{
						if (customizationInventoryCategoryData.Items[j].IsNew)
						{
							customizationInventoryCategoryData.Items[j].IsNew = false;
							list.Add(customizationInventoryCategoryData.Items[j].ItemId);
						}
					}
					customizationInventoryCategoryData.NewItemsCount = 0;
				}
			}
			if (list.Count > 0)
			{
				long[] array = list.ToArray();
				CustomizationInventoryCustomWS.MarkItemAsSeen(array, array, new SwordfishClientApi.ParameterizedCallback<string>(this.OnMarkItemAsSeenSuccess), new SwordfishClientApi.ErrorCallback(this.OnMarkItemAsSeenError));
			}
			this._hasNewItems = false;
			this.TriggerHasNewItemsStateChangedCallback();
			return result;
		}

		private void TriggerHasNewItemsStateChangedCallback()
		{
			if (this.OnHasNewItemsStateChanged != null)
			{
				this.OnHasNewItemsStateChanged(this._hasNewItems);
			}
		}

		private void OnMarkItemAsSeenSuccess(object state, string json)
		{
			NetResult netResult = (NetResult)((JsonSerializeable<T>)json);
			long[] array = (long[])state;
			if (!netResult.Success)
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Failed to mark item as seen: Attemp unsuccessful. Full response={0}", new object[]
				{
					json
				});
				return;
			}
			for (int i = 0; i < array.Length; i++)
			{
				this.MarkItemAsSeenInLocalCache(array[i]);
			}
			this.TryToUpdatedSkinTabNewItems();
		}

		private void TryToUpdatedSkinTabNewItems()
		{
			for (int i = 0; i < this._categoriesIds.Count; i++)
			{
				Guid key = this._categoriesIds[i];
				CustomizationInventoryCategoryData customizationInventoryCategoryData;
				if (this._categoryInventory.TryGetValue(key, out customizationInventoryCategoryData) && customizationInventoryCategoryData.CustomizationSlot == PlayerCustomizationSlot.Skin)
				{
					if (this.UpdateSkinTabDataNewItems(customizationInventoryCategoryData) && this.OnSkinTabHasNewItemsStateChanged != null)
					{
						this.OnSkinTabHasNewItemsStateChanged();
					}
					break;
				}
			}
		}

		private void MarkItemAsSeenInLocalCache(long itemId)
		{
			Item itemById = GameHubScriptableObject.Hub.User.Inventory.GetItemById(itemId);
			if (itemById == null)
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Failed to update item bag in local cache: Item not found. ItemId={0}", new object[]
				{
					itemId
				});
				return;
			}
			itemById.Bag = new CustomizationItemTypeBag
			{
				Seen = true
			}.ToString();
		}

		private void OnMarkItemAsSeenError(object state, Exception exception)
		{
			CustomizationInventoryComponent.Log.ErrorFormat("Error trying to mark item as seen: {0}", new object[]
			{
				exception.ToString()
			});
		}

		private ItemCategoryScriptableObject GetItemCategory(Guid categoryId)
		{
			return GameHubScriptableObject.Hub.InventoryColletion.GetCategoryById(categoryId);
		}

		private void OnPlayerDataReloaded()
		{
			if (GameHubScriptableObject.Hub.User.Inventory.HasNewItems == this._hasNewItems)
			{
				return;
			}
			this._hasNewItems = GameHubScriptableObject.Hub.User.Inventory.HasNewItems;
			this.TriggerHasNewItemsStateChangedCallback();
		}

		public void OnCustomizationItemBought()
		{
			if (this._hasNewItems)
			{
				return;
			}
			this._hasNewItems = true;
			this.TriggerHasNewItemsStateChangedCallback();
		}

		public void UpdateExpandSkinState(KeyValuePair<Guid, bool> charIdExpandPair)
		{
			this._expandStateSkins[charIdExpandPair.Key] = charIdExpandPair.Value;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CustomizationInventoryComponent));

		[SerializeField]
		private string _inventorySceneName = "UI_ADD_Inventory";

		private ICustomizationInventoryView _customizationInventoryView;

		private bool _hasNewItems;

		private System.Action OnCloseInventoryCallback;

		private Dictionary<Guid, CustomizationInventoryCategoryData> _categoryInventory = new Dictionary<Guid, CustomizationInventoryCategoryData>(5);

		private Dictionary<Guid, bool> _expandStateSkins = new Dictionary<Guid, bool>(20);

		private List<Guid> _categoriesIds = new List<Guid>(5);

		private class EquipItemRequestState
		{
			public Guid CategoryId;

			public Guid ItemTypeId;
		}
	}
}
