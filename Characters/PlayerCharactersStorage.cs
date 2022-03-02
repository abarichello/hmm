using System;
using System.Collections.Generic;
using System.Linq;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI.Objects;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Swordfish.Player;
using Hoplon.Serialization;
using Pocketverse;

namespace HeavyMetalMachines.Characters
{
	public class PlayerCharactersStorage : IPlayerCharactersStorage
	{
		public PlayerCharactersStorage(UserInfo userInfo, SharedConfigs sharedConfigs, IGetCharacterRole getCharacterRole, ICollectionScriptableObject collectionScriptableObject)
		{
			this._userInfo = userInfo;
			this._sharedConfigs = sharedConfigs;
			this._getCharacterRole = getCharacterRole;
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public PlayerCharacter[] PlayerCharacters
		{
			get
			{
				return this._userInfo.Characters.Select(new Func<Character, PlayerCharacter>(this.ConvertToPlayerCharacter)).ToArray<PlayerCharacter>();
			}
		}

		private PlayerCharacter ConvertToPlayerCharacter(Character clientApiCharacter)
		{
			CharacterBag characterBag = (CharacterBag)((JsonSerializeable<!0>)clientApiCharacter.Bag);
			IItemType itemType = this._collectionScriptableObject.Get(characterBag.CharacterId);
			CharacterItemTypeComponent component = itemType.GetComponent<CharacterItemTypeComponent>();
			int characterLevelZeroBased = this.GetCharacterLevelZeroBased(characterBag);
			float characterLevelExperienceNormalized = this.GetCharacterLevelExperienceNormalized(characterLevelZeroBased, characterBag);
			PlayerCharacter playerCharacter = new PlayerCharacter
			{
				Matches = characterBag.MatchesCount,
				Victories = characterBag.WinsCount,
				Defeats = characterBag.DefeatsCount,
				Kills = characterBag.KillsCount,
				Deaths = characterBag.DeathsCount,
				Damage = characterBag.TotalDamage,
				Repair = characterBag.TotalRepair,
				SpeedBoosts = characterBag.SpeedBoostCount,
				TackledBombs = characterBag.BombStolenCount,
				DroppedBombs = characterBag.BombLostCount,
				DeliveredBombs = characterBag.BombDeliveredCount,
				TravelledDistance = characterBag.TravelledDistance,
				Role = this._getCharacterRole.Get(characterBag.CharacterId),
				CharacterId = component.CharacterId,
				VictoryRatioToNextLevel = this.GetVictoryRatioToNextLevel(characterBag.Xp, characterLevelZeroBased),
				LocalizedCharacterName = component.GetCharacterLocalizedName(),
				CharacterIcon128 = string.Format("{0}_icon_char_128", component.AssetPrefix),
				CharacterLevelOneBased = characterLevelZeroBased + 1,
				CharacterLevelExperienceNormalized = characterLevelExperienceNormalized
			};
			playerCharacter.Unlockables = this.GetRewards(playerCharacter.CharacterLevelOneBased - 1).ToArray<PlayerCharacterUnlockable>();
			return playerCharacter;
		}

		private IEnumerable<PlayerCharacterUnlockable> GetRewards(int characterLevelZeroBased)
		{
			ProgressionInfo.Level[] levels = this._sharedConfigs.CharacterProgression.Levels;
			for (int level = 0; level < levels.Length; level++)
			{
				if (levels[level].Kind != null)
				{
					string spriteName;
					HudUtils.TryToGetPlayerUnlockRewardIconSpriteName(this._sharedConfigs.CharacterProgression, level, out spriteName);
					string rewardName;
					HudUtils.TryToGetUnlockRewardName(this._sharedConfigs.CharacterProgression, level, out rewardName);
					PlayerCharacterUnlockable reward = new PlayerCharacterUnlockable
					{
						Title = rewardName,
						SpriteName = spriteName,
						IsUnlocked = (characterLevelZeroBased >= level),
						UnlockLevel = level + 1
					};
					yield return reward;
				}
			}
			yield break;
		}

		private string GetVictoryRatioToNextLevel(int overallXp, int zeroBasedLevel)
		{
			int num;
			int num2;
			this._sharedConfigs.CharacterProgression.GetXpForSegment(overallXp, zeroBasedLevel, ref num, ref num2);
			return string.Format("{0}/{1}", num, num2);
		}

		private int GetCharacterLevelZeroBased(CharacterBag characterBag)
		{
			return this._sharedConfigs.CharacterProgression.GetLevelForXP(characterBag.Xp);
		}

		private float GetCharacterLevelExperienceNormalized(int levelZeroBased, CharacterBag characterBag)
		{
			return HudUtils.GetNormalizedLevelInfo(this._sharedConfigs.CharacterProgression, levelZeroBased, characterBag.Xp);
		}

		private readonly UserInfo _userInfo;

		private readonly SharedConfigs _sharedConfigs;

		private readonly IGetCharacterRole _getCharacterRole;

		private readonly ICollectionScriptableObject _collectionScriptableObject;
	}
}
