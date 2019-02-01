using System;
using HeavyMetalMachines.Match;

namespace HeavyMetalMachines.Server.Pick.Rules.Apis
{
	public interface IBotCharacterSelector
	{
		int SelectCharacter(PlayerData bot);
	}
}
