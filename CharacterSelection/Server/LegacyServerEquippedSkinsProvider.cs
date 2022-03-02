using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using ClientAPI.Objects;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.CharacterSelection.Server.Skins;
using HeavyMetalMachines.CharacterSelection.Skins;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish.Player;
using Hoplon.Serialization;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Server
{
	public class LegacyServerEquippedSkinsProvider : GameHubObject, IEquippedSkinsProvider
	{
		public LegacyServerEquippedSkinsProvider(IGetCharacterData getCharacterData, ICollectionScriptableObject collectionScriptableObject)
		{
			this._getCharacterData = getCharacterData;
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public List<MatchClientEquippedSkins> Get()
		{
			List<MatchClientEquippedSkins> list = new List<MatchClientEquippedSkins>();
			CharacterEquippedSkin[] allDefaultSkinsEquipped = this.GetAllDefaultSkinsEquipped();
			foreach (PlayerData playerData in GameHubObject.Hub.Players.Players)
			{
				CharacterEquippedSkin[] equippedSkins = this.GetEquippedSkins(playerData, allDefaultSkinsEquipped);
				list.Add(new MatchClientEquippedSkins
				{
					Client = playerData.ToMatchClient(),
					EquippedSkins = equippedSkins
				});
			}
			foreach (PlayerData playerData2 in GameHubObject.Hub.Players.Bots)
			{
				list.Add(new MatchClientEquippedSkins
				{
					Client = playerData2.ToMatchClient(),
					EquippedSkins = allDefaultSkinsEquipped
				});
			}
			foreach (PlayerData playerData3 in GameHubObject.Hub.Players.Narrators)
			{
				list.Add(new MatchClientEquippedSkins
				{
					Client = playerData3.ToMatchClient(),
					EquippedSkins = allDefaultSkinsEquipped
				});
			}
			return list;
		}

		private CharacterEquippedSkin[] GetAllDefaultSkinsEquipped()
		{
			List<CharacterEquippedSkin> list = new List<CharacterEquippedSkin>();
			foreach (CharacterData characterData in this._getCharacterData.GetAll())
			{
				list.Add(new CharacterEquippedSkin
				{
					CharacterId = characterData.Id,
					SkinId = this._collectionScriptableObject.GetDefaultSkin(characterData.Id).Id
				});
			}
			return list.ToArray();
		}

		private CharacterEquippedSkin[] GetEquippedSkins(PlayerData playerData, CharacterEquippedSkin[] defaultCharacterSkins)
		{
			Dictionary<Guid, Guid> playerCharacterSkins = LegacyServerEquippedSkinsProvider.CreateDefaultCharacterSkinsDictionary(defaultCharacterSkins);
			LegacyServerEquippedSkinsProvider.UpdateDictionaryWithPlayerData(playerCharacterSkins, playerData);
			return LegacyServerEquippedSkinsProvider.ConvertToEquippedSkins(playerCharacterSkins);
		}

		private static CharacterEquippedSkin[] ConvertToEquippedSkins(Dictionary<Guid, Guid> playerCharacterSkins)
		{
			List<CharacterEquippedSkin> list = new List<CharacterEquippedSkin>();
			foreach (KeyValuePair<Guid, Guid> keyValuePair in playerCharacterSkins)
			{
				list.Add(new CharacterEquippedSkin
				{
					CharacterId = keyValuePair.Key,
					SkinId = keyValuePair.Value
				});
			}
			return list.ToArray();
		}

		private static void UpdateDictionaryWithPlayerData(Dictionary<Guid, Guid> playerCharacterSkins, PlayerData playerData)
		{
			foreach (Character character in playerData.SwordfishCharacters)
			{
				CharacterBag characterBag = JsonSerializeable<CharacterBag>.Deserialize(character.Bag);
				if (playerCharacterSkins.ContainsKey(characterBag.CharacterId))
				{
					if (characterBag.Skin != Guid.Empty)
					{
						playerCharacterSkins[characterBag.CharacterId] = characterBag.Skin;
					}
				}
			}
		}

		private static Dictionary<Guid, Guid> CreateDefaultCharacterSkinsDictionary(CharacterEquippedSkin[] defaultCharacterSkins)
		{
			Dictionary<Guid, Guid> dictionary = new Dictionary<Guid, Guid>();
			foreach (CharacterEquippedSkin characterEquippedSkin in defaultCharacterSkins)
			{
				dictionary.Add(characterEquippedSkin.CharacterId, characterEquippedSkin.SkinId);
			}
			return dictionary;
		}

		private readonly IGetCharacterData _getCharacterData;

		private readonly ICollectionScriptableObject _collectionScriptableObject;
	}
}
