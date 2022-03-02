using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;

namespace HeavyMetalMachines.Customizations.Skins
{
	public class FakeGetLocalPlayerAvailableSkins : IGetLocalPlayerAvailableSkins
	{
		public FakeGetLocalPlayerAvailableSkins(CollectionScriptableObject collectionScriptableObject)
		{
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public List<SkinData> Get(Guid characterId)
		{
			ItemTypeScriptableObject itemTypeScriptableObject = this._collectionScriptableObject.AllItemTypes[characterId];
			List<SkinData> list = new List<SkinData>();
			List<Guid> list2 = this._collectionScriptableObject.CharacterToSkinGuids[itemTypeScriptableObject.Id];
			for (int i = 0; i < list2.Count; i++)
			{
				ItemTypeScriptableObject skinItemType = this._collectionScriptableObject.AllItemTypes[list2[i]];
				if (this.IsSkinValid(skinItemType))
				{
					list.Add(this.CreateSkinDataItem(skinItemType));
				}
			}
			return list;
		}

		private bool IsSkinValid(ItemTypeScriptableObject skinItemType)
		{
			return !skinItemType.Deleted && skinItemType.ContainsComponent<InventoryItemTypeComponent>();
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

		private readonly CollectionScriptableObject _collectionScriptableObject;
	}
}
