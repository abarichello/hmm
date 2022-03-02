using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Assets.Customization;
using ClientAPI;
using ClientAPI.Objects;
using HeavyMetalMachines.Customization.Business;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using HeavyMetalMachines.DataTransferObjects.Result;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Items.DataTransferObjects;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Customization
{
	[CreateAssetMenu(menuName = "UnityUI/CustomizationInventoryComponent")]
	public class CustomizationInventoryComponent : GameHubScriptableObject, IGetCustomizationChange, IGetCustomizationHoverChange
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ItemEquippedEventHandler OnReceivedItemEquipChangedCallback;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> OnHasNewItemsStateChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnSkinTabHasNewItemsStateChanged;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<Guid, int> OnCategoryItemSeenCountChanged;

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
			this._onItemEquipChangedSubject = new Subject<ItemChangeRequestState>();
			this._onItemHoverChangedSubject = new Subject<CustomizationInventoryCellItemData>();
		}

		private void OnDisable()
		{
			MainMenu.PlayerReloadedEvent -= this.OnPlayerDataReloaded;
			this._onItemEquipChangedSubject = null;
			this._onItemHoverChangedSubject = null;
		}

		[Obsolete("Use MainMenuTree.InventoryNode")]
		public void LoadInventoryScene(Action onCloseInventoryCallback)
		{
			CustomizationInventoryComponent.Log.Warn("Obsolete LoadInventoryScene. Use MainMenuTree.InventoryNode");
		}

		public void RegisterView(ICustomizationInventoryView view)
		{
			this._customizationInventoryView = view;
		}

		public void ShowCustomizationInventoryWindow()
		{
			if (!this._customizationInventoryView.IsVisible())
			{
				this._customizationInventoryView.SetVisibility(true, false);
			}
		}

		[Obsolete("Use MainMenuPresenterTree")]
		public void HideCustomizationInventoryWindow(bool imediate = false)
		{
			CustomizationInventoryComponent.Log.Warn("Obsolete HideCustomizationInventoryWindow. Use MainMenuPresenterTree");
		}

		public void CustomizationWindowUnloaded()
		{
			this._categoryInventory.Clear();
			this._customizationInventoryView = null;
		}

		public CustomizationInventoryCellItemData GetItem(Guid itemTypeId)
		{
			ItemTypeScriptableObject itemTypeScriptableObject;
			try
			{
				itemTypeScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[itemTypeId];
			}
			catch (KeyNotFoundException ex)
			{
				CustomizationInventoryComponent.Log.ErrorFormat("keyNotFoundException. id={0}, ex={1}", new object[]
				{
					itemTypeId,
					ex
				});
				return null;
			}
			catch (Exception ex2)
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Exception. id={0}, ex={1}", new object[]
				{
					itemTypeId,
					ex2
				});
				return null;
			}
			ItemCategoryScriptableObject itemCategoryScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemCategories[itemTypeScriptableObject.ItemCategoryId];
			Guid id = itemCategoryScriptableObject.Id;
			CustomizationInventoryCategoryData customizationInventoryCategoryData;
			CustomizationInventoryCellItemData result;
			if (this._categoryInventory.TryGetValue(id, out customizationInventoryCategoryData) && customizationInventoryCategoryData.ItemsDictionary.TryGetValue(itemTypeId, out result))
			{
				return result;
			}
			return null;
		}

		public CustomizationInventoryCategoryData GetCategoryData(Guid categoryId)
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
			string categoryName = (!itemCategory) ? "Unnamed category" : itemCategory.LocalizedName;
			IGetCustomizationSlot customizationSlotSelector;
			if (itemCategory.CustomizationSlots.Count > 1)
			{
				customizationSlotSelector = new MultipleCustomizationSlotSelector(itemCategory, GameHubScriptableObject.Hub.User.Inventory.Customizations);
			}
			else if (itemCategory.CustomizationSlots.Count == 1)
			{
				customizationSlotSelector = new SingleCustomizationSlotSelector(itemCategory);
			}
			else
			{
				customizationSlotSelector = new StandardCustomizationSlotSelector();
			}
			CustomizationInventoryCategoryData customizationInventoryCategoryData = new CustomizationInventoryCategoryData(categoryId, categoryName, itemCategory.CustomizationSlots, customizationSlotSelector);
			customizationInventoryCategoryData.IsLore = (itemCategory.Name == "Lore");
			this._categoryInventory[categoryId] = customizationInventoryCategoryData;
			if (customizationInventoryCategoryData.CategoryId != InventoryMapper.SkinsCategoryGuid)
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
				this.TryToAddItemData(3, categoryId, defaultItemTypeId, customizationInventoryCategoryData);
				this.TryToAddItemData(14, categoryId, defaultItemTypeId, customizationInventoryCategoryData);
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
			InventoryAdapter inventoryAdapterByKind = GameHubScriptableObject.Hub.User.Inventory.GetInventoryAdapterByKind(3);
			if (inventoryAdapterByKind == null)
			{
				return;
			}
			List<IItemType> list = GameHubScriptableObject.Hub.InventoryColletion.CategoriesIdToItemTypes[InventoryMapper.CharactersCategoryGuid];
			list.Sort(new Comparison<IItemType>(this.CharacterListSort));
			for (int i = 0; i < list.Count; i++)
			{
				IItemType itemType = list[i];
				bool flag = GameHubScriptableObject.Hub.User.Inventory.HasItemOfType(itemType.Id);
				CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
				List<Guid> list2 = GameHubScriptableObject.Hub.InventoryColletion.CharacterToSkinGuids[itemType.Id];
				List<CustomizationInventoryCellItemData> list3 = new List<CustomizationInventoryCellItemData>(list2.Count + 1);
				CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<!0>)itemType.Bag);
				ItemTypeScriptableObject itemTypeScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[characterItemTypeBag.DefaultSkinGuid];
				if (flag)
				{
					CustomizationInventoryCellItemData customizationInventoryCellItemData = this.ConvertItemType(itemTypeScriptableObject, null);
					if (customizationInventoryCellItemData != null)
					{
						categoryData.AddItem(customizationInventoryCellItemData);
						list3.Add(customizationInventoryCellItemData);
					}
					else
					{
						CustomizationInventoryComponent.Log.WarnFormat("Missing Inventory Data for ItemType: {0}", new object[]
						{
							itemTypeScriptableObject
						});
					}
				}
				for (int j = 0; j < list2.Count; j++)
				{
					ItemTypeScriptableObject itemTypeScriptableObject2 = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[list2[j]];
					Item item;
					if (inventoryAdapterByKind.ItemTypeGuidToItem.TryGetValue(itemTypeScriptableObject2.Id, out item))
					{
						CustomizationInventoryCellItemData customizationInventoryCellItemData2 = this.ConvertItemType(itemTypeScriptableObject2, item);
						if (customizationInventoryCellItemData2 == null)
						{
							CustomizationInventoryComponent.Log.WarnFormat("Missing Inventory Data for ItemType: {0}", new object[]
							{
								itemTypeScriptableObject2
							});
						}
						else if (!(itemTypeScriptableObject != null) || !(itemTypeScriptableObject.Id == itemTypeScriptableObject2.Id))
						{
							categoryData.AddItem(customizationInventoryCellItemData2);
							list3.Add(customizationInventoryCellItemData2);
						}
					}
				}
				if (list3.Count > 0)
				{
					categoryData.AddCharacterSkinDataItems(itemType.Id, list3);
				}
			}
		}

		private int CharacterListSort(IItemType itemType1, IItemType itemType2)
		{
			CharacterItemTypeComponent component = itemType1.GetComponent<CharacterItemTypeComponent>();
			CharacterItemTypeComponent component2 = itemType2.GetComponent<CharacterItemTypeComponent>();
			string characterLocalizedName = component.GetCharacterLocalizedName();
			string characterLocalizedName2 = component2.GetCharacterLocalizedName();
			int num = characterLocalizedName.CompareTo(characterLocalizedName2);
			if (num == 0)
			{
				num = itemType1.Name.CompareTo(itemType2.Name);
			}
			return num;
		}

		public bool UpdateSkinTabDataNewItems(CustomizationInventoryCategoryData categoryData)
		{
			InventoryAdapter inventoryAdapterByKind = GameHubScriptableObject.Hub.User.Inventory.GetInventoryAdapterByKind(3);
			if (inventoryAdapterByKind == null)
			{
				return false;
			}
			List<IItemType> list = GameHubScriptableObject.Hub.InventoryColletion.CategoriesIdToItemTypes[InventoryMapper.CharactersCategoryGuid];
			for (int i = 0; i < list.Count; i++)
			{
				IItemType itemType = list[i];
				if (this.CharacterSkinHasNewItems(inventoryAdapterByKind, itemType.Id))
				{
					return true;
				}
			}
			return false;
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

		private void TryToAddItemData(InventoryBag.InventoryKind inventoryKind, Guid categoryId, Guid defaultItemTypeId, CustomizationInventoryCategoryData categoryData)
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
							categoryData.AddItem(customizationInventoryCellItemData);
						}
					}
				}
			}
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
			customizationInventoryCellItemData.IsSelected = false;
			ItemTypeComponent itemTypeComponent2;
			if (itemType.GetComponentByEnum(ItemTypeComponent.Type.SkinPrefab, out itemTypeComponent2))
			{
				customizationInventoryCellItemData.SkinPrefabComponent = (SkinPrefabItemTypeComponent)itemTypeComponent2;
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
					CustomizationItemTypeBag customizationItemTypeBag = (CustomizationItemTypeBag)((JsonSerializeable<!0>)item.Bag);
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

		public bool GetIsItemEquiped(CustomizationInventoryCellItemData itemData)
		{
			if (GameHubScriptableObject.Hub.User.Inventory.Customizations.Contains(itemData.ItemTypeId))
			{
				return true;
			}
			ItemTypeScriptableObject defaultCategoryItem = this._customizationsConfig.GetDefaultCategoryItem(itemData.ItemCategoryId);
			if (defaultCategoryItem != null && defaultCategoryItem.Id == itemData.ItemTypeId)
			{
				CustomizationInventoryCategoryData categoryData = this.GetCategoryData(itemData.ItemCategoryId);
				for (int i = 0; i < categoryData.CustomizationSlots.Count; i++)
				{
					PlayerCustomizationSlot playerCustomizationSlot = categoryData.CustomizationSlots[i];
					Guid guidBySlot = GameHubScriptableObject.Hub.User.Inventory.Customizations.GetGuidBySlot(playerCustomizationSlot);
					if (guidBySlot == Guid.Empty || guidBySlot == defaultCategoryItem.Id)
					{
						return true;
					}
				}
			}
			return false;
		}

		public bool IsEquipable(CustomizationInventoryCellItemData itemData)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[itemData.ItemTypeId];
			ItemCategoryScriptableObject itemCategoryScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemCategories[itemTypeScriptableObject.ItemCategoryId];
			Guid id = itemCategoryScriptableObject.Id;
			return !(id == InventoryMapper.SkinsCategoryGuid) && !(id == InventoryMapper.LoreCategoryGuid);
		}

		public bool IsInteractable(CustomizationInventoryCellItemData itemData)
		{
			return !this.GetIsItemEquiped(itemData);
		}

		public bool IsUnequipable(CustomizationInventoryCellItemData itemData)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[itemData.ItemTypeId];
			ItemCategoryScriptableObject itemCategoryScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemCategories[itemTypeScriptableObject.ItemCategoryId];
			Guid id = itemCategoryScriptableObject.Id;
			return id == InventoryMapper.EmoteCategoryGuid || !this.GetIsItemEquiped(itemData);
		}

		public void EquipItem(Guid itemTypeId)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[itemTypeId];
			ItemCategoryScriptableObject itemCategoryScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemCategories[itemTypeScriptableObject.ItemCategoryId];
			Guid id = itemCategoryScriptableObject.Id;
			CustomizationInventoryCellItemData item = this.GetItem(itemTypeId);
			CustomizationInventoryCategoryData categoryData = this.GetCategoryData(id);
			PlayerCustomizationSlot equippingSlot = categoryData.GetEquippingSlot();
			if (equippingSlot == null)
			{
				return;
			}
			if (item.IsDefault)
			{
				itemTypeId = Guid.Empty;
			}
			ItemChangeRequestState itemChangeRequestState = new ItemChangeRequestState
			{
				IsEquip = true,
				CategoryId = id,
				ItemTypeId = itemTypeId,
				Slot = equippingSlot
			};
			BattlepassCustomWS.SaveCustomizationsSelected(new CustomizationBagAdapter
			{
				Slot = equippingSlot,
				TypeId = itemTypeId
			}, itemChangeRequestState, new SwordfishClientApi.ParameterizedCallback<string>(this.OnEquipItemSuccess), new SwordfishClientApi.ErrorCallback(this.OnEquipItemError));
			this.UpdateItemEquip(itemChangeRequestState);
		}

		public void UnequipItem(Guid itemTypeId)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemTypes[itemTypeId];
			ItemCategoryScriptableObject itemCategoryScriptableObject = GameHubScriptableObject.Hub.InventoryColletion.AllItemCategories[itemTypeScriptableObject.ItemCategoryId];
			Guid id = itemCategoryScriptableObject.Id;
			CustomizationInventoryCategoryData categoryData = this.GetCategoryData(id);
			PlayerCustomizationSlot unequippingSlot = categoryData.GetUnequippingSlot(itemTypeId);
			if (unequippingSlot == null)
			{
				return;
			}
			ItemChangeRequestState itemChangeRequestState = new ItemChangeRequestState
			{
				IsEquip = false,
				CategoryId = id,
				ItemTypeId = itemTypeId,
				Slot = unequippingSlot
			};
			BattlepassCustomWS.SaveCustomizationsSelected(new CustomizationBagAdapter
			{
				Slot = unequippingSlot,
				TypeId = Guid.Empty
			}, itemChangeRequestState, new SwordfishClientApi.ParameterizedCallback<string>(this.OnEquipItemSuccess), new SwordfishClientApi.ErrorCallback(this.OnEquipItemError));
			this.UpdateItemEquip(itemChangeRequestState);
		}

		private void RaiseItemEquipped(ItemChangeRequestState state)
		{
			this._onItemEquipChangedSubject.OnNext(state);
		}

		private void OnEquipItemSuccess(object state, string json)
		{
			ItemChangeRequestState itemChangeRequestState = state as ItemChangeRequestState;
			bool flag = null != this._customizationInventoryView;
			if (itemChangeRequestState == null)
			{
				CustomizationInventoryComponent.Log.Error("Failed to equip item: No state object received.");
				if (flag)
				{
					this.OnEquipItemResponse(false, itemChangeRequestState);
				}
				return;
			}
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)json);
			if (netResult.Success)
			{
				CustomizationInventoryComponent.Log.DebugFormat("Item equipped successfully. CategoryId={0} ItemTypeId={1}. Full response={2}", new object[]
				{
					itemChangeRequestState.CategoryId,
					itemChangeRequestState.ItemTypeId,
					json
				});
				CustomizationInventoryCategoryData customizationInventoryCategoryData;
				if (!this._categoryInventory.TryGetValue(itemChangeRequestState.CategoryId, out customizationInventoryCategoryData))
				{
					CustomizationInventoryComponent.Log.ErrorFormat("Failed to mark item as equipped: No category data found with ID {0}", new object[]
					{
						itemChangeRequestState.CategoryId
					});
					if (flag)
					{
						this.OnEquipItemResponse(false, itemChangeRequestState);
					}
					return;
				}
				if (itemChangeRequestState.Slot == null)
				{
					CustomizationInventoryComponent.Log.ErrorFormat("Failed to mark item as equipped: Unable to find category slot with ID {0}", new object[]
					{
						itemChangeRequestState.CategoryId
					});
					if (flag)
					{
						this.OnEquipItemResponse(false, itemChangeRequestState);
					}
					return;
				}
				this.UpdateItemEquip(itemChangeRequestState);
				if (flag)
				{
					this.OnEquipItemResponse(true, itemChangeRequestState);
				}
			}
			else
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Failed to equip item: Attempt unsuccessful. CategoryId={0} ItemTypeId={1}. Full response={2}", new object[]
				{
					itemChangeRequestState.CategoryId,
					itemChangeRequestState.ItemTypeId,
					json
				});
				if (flag)
				{
					this.OnEquipItemResponse(false, itemChangeRequestState);
				}
			}
		}

		private void OnEquipItemResponse(bool success, ItemChangeRequestState data)
		{
			this._customizationInventoryView.OnEquipItemResponse(success);
			if (this.OnReceivedItemEquipChangedCallback != null)
			{
				this.OnReceivedItemEquipChangedCallback(data);
			}
		}

		private void UpdateItemEquip(ItemChangeRequestState data)
		{
			if (data.IsEquip)
			{
				GameHubScriptableObject.Hub.User.Inventory.Customizations.SetGuidAndSlot(data.Slot, data.ItemTypeId);
			}
			else
			{
				GameHubScriptableObject.Hub.User.Inventory.Customizations.SetGuidAndSlot(data.Slot, Guid.Empty);
			}
			this.RaiseItemEquipped(data);
		}

		private void OnEquipItemError(object state, Exception exception)
		{
			CustomizationInventoryComponent.Log.ErrorFormat("Error trying to equip item: {0}", new object[]
			{
				exception.ToString()
			});
			this.OnEquipItemResponse(false, (ItemChangeRequestState)state);
		}

		public void MarkItemAsSeen(CustomizationInventoryCellItemData itemData)
		{
			bool isNew = itemData.IsNew;
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
				this.TriggerItemSeenChanged(customizationInventoryCategoryData.CategoryId, customizationInventoryCategoryData.NewItemsCount);
			}
			else
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Failed to mark item as seen. No category data found with ID {0}", new object[]
				{
					itemData.ItemCategoryId
				});
			}
			this.UpdateHasNewItems();
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

		private void TriggerItemSeenChanged(Guid categoryId, int count)
		{
			if (this.OnCategoryItemSeenCountChanged != null)
			{
				this.OnCategoryItemSeenCountChanged(categoryId, count);
			}
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
			NetResult netResult = (NetResult)((JsonSerializeable<!0>)json);
			long[] array = (long[])state;
			if (!netResult.Success)
			{
				CustomizationInventoryComponent.Log.ErrorFormat("Failed to mark item as seen: Attemp unsuccessful. Full response={0}", new object[]
				{
					json
				});
				return;
			}
			CustomizationInventoryComponent.Log.DebugFormat("Items marked as seen successfully. Will try to update local cache. Full response={0}", new object[]
			{
				json
			});
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
				if (this._categoryInventory.TryGetValue(key, out customizationInventoryCategoryData) && !(customizationInventoryCategoryData.CategoryId != InventoryMapper.SkinsCategoryGuid))
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
			CustomizationInventoryComponent.Log.DebugFormat("Item bag updated in local cache. ItemId={0}", new object[]
			{
				itemId
			});
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

		public void RaiseItemHoverChange(CustomizationInventoryCellItemData itemData)
		{
			this._onItemHoverChangedSubject.OnNext(itemData);
		}

		public IObservable<ItemChangeRequestState> OnItemEquipChanged
		{
			get
			{
				return this._onItemEquipChangedSubject;
			}
		}

		public IObservable<CustomizationInventoryCellItemData> ObserveHoverChange
		{
			get
			{
				return this._onItemHoverChangedSubject;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CustomizationInventoryComponent));

		[SerializeField]
		private string _inventorySceneName = "UI_ADD_Inventory";

		private ICustomizationInventoryView _customizationInventoryView;

		private Subject<ItemChangeRequestState> _onItemEquipChangedSubject;

		private Subject<CustomizationInventoryCellItemData> _onItemHoverChangedSubject;

		private bool _hasNewItems;

		private Action OnCloseInventoryCallback;

		private Dictionary<Guid, CustomizationInventoryCategoryData> _categoryInventory = new Dictionary<Guid, CustomizationInventoryCategoryData>(5);

		private List<Guid> _categoriesIds = new List<Guid>(5);

		[SerializeField]
		private CustomizationAssetsScriptableObject _customizationsConfig;

		[Inject]
		private DiContainer _diContainer;
	}
}
