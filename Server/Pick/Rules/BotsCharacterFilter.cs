using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Server.Pick.Rules.Apis;

namespace HeavyMetalMachines.Server.Pick.Rules
{
	public class BotsCharacterFilter : ICharacterInfoFilter
	{
		public IItemType[] FilterCharacters(IItemType[] characters)
		{
			List<IItemType> list = new List<IItemType>();
			foreach (IItemType itemType in characters)
			{
				if (BotsCharacterFilter.IsCharacterValid(itemType))
				{
					list.Add(itemType);
				}
			}
			return list.ToArray();
		}

		private static bool IsCharacterValid(IItemType charItemType)
		{
			CharacterItemTypeComponent component = charItemType.GetComponent<CharacterItemTypeComponent>();
			BotItemTypeComponent component2 = charItemType.GetComponent<BotItemTypeComponent>();
			return component2 != null && component2.IsAnAvailableBot && component.CanBePicked;
		}
	}
}
