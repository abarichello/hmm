using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.DataTransferObjects.Inventory;
using Hoplon.Serialization;
using Pocketverse;

namespace HeavyMetalMachines.Customizations.Skins
{
	public class GetLocalPlayerAvailableSkins : IGetLocalPlayerAvailableSkins
	{
		public GetLocalPlayerAvailableSkins(IConfigLoader configLoader, CollectionScriptableObject collectionScriptableObject, PlayerInventory playerInventory)
		{
			this._configLoader = configLoader;
			this._collectionScriptableObject = collectionScriptableObject;
			this._playerInventory = playerInventory;
		}

		public List<SkinData> Get(Guid characterId)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = this._collectionScriptableObject.AllItemTypes[characterId];
			CharacterItemTypeBag characterItemTypeBag = (CharacterItemTypeBag)((JsonSerializeable<!0>)itemTypeScriptableObject.Bag);
			Guid defaultSkinGuid = characterItemTypeBag.DefaultSkinGuid;
			List<SkinData> list = new List<SkinData>();
			List<Guid> list2 = this._collectionScriptableObject.CharacterToSkinGuids[itemTypeScriptableObject.Id];
			for (int i = 0; i < list2.Count; i++)
			{
				ItemTypeScriptableObject skinItemType = this._collectionScriptableObject.AllItemTypes[list2[i]];
				if (this.IsSkinValid(skinItemType, defaultSkinGuid))
				{
					list.Add(this.CreateSkinDataItem(skinItemType));
				}
			}
			return list;
		}

		private bool IsSkinValid(ItemTypeScriptableObject skinItemType, Guid defaultSkinId)
		{
			return !skinItemType.Deleted && skinItemType.ContainsComponent<InventoryItemTypeComponent>() && (defaultSkinId == skinItemType.Id || this.IsSkinBought(skinItemType.Id));
		}

		private bool IsSkinBought(Guid id)
		{
			return this._configLoader.GetBoolValue(ConfigAccess.SkipSwordfish) || this._playerInventory.HasItemOfType(id);
		}

		private SkinData CreateSkinDataItem(ItemTypeScriptableObject skinItemType)
		{
			SkinPrefabItemTypeComponent component = skinItemType.GetComponent<SkinPrefabItemTypeComponent>();
			InventoryItemTypeComponent component2 = skinItemType.GetComponent<InventoryItemTypeComponent>();
			return new SkinData
			{
				Id = skinItemType.Id,
				NameDraft = component.CardSkinDraft,
				IconName = skinItemType.Name,
				TierKind = component.Tier,
				Preview3DAssetName = component2.InventoryPreviewName
			};
		}

		private readonly IConfigLoader _configLoader;

		private readonly CollectionScriptableObject _collectionScriptableObject;

		private readonly PlayerInventory _playerInventory;
	}
}
