using System;
using System.Collections.Generic;
using System.Linq;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.DataTransferObjects.Util;

namespace HeavyMetalMachines.Characters
{
	public class GetCharacterData : IGetCharacterData
	{
		public GetCharacterData(ICollectionScriptableObject collectionScriptableObject)
		{
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public CharacterData[] GetAll()
		{
			return this._collectionScriptableObject.GetItemsFromCategoryId(InventoryMapper.CharactersCategoryGuid).Select(new Func<IItemType, CharacterData>(this.ConvertToCharacterData)).ToArray<CharacterData>();
		}

		public CharacterData Get(Guid characterId)
		{
			IItemType itemType = this._collectionScriptableObject.Get(characterId);
			return this.ConvertToCharacterData(itemType);
		}

		private CharacterData ConvertToCharacterData(IItemType itemType)
		{
			CharacterData characterData;
			if (GetCharacterData._characterCache.TryGetValue(itemType.Id, out characterData))
			{
				return characterData;
			}
			CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
			PickModeStats pickModeStats = component.PickModeStats;
			CombatStats combatStats = default(CombatStats);
			combatStats.Durability = pickModeStats.Durability;
			combatStats.Damage = pickModeStats.Damage;
			combatStats.Repair = pickModeStats.Repair;
			combatStats.Control = pickModeStats.Control;
			combatStats.Mobility = pickModeStats.Mobility;
			CombatStats combatStats2 = combatStats;
			characterData = new CharacterData
			{
				Id = itemType.Id,
				NameDraft = component.NameDraft,
				Role = component.Role,
				SmallIconName = component.CharacterIcon64Name,
				RoundLookRightIconName = component.Round128LookRightIconName,
				RoundLookLeftIconName = component.Round128LookLeftIconName,
				SquaredLookRightIconName = component.Squared128LookRightIconName,
				SquaredLookLeftIconName = component.Squared128LookLeftIconName,
				CombatStats = combatStats2
			};
			GetCharacterData._characterCache.Add(itemType.Id, characterData);
			return characterData;
		}

		private readonly ICollectionScriptableObject _collectionScriptableObject;

		private static readonly Dictionary<Guid, CharacterData> _characterCache = new Dictionary<Guid, CharacterData>(24);
	}
}
