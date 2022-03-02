using System;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	public class MatchmakingStartedArgs
	{
		public string Host { get; set; }

		public int Port { get; set; }

		public Guid MatchId { get; set; }
	}
}
