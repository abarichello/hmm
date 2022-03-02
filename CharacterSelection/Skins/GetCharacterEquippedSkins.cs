using System;
using System.Linq;
using Assets.ClientApiObjects;
using HeavyMetalMachines.CharacterSelection.Server.Skins;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Matches;

namespace HeavyMetalMachines.CharacterSelection.Skins
{
	public class GetCharacterEquippedSkins : IGetCharacterEquippedSkins
	{
		public GetCharacterEquippedSkins(ICollectionScriptableObject collectionScriptableObject)
		{
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public GetCharacterEquippedSkins(EquippedSkinsStorage storage)
		{
			this._storage = storage;
		}

		public CharacterEquippedSkin[] Get(MatchClient matchClient)
		{
			if (matchClient.IsBot || matchClient.Team == 2)
			{
				this._collectionScriptableObject.GetItemsFromCategoryId(InventoryMapper.CharactersCategoryGuid).Select(new Func<IItemType, CharacterEquippedSkin>(this.ConvertToCharacterEquippedSkin)).ToArray<CharacterEquippedSkin>();
			}
			return this._storage.GetEquippedSkins(matchClient).ToArray();
		}

		private CharacterEquippedSkin ConvertToCharacterEquippedSkin(IItemType itemType)
		{
			return new CharacterEquippedSkin
			{
				CharacterId = itemType.ItemCategoryId,
				SkinId = this._collectionScriptableObject.GetDefaultSkin(itemType.ItemCategoryId).Id
			};
		}

		private readonly ICollectionScriptableObject _collectionScriptableObject;

		private readonly EquippedSkinsStorage _storage;
	}
}
