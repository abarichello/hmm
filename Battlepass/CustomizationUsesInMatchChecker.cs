using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets.ClientApiObjects;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Infra.Context;

namespace HeavyMetalMachines.Battlepass
{
	internal class CustomizationUsesInMatchChecker
	{
		public CustomizationUsesInMatchChecker()
		{
			Dictionary<Guid, Func<Guid, IPlayerStats, bool>> dictionary = new Dictionary<Guid, Func<Guid, IPlayerStats, bool>>();
			Dictionary<Guid, Func<Guid, IPlayerStats, bool>> dictionary2 = dictionary;
			Guid sprayCategoryGuid = InventoryMapper.SprayCategoryGuid;
			if (CustomizationUsesInMatchChecker.<>f__mg$cache0 == null)
			{
				CustomizationUsesInMatchChecker.<>f__mg$cache0 = new Func<Guid, IPlayerStats, bool>(CustomizationUsesInMatchChecker.HasUseTheItem);
			}
			dictionary2.Add(sprayCategoryGuid, CustomizationUsesInMatchChecker.<>f__mg$cache0);
			Dictionary<Guid, Func<Guid, IPlayerStats, bool>> dictionary3 = dictionary;
			Guid emoteCategoryGuid = InventoryMapper.EmoteCategoryGuid;
			if (CustomizationUsesInMatchChecker.<>f__mg$cache1 == null)
			{
				CustomizationUsesInMatchChecker.<>f__mg$cache1 = new Func<Guid, IPlayerStats, bool>(CustomizationUsesInMatchChecker.HasUseTheItem);
			}
			dictionary3.Add(emoteCategoryGuid, CustomizationUsesInMatchChecker.<>f__mg$cache1);
			dictionary.Add(InventoryMapper.VfxTakeOffCategoryGuid, (Guid id, IPlayerStats playerStats) => true);
			dictionary.Add(InventoryMapper.VfxScoreCategoryGuid, (Guid id, IPlayerStats playerStats) => playerStats.BombsDelivered > 0);
			dictionary.Add(InventoryMapper.VfxKillCategoryGuid, (Guid id, IPlayerStats playerStats) => playerStats.Kills > 0);
			dictionary.Add(InventoryMapper.VfxRespawnCategoryGuid, (Guid id, IPlayerStats playerStats) => playerStats.Deaths > 0);
			dictionary.Add(InventoryMapper.SkinsCategoryGuid, (Guid id, IPlayerStats playerStats) => true);
			dictionary.Add(InventoryMapper.PortraitsCategoryGuid, (Guid id, IPlayerStats playerStats) => true);
			this._itemCategoryToActionHandle = dictionary;
			base..ctor();
		}

		private static bool HasUseTheItem(Guid itemTypeGuid, IPlayerStats playerStats)
		{
			return playerStats.GetItemTypeUses(itemTypeGuid) > 0;
		}

		public bool HasUsesCustomizationInMatch(IItemType itemType, IPlayerStats playerStats)
		{
			return playerStats.Customizations.Contains(itemType.Id) && this._itemCategoryToActionHandle[itemType.ItemCategoryId](itemType.Id, playerStats);
		}

		private readonly Dictionary<Guid, Func<Guid, IPlayerStats, bool>> _itemCategoryToActionHandle;

		[CompilerGenerated]
		private static Func<Guid, IPlayerStats, bool> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<Guid, IPlayerStats, bool> <>f__mg$cache1;
	}
}
