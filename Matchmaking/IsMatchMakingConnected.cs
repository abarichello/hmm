using System;
using ClientAPI.MessageHub;

namespace HeavyMetalMachines.MatchMaking
{
	public class IsMatchMakingConnected : IIsMatchMakingConnected
	{
		public IsMatchMakingConnected(AbstractHubClient hubClient)
		{
			this._hubClient = hubClient;
		}

		public bool IsConnected()
		{
			return this._hubClient.IsConnected;
		}

		private readonly AbstractHubClient _hubClient;
	}
}
