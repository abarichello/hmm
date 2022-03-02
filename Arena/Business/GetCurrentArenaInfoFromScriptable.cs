using System;
using Pocketverse;

namespace HeavyMetalMachines.Arena.Business
{
	public class GetCurrentArenaInfoFromScriptable : GameHubObject, IGetCurrentArenaInfo
	{
		public IGameArenaInfo Get()
		{
			return GameHubObject.Hub.ArenaConfig.GetCurrentArena();
		}
	}
}
