using System;
using Pocketverse;

namespace HeavyMetalMachines.Arena.Infra
{
	public class GameArenaConfigProviderFromHub : GameHubObject, IGameArenaConfigProvider
	{
		public IGameArenaConfig GameArenaConfig
		{
			get
			{
				return GameHubObject.Hub.ArenaConfig;
			}
		}
	}
}
