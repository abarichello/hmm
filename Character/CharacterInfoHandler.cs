using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;

namespace HeavyMetalMachines.Character
{
	public class CharacterInfoHandler
	{
		public CharacterInfoHandler(List<ItemTypeScriptableObject> characterList)
		{
			List<CharacterInfo> list = new List<CharacterInfo>();
			for (int i = 0; i < characterList.Count; i++)
			{
				if (characterList[i].IsActive)
				{
					CharacterItemTypeComponent component = characterList[i].GetComponent<CharacterItemTypeComponent>();
					list.Add(component.MainAttributes);
				}
			}
			this._characters = list.ToArray();
		}

		public CharacterInfoHandler(CharacterInfo[] characterList)
		{
			this._characters = characterList;
		}

		public Dictionary<Guid, int> CharacterTypesToId
		{
			get
			{
				return this._characterTypesToId ?? this.InitializeCharacterTypesToId();
			}
		}

		public Dictionary<Guid, CharacterInfo> CharactersByTypeId
		{
			get
			{
				return this._charactersByTypeId ?? this.InitializeCharactersByTypeId();
			}
		}

		public Dictionary<int, CharacterInfo> AvailableCharactersByInfoId
		{
			get
			{
				return this._availableCharactersByInfoId ?? this.InitializeAvailableCharactersByInfoId();
			}
		}

		public Dictionary<int, CharacterInfo> AllCharactersByInfoId
		{
			get
			{
				return this._allCharactersByInfoId ?? this.InitializeAllCharactersByInfoId();
			}
		}

		private Dictionary<Guid, int> InitializeCharacterTypesToId()
		{
			this._characterTypesToId = new Dictionary<Guid, int>(this._characters.Length);
			for (int i = 0; i < this._characters.Length; i++)
			{
				CharacterInfo characterInfo = this._characters[i];
				this._characterTypesToId[characterInfo.CharacterItemTypeGuid] = characterInfo.CharacterId;
			}
			return this._characterTypesToId;
		}

		private Dictionary<Guid, CharacterInfo> InitializeCharactersByTypeId()
		{
			this._charactersByTypeId = new Dictionary<Guid, CharacterInfo>(this._characters.Length);
			for (int i = 0; i < this._characters.Length; i++)
			{
				CharacterInfo characterInfo = this._characters[i];
				this._charactersByTypeId[characterInfo.CharacterItemTypeGuid] = characterInfo;
			}
			return this._charactersByTypeId;
		}

		private Dictionary<int, CharacterInfo> InitializeAvailableCharactersByInfoId()
		{
			this._availableCharactersByInfoId = new Dictionary<int, CharacterInfo>(this._characters.Length);
			for (int i = 0; i < this._characters.Length; i++)
			{
				CharacterInfo characterInfo = this._characters[i];
				this._availableCharactersByInfoId[characterInfo.CharacterId] = characterInfo;
			}
			return this._availableCharactersByInfoId;
		}

		private Dictionary<int, CharacterInfo> InitializeAllCharactersByInfoId()
		{
			this._allCharactersByInfoId = new Dictionary<int, CharacterInfo>(this._characters.Length);
			for (int i = 0; i < this._characters.Length; i++)
			{
				CharacterInfo characterInfo = this._characters[i];
				this._allCharactersByInfoId[characterInfo.CharacterId] = characterInfo;
			}
			return this._allCharactersByInfoId;
		}

		public CharacterInfo[] GetAvailableCharacterInfos()
		{
			return this._characters;
		}

		private readonly CharacterInfo[] _characters;

		private Dictionary<Guid, int> _characterTypesToId;

		private Dictionary<Guid, CharacterInfo> _charactersByTypeId;

		private Dictionary<int, CharacterInfo> _availableCharactersByInfoId;

		private Dictionary<int, CharacterInfo> _allCharactersByInfoId;
	}
}
