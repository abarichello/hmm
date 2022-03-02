using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;

namespace HeavyMetalMachines.Characters
{
	public class GetCharacterSkinData : IGetCharacterSkinData
	{
		public GetCharacterSkinData(CollectionScriptableObject collectionScriptableObject)
		{
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public CharacterSkinData GetDefaultSkin(Guid characterId)
		{
			ItemTypeScriptableObject defaultSkin = this._collectionScriptableObject.GetDefaultSkin(characterId);
			return this.CreateSkinData(characterId, defaultSkin);
		}

		public CharacterSkinData GetCharacterSkin(Guid characterId, Guid skinId)
		{
			ItemTypeScriptableObject skinItemType = this._collectionScriptableObject.AllItemTypes[skinId];
			return this.CreateSkinData(characterId, skinItemType);
		}

		private CharacterSkinData CreateSkinData(Guid characterId, ItemTypeScriptableObject skinItemType)
		{
			InventoryItemTypeComponent component = skinItemType.GetComponent<InventoryItemTypeComponent>();
			return new CharacterSkinData
			{
				CharacterId = characterId,
				Preview3DAssetName = component.InventoryPreviewName
			};
		}

		private readonly CollectionScriptableObject _collectionScriptableObject;
	}
}
