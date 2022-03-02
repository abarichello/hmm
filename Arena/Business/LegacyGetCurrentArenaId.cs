using System;
using Pocketverse;

namespace HeavyMetalMachines.Arena.Business
{
	public class LegacyGetCurrentArenaId : GameHubObject, IGetCurrentArenaId
	{
		public int Get()
		{
			return GameHubObject.Hub.Match.ArenaIndex;
		}
	}
}
