using System;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Server.Pick.Rules.Apis
{
	public interface ICharacterInfoFilter
	{
		IItemType[] FilterCharacters(IItemType[] characters);
	}
}
