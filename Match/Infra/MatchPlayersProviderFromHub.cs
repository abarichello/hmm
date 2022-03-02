using System;
using Pocketverse;

namespace HeavyMetalMachines.Match.Infra
{
	public class MatchPlayersProviderFromHub : GameHubObject, IMatchPlayersProvider
	{
		public IMatchPlayers GetMatchPlayers
		{
			get
			{
				return GameHubObject.Hub.Players;
			}
		}
	}
}
