using System;
using Pocketverse.Util;

namespace HeavyMetalMachines.CompetitiveMode.Matchmaking
{
	public class CompetitiveMatchResult
	{
		public override string ToString()
		{
			return string.Format("[Id={0} Season={1} Ranked={2} Winner={3} Loser={4}]", new object[]
			{
				this.MatchId,
				this.SeasonId,
				this.IsRankedMatch,
				Arrays.ToStringWithComma(this.WinnerTeam),
				Arrays.ToStringWithComma(this.LoserTeam)
			});
		}

		public string MatchId;

		public long SeasonId;

		public bool IsRankedMatch;

		public CompetitiveMatchPlayerState[] WinnerTeam;

		public CompetitiveMatchPlayerState[] LoserTeam;
	}
}
