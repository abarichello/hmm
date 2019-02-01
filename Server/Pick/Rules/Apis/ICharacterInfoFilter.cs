using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;

namespace HeavyMetalMachines.Server.Pick.Rules.Apis
{
	public interface ICharacterInfoFilter
	{
		CharacterInfo[] FilterCharacters(IList<CharacterInfo> characters);
	}
}
