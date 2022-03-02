using System;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.BotAI;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Matches;

namespace HeavyMetalMachines.Match
{
	public static class PlayerDataExtension
	{
		private static T GetItemTypeComponent<T>(PlayerData playerData) where T : ItemTypeComponent
		{
			return playerData.CharacterItemType.GetComponent<T>();
		}

		public static Guid GetCharacterItemTypeId(this PlayerData playerData)
		{
			return playerData.CharacterItemType.Id;
		}

		public static int GetCharacterId(this PlayerData playerData)
		{
			return PlayerDataExtension.GetItemTypeComponent<CharacterItemTypeComponent>(playerData).CharacterId;
		}

		public static DriverRoleKind GetCharacterRole(this PlayerData playerData)
		{
			return PlayerDataExtension.GetItemTypeComponent<CharacterItemTypeComponent>(playerData).Role;
		}

		public static CharacterTarget GetCharacter(this PlayerData playerData)
		{
			return PlayerDataExtension.GetItemTypeComponent<CharacterItemTypeComponent>(playerData).Character;
		}

		public static string GetCharacterBiName(this PlayerData playerData)
		{
			return PlayerDataExtension.GetItemTypeComponent<CharacterItemTypeComponent>(playerData).BiName;
		}

		public static string GetCharacterAssetPrefix(this PlayerData playerData)
		{
			return PlayerDataExtension.GetItemTypeComponent<CharacterItemTypeComponent>(playerData).AssetPrefix;
		}

		public static string GetCharacterAssetPrefixToUpper(this PlayerData playerData)
		{
			return playerData.GetCharacterAssetPrefix().ToUpperInvariant();
		}

		public static int GetCharacterPreferredGridPosition(this PlayerData playerData)
		{
			return PlayerDataExtension.GetItemTypeComponent<CharacterItemTypeComponent>(playerData).PreferredGridPosition;
		}

		public static string GetCharacterLocalizedName(this PlayerData playerData)
		{
			string characterAssetPrefixToUpper = playerData.GetCharacterAssetPrefixToUpper();
			return Language.Get(string.Format("{0}_NAME", characterAssetPrefixToUpper), TranslationContext.CharactersBaseInfo);
		}

		public static string GetCharacterBotLocalizedName(this PlayerData playerData)
		{
			string characterAssetPrefixToUpper = playerData.GetCharacterAssetPrefixToUpper();
			return Language.Get(string.Format("{0}_BOT_NAME", characterAssetPrefixToUpper), TranslationContext.CharactersBaseInfo);
		}

		public static bool GetCharacterHasPassive(this PlayerData playerData)
		{
			return PlayerDataExtension.GetItemTypeComponent<CharacterItemTypeComponent>(playerData).HasPassive;
		}

		public static CarAudioData GetCharacterCarAudioData(this PlayerData playerData)
		{
			AudioItemTypeComponent itemTypeComponent = PlayerDataExtension.GetItemTypeComponent<AudioItemTypeComponent>(playerData);
			return (!itemTypeComponent) ? null : itemTypeComponent.CarAudioData;
		}

		public static VoiceOver GetCharacterVoiceOver(this PlayerData playerData)
		{
			AudioItemTypeComponent itemTypeComponent = PlayerDataExtension.GetItemTypeComponent<AudioItemTypeComponent>(playerData);
			return (!itemTypeComponent) ? null : itemTypeComponent.VoiceOver;
		}

		public static int GetCharacterMusicId(this PlayerData playerData)
		{
			AudioItemTypeComponent itemTypeComponent = PlayerDataExtension.GetItemTypeComponent<AudioItemTypeComponent>(playerData);
			return (!itemTypeComponent) ? 0 : itemTypeComponent.CharacterMusicId;
		}

		public static BotAIGoal GetBotAiGoal(this PlayerData playerData, BotAIGoal.BotDifficulty botDifficulty)
		{
			BotItemTypeComponent itemTypeComponent = PlayerDataExtension.GetItemTypeComponent<BotItemTypeComponent>(playerData);
			if (!itemTypeComponent)
			{
				return null;
			}
			switch (botDifficulty)
			{
			case BotAIGoal.BotDifficulty.Invalid:
				return null;
			case BotAIGoal.BotDifficulty.Easy:
				return itemTypeComponent.GoalEasy;
			case BotAIGoal.BotDifficulty.Medium:
				return itemTypeComponent.GoalMedium;
			case BotAIGoal.BotDifficulty.Hard:
				return itemTypeComponent.GoalHard;
			default:
				return null;
			}
		}

		public static MatchClient ToMatchClient(this PlayerData playerData)
		{
			MatchClient result = default(MatchClient);
			result.PlayerId = playerData.PlayerId;
			result.Team = ((!playerData.IsNarrator) ? playerData.Team.GetMatchTeam() : 2);
			result.IsBot = playerData.IsBot;
			result.BotId = playerData.BotId;
			result.PlayerName = playerData.Name;
			result.PublisherId = playerData.PublisherId;
			result.PublisherUserName = playerData.PublisherUserName;
			result.PlayerTag = playerData.PlayerTag;
			result.UniversalId = playerData.UserId;
			return result;
		}
	}
}
