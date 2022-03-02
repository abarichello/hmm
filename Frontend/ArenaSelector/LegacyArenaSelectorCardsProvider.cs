using System;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Matches.DataTransferObjects;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend.ArenaSelector
{
	public class LegacyArenaSelectorCardsProvider : GameHubObject, IArenaSelectorCardsProvider
	{
		public ArenaCardInfo[] Get(Sprite[] arenaSprites)
		{
			int numValidArenas = this.GetNumValidArenas();
			ArenaCardInfo[] array = new ArenaCardInfo[numValidArenas];
			int num = 0;
			int numberOfArenas = GameHubObject.Hub.ArenaConfig.GetNumberOfArenas();
			int i = 0;
			int num2 = 0;
			while (i < numberOfArenas)
			{
				IGameArenaInfo arenaByIndex = GameHubObject.Hub.ArenaConfig.GetArenaByIndex(i);
				if (this.IsValidArena(arenaByIndex))
				{
					array[num2].NameText = Language.Get(arenaByIndex.DraftName, TranslationContext.MainMenuGui);
					array[num2].ImageSprite = arenaSprites[i];
					array[num2].ArenaIndex = i;
					int num3 = 1;
					if (!GameHubObject.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish))
					{
						num3 = GameHubObject.Hub.User.GetTotalPlayerLevel();
						num3++;
					}
					int unlockLevel = arenaByIndex.UnlockLevel;
					array[num2].UnlockLevelText = unlockLevel.ToString("0");
					bool flag = num3 < unlockLevel;
					array[num2].Locked = flag;
					if (flag)
					{
						num++;
					}
					num2++;
				}
				i++;
			}
			if (numValidArenas == 1)
			{
				Array.Resize<ArenaCardInfo>(ref array, 3);
				array[1] = (array[2] = array[0]);
			}
			if (numValidArenas == 2)
			{
				Array.Resize<ArenaCardInfo>(ref array, 4);
				array[2] = array[0];
				array[3] = array[1];
			}
			return array;
		}

		private int GetNumValidArenas()
		{
			int num = 0;
			for (int i = 0; i < GameHubObject.Hub.ArenaConfig.GetNumberOfArenas(); i++)
			{
				if (this.IsValidArena(GameHubObject.Hub.ArenaConfig.GetArenaByIndex(i)))
				{
					num++;
				}
			}
			return num;
		}

		private bool IsValidArena(IGameArenaInfo arenaInfo)
		{
			MatchKind[] showInMatchKindsSelector = arenaInfo.ShowInMatchKindsSelector;
			if (showInMatchKindsSelector == null)
			{
				return false;
			}
			return Array.FindIndex<MatchKind>(showInMatchKindsSelector, (MatchKind x) => x == GameHubObject.Hub.Match.Kind) > -1;
		}
	}
}
