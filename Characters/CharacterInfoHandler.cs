using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;

namespace HeavyMetalMachines.Characters
{
	public class CharacterInfoHandler
	{
		public CharacterInfoHandler(List<IItemType> characterList)
		{
			this._charactersItemTypes = characterList.ToArray();
		}

		public CharacterInfoHandler(IItemType[] charactersItemTypes)
		{
			this._charactersItemTypes = charactersItemTypes;
		}

		public Dictionary<Guid, int> CharacterTypesToId
		{
			get
			{
				return this._characterTypesToId ?? this.InitializeCharacterTypesToId();
			}
		}

		public Dictionary<int, IItemType> AllCharactersByCharacterId
		{
			get
			{
				return this._allCharactersByCharacterId ?? this.InitializeAllCharactersByCharacterId();
			}
		}

		private Dictionary<Guid, int> InitializeCharacterTypesToId()
		{
			this._characterTypesToId = new Dictionary<Guid, int>(this._charactersItemTypes.Length);
			for (int i = 0; i < this._charactersItemTypes.Length; i++)
			{
				IItemType itemType = this._charactersItemTypes[i];
				CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
				this._characterTypesToId[itemType.Id] = component.CharacterId;
			}
			return this._characterTypesToId;
		}

		private Dictionary<int, IItemType> InitializeAllCharactersByCharacterId()
		{
			this._allCharactersByCharacterId = new Dictionary<int, IItemType>(this._charactersItemTypes.Length);
			for (int i = 0; i < this._charactersItemTypes.Length; i++)
			{
				IItemType itemType = this._charactersItemTypes[i];
				CharacterItemTypeComponent characterItemTypeComponent;
				if (itemType.TryGetComponent<CharacterItemTypeComponent>(out characterItemTypeComponent))
				{
					this._allCharactersByCharacterId.Add(characterItemTypeComponent.CharacterId, itemType);
				}
			}
			return this._allCharactersByCharacterId;
		}

		public IItemType[] GetAvailableCharactersItemTypes()
		{
			return this._charactersItemTypes;
		}

		private readonly IItemType[] _charactersItemTypes;

		private Dictionary<Guid, int> _characterTypesToId;

		private Dictionary<int, IItemType> _allCharactersByCharacterId;
	}
}
