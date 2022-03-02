using System;

namespace HeavyMetalMachines.Matchmaking.Queue.Exceptions
{
	public class PlayerDeclinedMatchException : Exception
	{
		public PlayerDeclinedMatchException(string playerId) : base(string.Format("The player {0} declined the match.", playerId))
		{
			this.PlayerId = playerId;
		}

		public string PlayerId { get; private set; }
	}
}
