using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Server.Pick.Rules.Apis;

namespace HeavyMetalMachines.Server.Pick.Rules
{
	public class BotsCharacterFilter : ICharacterInfoFilter
	{
		public CharacterInfo[] FilterCharacters(IList<CharacterInfo> characters)
		{
			List<CharacterInfo> list = new List<CharacterInfo>();
			for (int i = 0; i < characters.Count; i++)
			{
				CharacterInfo characterInfo = characters[i];
				if (BotsCharacterFilter.IsCharacterValid(characterInfo))
				{
					list.Add(characterInfo);
				}
			}
			return list.ToArray();
		}

		private static bool IsCharacterValid(CharacterInfo character)
		{
			return character.IsAnAvailableBot && character.CanBePicked;
		}
	}
}
