using System;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;

namespace HeavyMetalMachines.Frontend
{
	public class FoundersBoosterGui
	{
		public static bool HasFounderBooster(PlayerData playerData)
		{
			return !playerData.IsBot && FoundersBoosterGui.HasFounderBooster(playerData.FounderLevel);
		}

		public static bool HasFounderBooster(FounderLevel founderLevel)
		{
			return FounderLevelEx.CheckHasFlag(founderLevel, 4) || FounderLevelEx.CheckHasFlag(founderLevel, 2) || FounderLevelEx.CheckHasFlag(founderLevel, 1);
		}

		public static string GetSpriteName(FounderLevel founderLevel, FoundersBoosterGui.SpriteType spriteType)
		{
			string result = string.Empty;
			if (FounderLevelEx.CheckHasFlag(founderLevel, 4))
			{
				switch (spriteType)
				{
				case FoundersBoosterGui.SpriteType.Corner:
					result = "booster_founder_border_profile_gold";
					break;
				case FoundersBoosterGui.SpriteType.Circle:
					result = "booster_founder_border_circular_gold";
					break;
				case FoundersBoosterGui.SpriteType.LoadingVersusIcon:
				case FoundersBoosterGui.SpriteType.MainMenuGroupIcon:
					result = "booster_gold_loading_small";
					break;
				case FoundersBoosterGui.SpriteType.LoadingVersusBox:
					result = "booster_founder_border_loading_gold";
					break;
				case FoundersBoosterGui.SpriteType.MainMenuName:
					result = "booster_founder_name_gold_edition";
					break;
				case FoundersBoosterGui.SpriteType.MainMenuBooster:
					result = "boost_founder_gold_topgroup";
					break;
				}
			}
			else if (FounderLevelEx.CheckHasFlag(founderLevel, 2))
			{
				switch (spriteType)
				{
				case FoundersBoosterGui.SpriteType.Corner:
					result = "booster_founder_border_profile_silver";
					break;
				case FoundersBoosterGui.SpriteType.Circle:
					result = "booster_founder_border_circular_silver";
					break;
				case FoundersBoosterGui.SpriteType.LoadingVersusIcon:
				case FoundersBoosterGui.SpriteType.MainMenuGroupIcon:
					result = "booster_silver_loading_small";
					break;
				case FoundersBoosterGui.SpriteType.LoadingVersusBox:
					result = "booster_founder_border_loading_silver";
					break;
				case FoundersBoosterGui.SpriteType.MainMenuName:
					result = "booster_founder_name_silver_edition";
					break;
				case FoundersBoosterGui.SpriteType.MainMenuBooster:
					result = "boost_founder_silver_topgroup";
					break;
				}
			}
			else if (FounderLevelEx.CheckHasFlag(founderLevel, 1))
			{
				switch (spriteType)
				{
				case FoundersBoosterGui.SpriteType.Corner:
					result = "booster_founder_border_profile_cooper";
					break;
				case FoundersBoosterGui.SpriteType.Circle:
					result = "booster_founder_border_circular_cooper";
					break;
				case FoundersBoosterGui.SpriteType.LoadingVersusIcon:
				case FoundersBoosterGui.SpriteType.MainMenuGroupIcon:
					result = "booster_cooper_loading_small";
					break;
				case FoundersBoosterGui.SpriteType.LoadingVersusBox:
					result = "booster_founder_border_loading_cooper";
					break;
				case FoundersBoosterGui.SpriteType.MainMenuName:
					result = "booster_founder_name_cooper_edition";
					break;
				case FoundersBoosterGui.SpriteType.MainMenuBooster:
					result = "boost_founder_cooper_topgroup";
					break;
				}
			}
			else
			{
				Debug.Log.ErrorFormat("FoundersBoosterGui.GetSpriteName - Unexpected founder level to get sprite name:[{0}]", new object[]
				{
					founderLevel
				});
			}
			return result;
		}

		public static string GetDescription(FounderLevel founderLevel)
		{
			string key = string.Empty;
			if (FounderLevelEx.CheckHasFlag(founderLevel, 4))
			{
				key = "FOUNDERS_BOOSTER_GOLD";
			}
			else if (FounderLevelEx.CheckHasFlag(founderLevel, 2))
			{
				key = "FOUNDERS_BOOSTER_SILVER";
			}
			else if (FounderLevelEx.CheckHasFlag(founderLevel, 1))
			{
				key = "FOUNDERS_BOOSTER_BRONZE";
			}
			else
			{
				Debug.Log.ErrorFormat("FoundersBoosterGui.GetDescription - Unexpected founder level to translate:[{0}]", new object[]
				{
					founderLevel
				});
			}
			return Language.Get(key, TranslationContext.MainMenuGui);
		}

		public static int GetBonusPercentage(FounderLevel founderLevel, HMMHub hub)
		{
			FounderPackConfig founderPackConfig = hub.SharedConfigs.FounderPackConfig;
			if (FounderLevelEx.CheckHasFlag(founderLevel, 4))
			{
				return founderPackConfig.FounderPackGoldXpBonus;
			}
			if (FounderLevelEx.CheckHasFlag(founderLevel, 2))
			{
				return founderPackConfig.FounderPackSilverXpBonus;
			}
			if (FounderLevelEx.CheckHasFlag(founderLevel, 1))
			{
				return founderPackConfig.FounderPackBronzeXpBonus;
			}
			return 0;
		}

		public static void UpdateHMM2DDynamicSprite(FounderLevel founderLevel, HMMUI2DDynamicSprite founderBorder, FoundersBoosterGui.SpriteType spriteType)
		{
			if (founderLevel == null)
			{
				founderBorder.gameObject.SetActive(false);
				return;
			}
			founderBorder.SpriteName = FoundersBoosterGui.GetSpriteName(founderLevel, spriteType);
			founderBorder.gameObject.SetActive(true);
		}

		public enum SpriteType
		{
			Corner,
			Circle,
			LoadingVersusIcon,
			LoadingVersusBox,
			MainMenuName,
			MainMenuBooster,
			MainMenuGroupIcon
		}
	}
}
