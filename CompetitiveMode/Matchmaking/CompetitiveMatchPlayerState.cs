using System;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class CompetitiveMatchPlayerState
	{
		public override string ToString()
		{
			return string.Format("<Player={0} Afk={1}>", this.PlayerId, this.Afk);
		}

		public long PlayerId;

		public bool Afk;
	}
}
