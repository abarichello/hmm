using System;
using System.Collections.Generic;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.CharacterSelection.Server.Skins;

namespace HeavyMetalMachines.CharacterSelection.Server.Swordfish
{
	public class SkipSwordfishEquippedSkinsProvider : IEquippedSkinsProvider
	{
		public List<MatchClientEquippedSkins> Get()
		{
			return new List<MatchClientEquippedSkins>();
		}
	}
}
