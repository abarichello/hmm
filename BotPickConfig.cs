using System;
using HeavyMetalMachines.Character;

namespace HeavyMetalMachines
{
	[Serializable]
	public class BotPickConfig
	{
		public CharacterInfo.DriverRoleKind[] roles = new CharacterInfo.DriverRoleKind[4];
	}
}
