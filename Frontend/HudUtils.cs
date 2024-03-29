﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using ClientAPI.Objects;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish.Player;
using HeavyMetalMachines.Utils;
using Hoplon.Serialization;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public static class HudUtils
	{
		public static string GetColoredString(Color color, string baseString)
		{
			return string.Format("[{0}]{1}[-]", HudUtils.RGBToHex(color), baseString);
		}

		public static string GetTagColoredEncoded(string teamTag, Color textColor)
		{
			return string.Format("[{0}][[-][{0}]{1}[-][{0}]][-]", HudUtils.RGBToHex(textColor), NGUIText.EscapeSymbols(teamTag));
		}

		public static string GetPlayerIconName(HMMHub hub, Guid characterItemTypeId, HudUtils.PlayerIconSize size)
		{
			CharacterItemTypeComponent characterComponentFromGuid = HudUtils.GetCharacterComponentFromGuid(hub, characterItemTypeId);
			if (size == HudUtils.PlayerIconSize.Size64)
			{
				return characterComponentFromGuid.CharacterIcon64Name;
			}
			if (size != HudUtils.PlayerIconSize.Size128)
			{
				return "Generic_icon_char_64";
			}
			return characterComponentFromGuid.Round128LookRightIconName;
		}

		public static string GetHudWinnerPlayertName(HMMHub hub, Guid customizationSkin)
		{
			SkinPrefabItemTypeComponent skinComponentFromGuid = HudUtils.GetSkinComponentFromGuid(hub, customizationSkin);
			return skinComponentFromGuid.HudWinnerName;
		}

		public static string GetPlayerWinnerPortraitName(HMMHub hub, Guid customizationSkin)
		{
			SkinPrefabItemTypeComponent skinComponentFromGuid = HudUtils.GetSkinComponentFromGuid(hub, customizationSkin);
			return skinComponentFromGuid.PortraitVictoryName;
		}

		public static string GetGenericPlayerCarIconName(HMMHub hub)
		{
			return "Generic_default_car";
		}

		public static string GetPlayerPixelArtIconName(HMMHub hub, Guid characterItemTypeID)
		{
			CharacterItemTypeComponent characterComponentFromGuid = HudUtils.GetCharacterComponentFromGuid(hub, characterItemTypeID);
			return characterComponentFromGuid.PixelArtName;
		}

		public static string GetGadgetIconName(string name, GadgetSlot gadgetSlot)
		{
			string arg = string.Empty;
			switch (gadgetSlot)
			{
			case GadgetSlot.CustomGadget0:
			case GadgetSlot.CustomGadget1:
			case GadgetSlot.CustomGadget2:
			{
				int num = (int)gadgetSlot;
				arg = num.ToString("00");
				break;
			}
			case GadgetSlot.BoostGadget:
				arg = "Nitro";
				break;
			case GadgetSlot.PassiveGadget:
				arg = "Passive";
				break;
			default:
				Debug.Assert(false, "Invalid gadget to load icon: " + gadgetSlot, Debug.TargetTeam.All);
				return string.Empty;
			}
			return string.Format("{0}_{1}{2}", name, "Gadget", arg);
		}

		public static string GetGadgetIconName(PlayerData playerData, GadgetSlot gadgetSlot)
		{
			return HudUtils.GetGadgetIconName(playerData.GetCharacterAssetPrefix(), gadgetSlot);
		}

		public static string GetGadgetIconNameB(PlayerData playerData, GadgetSlot gadgetSlot)
		{
			return HudUtils.GetGadgetIconName(playerData, gadgetSlot) + "B";
		}

		public static string GetGadgetUpgradeIconName(string name, GadgetSlot gadgetSlot, GadgetInfo gadgetInfo, UpgradeInfo upgradeInfo, bool bigIcon = false)
		{
			UpgradeInfo[] upgrades = gadgetInfo.Upgrades;
			int i = 0;
			while (i < upgrades.Length)
			{
				UpgradeInfo upgradeInfo2 = upgrades[i];
				if (upgradeInfo2.Name == upgradeInfo.Name)
				{
					int num = (int)gadgetSlot;
					string text = num.ToString("00");
					string text2 = (i + 1).ToString("00");
					if (gadgetSlot == GadgetSlot.GenericGadget)
					{
						return string.Format("Gadget_Generic{0}", text2);
					}
					return string.Format("{0}_{1}{2}_{3}{4}{5}", new object[]
					{
						name,
						"Gadget",
						text,
						"Upgrade",
						text2,
						(!bigIcon) ? string.Empty : "B"
					});
				}
				else
				{
					i++;
				}
			}
			return string.Empty;
		}

		public static string GetInstanceIconName(string assetName, UpgradeInfo instanceUpgradeInfo, bool smallAsset)
		{
			string name = instanceUpgradeInfo.Name;
			string text = int.Parse(name.Substring(name.Length - 2)).ToString("00");
			return string.Format("{0}_{1}{2}{3}", new object[]
			{
				assetName,
				"instance",
				text,
				(!smallAsset) ? "B" : string.Empty
			});
		}

		public static string RGBToHex(Color color)
		{
			float num = color.r * 255f;
			float num2 = color.g * 255f;
			float num3 = color.b * 255f;
			string hex = HudUtils.GetHex(Mathf.FloorToInt(num / 16f));
			string hex2 = HudUtils.GetHex(Mathf.RoundToInt(num) % 16);
			string hex3 = HudUtils.GetHex(Mathf.FloorToInt(num2 / 16f));
			string hex4 = HudUtils.GetHex(Mathf.RoundToInt(num2) % 16);
			string hex5 = HudUtils.GetHex(Mathf.FloorToInt(num3 / 16f));
			string hex6 = HudUtils.GetHex(Mathf.RoundToInt(num3) % 16);
			return string.Concat(new string[]
			{
				hex,
				hex2,
				hex3,
				hex4,
				hex5,
				hex6
			});
		}

		private static string GetHex(int num)
		{
			return string.Empty + "0123456789ABCDEF"[num];
		}

		public static void ChangeLayersRecursively(Transform trans, int name)
		{
			IEnumerator enumerator = trans.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					Transform transform = (Transform)obj;
					transform.gameObject.layer = name;
					HudUtils.ChangeLayersRecursively(transform, name);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		public static string GetCarSkinSpriteName(ICollectionScriptableObject inventoryCollection, Guid charItemTypeId, Guid customizationSkin)
		{
			ItemTypeScriptableObject skinItemTypeScriptableObjectByGuid = inventoryCollection.GetSkinItemTypeScriptableObjectByGuid(charItemTypeId, customizationSkin);
			SkinPrefabItemTypeComponent component = skinItemTypeScriptableObjectByGuid.GetComponent<SkinPrefabItemTypeComponent>();
			return component.SkinSpriteName;
		}

		public static bool TryToGetCharacterBag(HMMHub hub, Guid itemTypeId, out CharacterBag characterBag)
		{
			Character[] characters = hub.User.Characters;
			for (int i = 0; i < characters.Length; i++)
			{
				CharacterBag characterBag2 = (CharacterBag)((JsonSerializeable<!0>)characters[i].Bag);
				if (characterBag2 == null || !(characterBag2.CharacterId != itemTypeId))
				{
					characterBag = characterBag2;
					return true;
				}
			}
			characterBag = null;
			return false;
		}

		public static float GetNormalizedLevelInfo(ProgressionInfo progressioninfo, int characterLevel, int characterXp)
		{
			int overallXPForLevel = progressioninfo.GetOverallXPForLevel(characterLevel);
			float num = (float)(characterXp - overallXPForLevel);
			int overallXPForLevel2 = progressioninfo.GetOverallXPForLevel(characterLevel + 1);
			float num2 = (float)(overallXPForLevel2 - overallXPForLevel);
			return (num2 > 0f) ? (num / num2) : 1f;
		}

		public static bool TryToGetPlayerUnlockRewardIconSpriteName(ProgressionInfo progressionInfo, int level, out string spriteName)
		{
			if (level >= progressionInfo.TotalLevels)
			{
				spriteName = string.Empty;
				return false;
			}
			ProgressionInfo.Level level2 = progressionInfo.Levels[level];
			return HudUtils.TryToGetPlayerUnlockRewardIconSpriteName(level2.Kind, out spriteName);
		}

		public static bool TryToGetPlayerUnlockRewardIconSpriteName(ProgressionInfo.RewardKind kind, out string spriteName)
		{
			switch (kind)
			{
			case 1:
				spriteName = "Soft_currency_unlock";
				break;
			case 2:
				spriteName = "CharacterSlot_unlock";
				break;
			case 3:
				spriteName = "CharacterSlot_unlock";
				break;
			default:
				spriteName = string.Empty;
				return false;
			}
			return true;
		}

		public static bool TryToGetUnlockRewardName(ProgressionInfo progressionInfo, int level, out string name)
		{
			if (level >= progressionInfo.TotalLevels)
			{
				name = string.Empty;
				return false;
			}
			ProgressionInfo.Level level2 = progressionInfo.Levels[level];
			return HudUtils.TryToGetUnlockRewardName(level2.Kind, level2.Value, out name);
		}

		public static bool TryToGetUnlockRewardName(ProgressionInfo.RewardKind kind, string value, out string name)
		{
			switch (kind)
			{
			case 1:
				name = "PROFILE_REWARD_SOFT_CURRENCY";
				break;
			case 2:
				name = "PROFILE_REWARD_SKIN_UNLOCK";
				break;
			case 3:
				name = "PROFILE_REWARD_SKIN_UNLOCK";
				break;
			default:
				name = string.Empty;
				return false;
			}
			name = Language.GetFormatted(name, TranslationContext.Profile, new object[]
			{
				value
			});
			return true;
		}

		private static CharacterItemTypeComponent GetCharacterComponentFromGuid(HMMHub hub, Guid characterGuid)
		{
			ItemTypeScriptableObject itemTypeScriptableObject;
			if (hub.InventoryColletion.AllItemTypes.TryGetValue(characterGuid, out itemTypeScriptableObject))
			{
				return itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
			}
			return null;
		}

		private static SkinPrefabItemTypeComponent GetSkinComponentFromGuid(HMMHub hub, Guid skinGuid)
		{
			ItemTypeScriptableObject itemTypeScriptableObject;
			if (hub.InventoryColletion.AllItemTypes.TryGetValue(skinGuid, out itemTypeScriptableObject))
			{
				return itemTypeScriptableObject.GetComponent<SkinPrefabItemTypeComponent>();
			}
			return null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudUtils));

		public static float MillisToSeconds = 0.001f;

		public enum PlayerIconSize
		{
			Size179 = 179,
			Size128 = 128,
			Size64 = 64
		}

		public class PlayerDataComparer : IComparer<PlayerData>
		{
			public PlayerDataComparer(int currentPlayerInstanceId, HudUtils.PlayerDataComparer.PlayerDataComparerType type)
			{
				this._currentPlayerInstanceId = currentPlayerInstanceId;
				this._playerDataComparerType = type;
			}

			public int Compare(PlayerData pdX, PlayerData pdY)
			{
				switch (this._playerDataComparerType)
				{
				case HudUtils.PlayerDataComparer.PlayerDataComparerType.InstanceId:
				{
					int instanceId = pdX.PlayerCarId.GetInstanceId();
					int instanceId2 = pdY.PlayerCarId.GetInstanceId();
					if (instanceId == this._currentPlayerInstanceId)
					{
						return 1;
					}
					if (instanceId2 == this._currentPlayerInstanceId)
					{
						return -1;
					}
					return instanceId - instanceId2;
				}
				case HudUtils.PlayerDataComparer.PlayerDataComparerType.CurrentPlayerFirst:
				{
					int num = (!pdX.IsCurrentPlayer) ? pdX.GridIndex : -1;
					int value = (!pdY.IsCurrentPlayer) ? pdY.GridIndex : -1;
					return num.CompareTo(value);
				}
				}
				return pdX.GridIndex.CompareTo(pdY.GridIndex);
			}

			private int _currentPlayerInstanceId;

			private readonly HudUtils.PlayerDataComparer.PlayerDataComparerType _playerDataComparerType;

			public enum PlayerDataComparerType
			{
				InstanceId,
				GridPosition,
				CurrentPlayerFirst
			}
		}
	}
}
